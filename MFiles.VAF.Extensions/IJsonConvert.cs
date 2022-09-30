using MFiles.VAF.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

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
		internal class JsonConfEditorAwareValueProvider
			: IValueProvider
		{
			private MemberInfo memberInfo;
			private IValueProvider valueProvider;
			public JsonConfEditorAwareValueProvider(MemberInfo memberInfo, IValueProvider valueProvider)
			{
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
		/// that replaces dynamic value providers with instances of <see cref="DynamicValueProviderEx"/>.
		/// We do this so that we can take into account any default values specified by [JsonConfEditor] attributes.
		/// </summary>
		internal class JsonConfEditorDefaultValueContractResolver
			: DefaultContractResolver
		{
			/// <inheritdoc />
			/// <remarks>
			/// Wraps the value provider returned by the base implementation with a
			/// <see cref="JsonConfEditorAwareValueProvider"/> and returns that instead.
			/// </remarks>
			protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
				=> new JsonConfEditorAwareValueProvider
				(
					member, 
					base.CreateMemberValueProvider(member)
				);
		}

		/// <summary>
		/// The default json serialization settings to use.
		/// </summary>
		public static Newtonsoft.Json.JsonSerializerSettings DefaultJsonSerializerSettings { get; }
			= new Newtonsoft.Json.JsonSerializerSettings()
			{
				DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
				Formatting = Newtonsoft.Json.Formatting.Indented,
				ContractResolver = new JsonConfEditorDefaultValueContractResolver()
			};

		/// <summary>
		/// The json serialization settings to use with this instance.
		/// </summary>
		public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; }
			= DefaultJsonSerializerSettings;

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
