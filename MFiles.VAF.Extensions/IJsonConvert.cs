using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VaultApplications.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	public interface IJsonConvert
	{
		/// <summary>
		/// Deserializes <paramref name="input"/> into an instance of <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialize to.</typeparam>
		/// <param name="input">The serialized version.</param>
		/// <returns>The instance.</returns>
		T Deserialize<T>(string input);

		/// <summary>
		/// Deserializes <paramref name="input"/> to an instance of <paramref name="type"/>.
		/// </summary>
		/// <param name="input">The serialized version.</param>
		/// <param name="type">The type to deserialize to.</param>
		/// <returns>The instance.</returns>
		object Deserialize(string input, Type type);

		/// <summary>
		/// Serializes <paramref name="input"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialize from.</typeparam>
		/// <param name="input">The object to deserialize.</param>
		/// <returns>The instance.</returns>
		string Serialize<T>(T input);
	}

	/// <summary>
	/// An implementation of <see cref="IJsonConvert"/>
	/// that delegates to Newtonsoft.
	/// </summary>
	internal class NewtonsoftJsonConvert : IJsonConvert
	{
		internal class DefaultValueAwareValueProvider
			: IValueProvider
		{
			protected IJsonConvert JsonConvert { get; set; }
			private ILogger Logger { get; } = LogManager.GetLogger(typeof(DefaultValueAwareValueProvider));
			private MemberInfo memberInfo;
			private IValueProvider valueProvider;
			public DefaultValueAwareValueProvider(IJsonConvert jsonConvert, MemberInfo memberInfo, IValueProvider valueProvider)
			{
				this.JsonConvert = jsonConvert
					?? throw new ArgumentNullException(nameof(jsonConvert));
				this.memberInfo = memberInfo
					?? throw new ArgumentNullException(nameof(memberInfo));
				this.valueProvider = valueProvider;
			}

			/// <inheritdoc />
			public void SetValue(object target, object value)
				=> this.valueProvider.SetValue(target, value);

			/// <inheritdoc />
			/// <remarks>
			/// If the member has a [JsonConfEditor] attribute then the default value defined there is compared
			/// against the current value and, if they are the same, the value is returned as <see langword="null"/>
			/// (effectively causing the property to not be rendered to the saved configuration).
			/// </remarks>
			public object GetValue(object target)
			{
				// Get the value.
				var value = this.valueProvider.GetValue(target);
				if (null == value)
					return null;

				// If it is the version string then always output it.
				{
					var dataMemberAttribute = this.memberInfo.GetCustomAttribute<DataMemberAttribute>();
					if(null != dataMemberAttribute 
						&& this.memberInfo.DeclaringType == typeof(VersionedConfigurationBase)
						&& this.memberInfo.Name == nameof(VersionedConfigurationBase.VersionString))
					{
						return value;
					}
				}

				// Try to get the runtime default value.
				try
				{
					// Create an instance of this type.
					object defaultValue = null;
					var instance = Activator.CreateInstance(this.memberInfo.ReflectedType);
					Type type = null;
					switch(this.memberInfo.MemberType)
					{
						case MemberTypes.Field:
							{
								var fieldInfo = (FieldInfo)this.memberInfo;
								type = fieldInfo.FieldType;
								defaultValue = fieldInfo.GetValue(instance);
							break;
							}
						case MemberTypes.Property:
							{
								var propertyInfo = (PropertyInfo)this.memberInfo;
								type = propertyInfo.PropertyType;
								defaultValue = propertyInfo.GetValue(instance);
								break;
							}
					}

					// If the data is the same as the default value then do not serialize.
					if (type == typeof(string) && string.Equals(defaultValue, value))
						return null;
					if (defaultValue == value)
						return null;
					if (null != value
						&& this.JsonConvert.Serialize(defaultValue) == this.JsonConvert.Serialize(value))
						return null;
				}
				catch(Exception e)
				{
					this.Logger?.Warn(e, $"Could not identify default value for {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name}");
				}

				// If this member has a JsonConfEditorAttribute then we need to check whether to filter it.
				{
					var jsonConfEditorAttribute = this.memberInfo.GetCustomAttribute<JsonConfEditorAttribute>();
					if(null != jsonConfEditorAttribute && null != jsonConfEditorAttribute.DefaultValue)
					{
						// If it is the default then die now.
						if (value?.ToString() == jsonConfEditorAttribute.DefaultValue?.ToString())
							return null;

						// If it's the identifier then we need to check the alias/guid/id properties.
						if(value is MFIdentifier identifier && (
							identifier.Alias == jsonConfEditorAttribute.DefaultValue?.ToString()
							|| identifier.Guid == jsonConfEditorAttribute.DefaultValue?.ToString()
							|| identifier.ID.ToString() == jsonConfEditorAttribute.DefaultValue?.ToString()
							))
						{
							return null;
						}

					}
				}

				// Return the value that the base implementation gave.
				return value;
			}
		}

		/// <summary>
		/// An implementation of <see cref="DefaultContractResolver"/>
		/// that replaces value providers with instances of <see cref="DefaultValueAwareValueProvider"/>.
		/// We do this so that we can take into account any default values specified by [JsonConfEditor] attributes.
		/// </summary>
		internal class DefaultValueAwareContractResolver
			: DefaultContractResolver
		{
			protected IJsonConvert JsonConvert { get; set; }

			public DefaultValueAwareContractResolver(IJsonConvert jsonConvert)
			{
				this.JsonConvert = jsonConvert 
					?? throw new ArgumentNullException(nameof(jsonConvert));
			}

			/// <inheritdoc />
			/// <remarks>
			/// The default implementation returns memberinfo instances that have a ReflectedType
			/// pointing to where the member was declared.  Where abstract classes declare members
			/// we then don't have the data to instantiate the actual serialised class.
			/// So, for these ones, ensure that we have the correct reflected type.
			/// </remarks>
			protected override List<MemberInfo> GetSerializableMembers(Type objectType)
			{
				var list = new List<MemberInfo>();

				// The base implementation does all the retrieval of data, so utilise that.
				foreach (var memberInfo in base.GetSerializableMembers(objectType))
				{
					// If it was declared outside of this type then
					// re-request the member data ensuring that we use
					// the correct type.
					if(memberInfo.ReflectedType != objectType)
					{
						switch (memberInfo.MemberType)
						{
							case MemberTypes.Field:
								{
									list.Add(objectType.GetField(memberInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
									continue;
								}
							case MemberTypes.Property:
								{
									list.Add(objectType.GetProperty(memberInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
									continue;
								}
						}
					}

					// It was the correct type, or something other than a property or field.
					list.Add(memberInfo);
				}
				return list;
			}

			/// <inheritdoc />
			/// <remarks>
			/// Wraps the value provider returned by the base implementation with a
			/// <see cref="DefaultValueAwareValueProvider"/> and returns that instead.
			/// </remarks>
			protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
				=> new DefaultValueAwareValueProvider
				(
					this.JsonConvert,
					member, 
					base.CreateMemberValueProvider(member)
				);
		}

		/// <summary>
		/// The default json serialization settings to use.
		/// </summary>
		public static Newtonsoft.Json.JsonSerializerSettings DefaultJsonSerializerSettings { get; }

		/// <summary>
		/// The json serialization settings to use with this instance.
		/// </summary>
		public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; }

		public NewtonsoftJsonConvert()
		{
			this.JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
			{
				DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
				Formatting = Newtonsoft.Json.Formatting.Indented,
				ContractResolver = new DefaultValueAwareContractResolver(this)
			};
		}

		/// <inheritdoc />
		public T Deserialize<T>(string input)
			=> Deserialize<T>(input, null);

		/// <inheritdoc />
		public object Deserialize(string input, Type type)
			=> Deserialize(input, type, null);

		public object Deserialize(string input, Type type, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject(input, type, settings ?? this.JsonSerializerSettings ?? DefaultJsonSerializerSettings);

		public T Deserialize<T>(string input, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input, settings ?? this.JsonSerializerSettings ?? DefaultJsonSerializerSettings);

		/// <inheritdoc />
		public string Serialize<T>(T input)
			=> Serialize<T>(input, null);

		public string Serialize<T>(T input, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.SerializeObject(input, settings ?? this.JsonSerializerSettings ?? DefaultJsonSerializerSettings);
	}
}
