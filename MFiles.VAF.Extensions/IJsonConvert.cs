using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

		/// <summary>
		/// Serializes <paramref name="input"/>.
		/// </summary>
		/// <param name="t">The type to serialize from.</typeparam>
		/// <param name="input">The object to deserialize.</param>
		/// <returns>The instance.</returns>
		string Serialize(object input, Type t);
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

					// If the property has no default value, try to get the default of that type.
					try
					{
						if (null == defaultValue)
						{
							if (this.memberInfo is FieldInfo fieldInfo)
								defaultValue = Activator.CreateInstance(fieldInfo.FieldType);
							if (this.memberInfo is PropertyInfo propertyInfo)
							{
								defaultValue = Activator.CreateInstance(propertyInfo.PropertyType);
							}
						};
					}
					catch { } // Nope.

					// If the data is the same as the default value then do not serialize.
					if (type == typeof(string) && string.Equals(defaultValue, value))
						return null;
					if (defaultValue == value)
						return null;
					var serializedValue = this.JsonConvert.Serialize(value ?? "{}");
					if (serializedValue == "{}" 
						|| this.JsonConvert.Serialize(defaultValue) == serializedValue)
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
			private ILogger Logger { get; } = LogManager.GetLogger(typeof(DefaultValueAwareContractResolver));
			protected IJsonConvert JsonConvert { get; set; }

			public DefaultValueAwareContractResolver(IJsonConvert jsonConvert)
			{
				this.JsonConvert = jsonConvert
					?? throw new ArgumentNullException(nameof(jsonConvert));
			}

			protected virtual MemberInfo IdentifyLatestImplementation(Type objectType, MemberInfo memberInfo)
			{
				// Sanity.
				if (null == objectType)
					throw new ArgumentNullException(nameof(objectType));
				if (null == memberInfo)
					throw new ArgumentNullException(nameof(memberInfo));

				// The member info may apply to multiple fields/properties
				// if the field or property has been overridden or similar.
				if (memberInfo is FieldInfo)
				{
					// If it's directly on this type then return it.
					// If not, and there is a concrete base type, try to find it there.
					// If not then we're at the bottom of the pile so return it anywhere.
					return objectType.GetField(memberInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
						?? (objectType.BaseType != null && !objectType.BaseType.IsAbstract
						? this.IdentifyLatestImplementation(objectType?.BaseType, memberInfo)
						: objectType.GetField(memberInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
				}
				else if (memberInfo is PropertyInfo)
				{
					// If it's directly on this type then return it.
					// If not, and there is a concrete base type, try to find it there.
					// If not then we're at the bottom of the pile so return it anywhere.
					return objectType.GetProperty(memberInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
						?? (objectType.BaseType != null && !objectType.BaseType.IsAbstract
						? this.IdentifyLatestImplementation(objectType?.BaseType, memberInfo)
						: objectType.GetProperty(memberInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
				}
				else
					return memberInfo;
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
				var dict = new Dictionary<string, MemberInfo>();

				// The base implementation does all the retrieval of data, so utilise that.
				var baseMembers = base.GetSerializableMembers(objectType).GroupBy(m => m.Name);
				foreach (var g in baseMembers)
				{
					MemberInfo memberInfo = null;
					switch (g.Count())
					{
						case 0:
							// WTF?
							continue;
						default:
							{
								// Only one?  Fine.
								memberInfo = g.FirstOrDefault();
								var orig = memberInfo;
								if (null == memberInfo)
									continue;

								// If the member is not of the correct type then it's inherited.
								// Get the one from the actual type we cared about.
								memberInfo = this.IdentifyLatestImplementation(objectType, memberInfo);
								break;
							}
					}

					// Add the member information.
					if(null != memberInfo)
						dict.Add(memberInfo.Name, memberInfo);
				}

				return dict.Values.ToList();
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
		/// Gets the default json serialization settings to use.
		/// </summary>
		public virtual JsonSerializerSettings GetDefaultJsonSerializerSettings()
		{
			return new JsonSerializerSettings()
			{
				DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include,
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
				Formatting = Newtonsoft.Json.Formatting.Indented,
				Converters = new List<JsonConverter>()
					{
						new StringEnumConverter()
					},
				ContractResolver = new DefaultValueAwareContractResolver(this)
			};
		}

		/// <summary>
		/// The json serialization settings to use with this instance.
		/// </summary>
		public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; }

		public NewtonsoftJsonConvert()
		{
			this.JsonSerializerSettings = this.GetDefaultJsonSerializerSettings();
		}

		/// <inheritdoc />
		public T Deserialize<T>(string input)
			=> Deserialize<T>(input, null);

		/// <inheritdoc />
		public object Deserialize(string input, Type type)
			=> Deserialize(input, type, null);

		public object Deserialize(string input, Type type, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject(input, type, settings ?? this.JsonSerializerSettings);

		public T Deserialize<T>(string input, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input, settings ?? this.JsonSerializerSettings);

		/// <inheritdoc />
		public string Serialize<T>(T input)
			=> Serialize(input, typeof(T), null);

		/// <inheritdoc />
		public string Serialize(object input, Type t)
			=> Serialize(input, t, null);

		public string Serialize(object input, Type t, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.SerializeObject(input, t, settings ?? this.JsonSerializerSettings);
	}
}
