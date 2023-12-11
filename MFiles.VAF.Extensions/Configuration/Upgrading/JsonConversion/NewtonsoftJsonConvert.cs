using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.JsonAdaptor;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFilesAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// An implementation of <see cref="IJsonConvert"/>
	/// that delegates to Newtonsoft.
	/// </summary>
	public class NewtonsoftJsonConvert : IJsonConvert
	{
		public class DefaultValueAwareValueProvider
			: IValueProvider
		{
			protected IJsonConvert JsonConvert { get; set; }
			private ILogger Logger { get; } = LogManager.GetLogger(typeof(DefaultValueAwareValueProvider));
			private readonly MemberInfo memberInfo;
			private readonly IValueProvider valueProvider;
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

			public virtual bool ShouldRenderValue(ref object value)
			{
				// Sanity.
				if (null == value)
					return false;

				// If it is the version string then always output it.
				{
					var dataMemberAttribute = this.memberInfo.GetCustomAttribute<DataMemberAttribute>();
					if (null != dataMemberAttribute
						&& this.memberInfo.DeclaringType == typeof(VersionedConfigurationBase)
						&& this.memberInfo.Name == nameof(VersionedConfigurationBase.VersionString))
					{
						return true;
					}
				}

				// If this has the AllowDefaultValueSerializationAttribute attribute then always output it.
				{
					var allowDefaultAttribute = this.memberInfo.GetCustomAttribute<AllowDefaultValueSerializationAttribute>();
					if (null != allowDefaultAttribute)
						return true;
				}

				var jsonConfEditorAttribute = this.memberInfo.GetCustomAttribute<JsonConfEditorAttribute>();
				if(null != jsonConfEditorAttribute)
				{ 
					if (null != jsonConfEditorAttribute.DefaultValue)
					{
						// If it is the default then die now.
						if (value?.ToString() == jsonConfEditorAttribute.DefaultValue?.ToString())
						{
							this.Logger?.Trace($"Value of {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} was same as default value in JSONConfEditor ({jsonConfEditorAttribute.DefaultValue}) so not writing to JSON.");
							return false;
						}

						// If it's the identifier then we need to check the alias/guid/id properties.
						if (value is MFIdentifier identifier && (
							identifier.Alias == jsonConfEditorAttribute.DefaultValue?.ToString()
							|| identifier.Guid == jsonConfEditorAttribute.DefaultValue?.ToString()
							|| identifier.ID.ToString() == jsonConfEditorAttribute.DefaultValue?.ToString()
							))
						{
							this.Logger?.Trace($"Identifier at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} was same as default value in JSONConfEditor ({jsonConfEditorAttribute.DefaultValue}) so not writing to JSON.");
							return false;
						}
					}

					// If the configuration value has a JsonConfEditor attribute that defines an editor type
					// then we may need to convert our deserialized value.
					if (jsonConfEditorAttribute.TypeEditor == "date" && value is DateTime dateTime)
					{
						this.Logger?.Trace($"Value at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name}is a date, so removing time portion.");
						value = dateTime.ToString("yyyy-MM-dd");
					}

					// If it is required then give the current value.
					if (jsonConfEditorAttribute.IsRequired)
					{
						this.Logger?.Trace($"Value at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} is marked as required, so writing to JSON.");
						return true;
					}
				}

				// Try to get the runtime default value.
				Type type = null;
				try
				{
					// Create an instance of this type.
					object defaultValue = null;
					var instance = Activator.CreateInstance(this.memberInfo.ReflectedType);
					bool hasValueOptionsAttribute = false;
					switch (this.memberInfo.MemberType)
					{
						case MemberTypes.Field:
							{
								var fieldInfo = (FieldInfo)this.memberInfo;
								type = fieldInfo.FieldType;
								defaultValue = fieldInfo.GetValue(instance);
								hasValueOptionsAttribute = fieldInfo.GetCustomAttribute<ValueOptionsAttribute>() != null;
								break;
							}
						case MemberTypes.Property:
							{
								var propertyInfo = (PropertyInfo)this.memberInfo;
								type = propertyInfo.PropertyType;
								defaultValue = propertyInfo.GetValue(instance);
								hasValueOptionsAttribute = propertyInfo.GetCustomAttribute<ValueOptionsAttribute>() != null;
								break;
							}
					}

					// Is this type part of the skipped types?
					if (null != Extensions.JsonConvert.DefaultValueSkippedTypes)
					{
						foreach (var s in Extensions.JsonConvert.DefaultValueSkippedTypes)
						{
							// Sanity.
							if (null == s)
								continue;

							// Is it a prefix or an exact match?
							if (s.EndsWith("*"))
							{
								// It's a prefix.
								var prefix = s.Substring(0, s.Length - 1);
								if (type.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
									|| this.memberInfo.DeclaringType.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
								{
									this.Logger?.Trace($"Member at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} has a type prefix that is marked for skipping ({type.FullName} / {prefix}), so not writing to JSON.");
									return true;
								}
							}
							else
							{
								// Exact match.
								if (type.FullName.Equals(s, StringComparison.OrdinalIgnoreCase)
									|| this.memberInfo.DeclaringType.FullName.Equals(s, StringComparison.OrdinalIgnoreCase))
								{
									this.Logger?.Trace($"Member at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} has a type that is marked for skipping ({type.FullName}), so not writing to JSON.");
									return false;
								}
							}
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

					// If the type is an enum, but it has a ValueOptionsAttribute, then serialize as an integer.
					if (type.IsEnum && hasValueOptionsAttribute)
					{
						try
						{
							value = (int)value;
						}
						catch (Exception e)
						{
							// Could not convert to integer.
							this.Logger?.Warn(e, $"Could not convert {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} value to an integer ({value}).");
							return true;
						}
						try
						{
							defaultValue = (int)defaultValue;
						}
						catch (Exception e)
						{
							// Could not convert to integer.
							this.Logger?.Warn(e, $"Could not convert {type.FullName} default value to an integer ({defaultValue}).");
							return true;
						}
					}

					// If it's an MFIdentifier then force default values for non-allow-empty members.
					{
						if (type == typeof(MFIdentifier))
						{
							var vaultElementReferenceAttribute = this.memberInfo.GetCustomAttribute<VaultElementReferenceAttribute>();
							if (null != vaultElementReferenceAttribute && !vaultElementReferenceAttribute.AllowEmpty)
							{
								// We have a value, but we're not allowed empty values.
								// Return the value.
								this.Logger?.Trace($"Value at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} does not allow empty values, so writing to JSON.");
								return true;
							}
						}
					}

					// If the data is the same as the default value then do not serialize.
					if (type == typeof(string) && string.Equals(defaultValue, value))
					{
						this.Logger?.Trace($"Value at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} is an empty string, so not writing it to JSON.");
						return false;
					}
					if (defaultValue == value)
					{
						this.Logger?.Trace($"Value at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} is null, so not writing it to JSON.");
						return false;
					}

					try
					{
						var serializedValue = null == value ? "{}" : this.JsonConvert.Serialize(value);
						var serializedDefaultValue = null == defaultValue ? "{}" : this.JsonConvert.Serialize(defaultValue);

						if (serializedValue == "{}" || serializedDefaultValue == serializedValue)
						{
							this.Logger?.Trace($"Value at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} is empty, so not writing it to JSON.");
							return false;
						}
					}
					catch (Exception e)
					{
						this.Logger?.Warn(e, $"Could not create default serialised value for {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name}");
					}
				}
				catch (Exception e)
				{
					this.Logger?.Warn(e, $"Could not identify default value for {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name}");
				}

				// Return the value that the base implementation gave.
				return true;

			}
			public static bool ShouldRenderValue(ref object value, IJsonConvert jsonConvert, MemberInfo memberInfo)
			{
				var valueProvider = new DefaultValueAwareValueProvider(jsonConvert, memberInfo, null);
				return valueProvider.ShouldRenderValue(ref value);
			}

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
				return this.ShouldRenderValue(ref value)
					? value
					: null;
				this.Logger?.Trace($"Value at {this.memberInfo.ReflectedType.FullName}.{this.memberInfo.Name} is being written to JSON.");
				return value;
			}
		}

		/// <summary>
		/// A class to write enums as either integers or strings, depending on what's provided.
		/// </summary>
		internal class DateTimeConverter
			: Newtonsoft.Json.Converters.IsoDateTimeConverter
		{

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				// Sanity.
				if (null == writer
					|| null == value
					|| null == serializer)
					return;

				// If it's a string then write it.
				if(value.GetType() == typeof(string))
				{
					writer.WriteValue(value);
					return;
				}

				// Use the base implementation.
				base.WriteJson(writer, value, serializer);
			}
		}

		/// <summary>
		/// A class to write enums as either integers or strings, depending on what's provided.
		/// </summary>
		internal class StringEnumConverterHandlesIntegers 
			: StringEnumConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				// Sanity.
				if (null == writer 
					|| null == value
					|| null == serializer)
					return;

				// If it's an enum then write it as an enum.
				try
				{
					var enumValue = (Enum)value;
					base.WriteJson(writer, value, serializer);
					return;
				}
				catch
				{
					// If we can write integers then awesome.
					if (AllowIntegerValues && value.GetType() == typeof(int))
					{
						writer.WriteValue(value);
						return;
					}
				}
				throw new JsonSerializationException($"Value {value} ({value.GetType().FullName}) not supported by StringEnumConverterHandlesIntegers.");
			}
		}

		/// <summary>
		/// A converter that allows JSON for known types (<see cref="JsonConvert.LeaveJsonAloneTypes"/>)
		/// to be round-tripped through deserialization/serialization, rather than the deserialization/serialization
		/// process potentially affecting them.
		/// Used to ensure that classes that cannot be deserialized/serialized in .NET
		/// (such as <see cref="SearchConditionsJA"/>) are not changed.
		/// </summary>
		internal class LeaveJsonAloneConverter
			: JsonConverterBase
		{
			private ILogger Logger { get; } = LogManager.GetLogger(typeof(LeaveJsonAloneConverter));

			/// <summary>
			/// A collection of the data read from the raw JSON (so it can be written back as-is).
			/// The (deserialized) object instance is the key, and the JSON is the value.
			/// </summary>
			private static readonly Dictionary<object, string> RawJsonLookup
				= new Dictionary<object, string>();

			/// <inheritdoc />
			/// <remarks>Checks whether the type exists in <see cref="JsonConvert.LeaveJsonAloneTypes"/>.</remarks>
			public override bool CanConvert(Type objectType)
			{
				if (null == objectType)
					return false;
				this.Logger?.Trace($"Types to be left alone: {string.Join(", ", JsonConvert.LeaveJsonAloneTypes)}");

				// If the type is included in the "leave json alone" collection,
				// either explicitly or via a prefix match, then we will deal with it.
				var t = objectType.FullName;
				var b = JsonConvert.LeaveJsonAloneTypes.Any
				(
					s => string.Equals(s, t, StringComparison.OrdinalIgnoreCase)
						|| (t.Length > s.Length && s.EndsWith("*") && string.Equals(s.Substring(0, s.Length-1), t.Substring(0, s.Length-1)))
				);
				if (b)
				{
					this.Logger?.Debug($"Caching raw JSON for type {t} as it was found in the list of types to leave alone.");
				}
				else
				{
					this.Logger?.Trace($"Type {t} will be deserialized and serialized.");
				}
				return b;
			}

			/// <inheritdoc />
			/// <remarks>Allows standard deserialization, but caches the raw JSON for future reference.</remarks>
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				switch (reader.TokenType)
				{
					case JsonToken.None:
						// Try again.
						return reader.Read()
							? this.ReadJson(reader, objectType, existingValue, serializer)
							: default;
				}

				// Get the raw JSON.
				var rawJson = JRaw.Create(reader)?.ToString();
				if (null == rawJson)
				{
					this.Logger?.Warn($"Cannot cache data for {objectType.FullName} as value is null ({reader.Path}).");
					return null;
				}

				// Deserialize the JSON into an instance of the expected type.
				var instance = Newtonsoft.Json.JsonConvert.DeserializeObject
				(
					rawJson, 
					objectType, 
					serializer.Converters.Where(c => c != this).ToArray()
				);

				// Add the pair to the dictionary/cache and return the instance.
				this.Logger?.Trace($"Caching JSON for {objectType.FullName} at {reader.Path}.");
				RawJsonLookup.Add(instance, rawJson);
				return instance;
			}

			/// <inheritdoc />
			/// <remarks>If a cached version of the JSON exists then renders that instead,
			/// otherwise uses the default serialization.</remarks>
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if(null == value || !RawJsonLookup.ContainsKey(value))
				{
					this.Logger?.Warn($"No data found in cache for {writer.Path}.");

					// We have to deal with some types explicitly...
					{
						if (value is IMFJsonAdaptor<SearchConditions> a)
						{
							writer.WriteRawValue(a.ToJson());
							return;
						}
					}
					{
						if (value is IMFJsonAdaptor<SearchCondition> a)
						{
							writer.WriteRawValue(a.ToJson());
							return;
						}
					}

					// Use the basic approach.
					base.WriteJson(writer, value, serializer);
					return;
				}
				this.Logger?.Trace($"Cache retrieved for {value.GetType().FullName} at {writer.Path}.");
				writer.WriteRawValue(RawJsonLookup[value]);
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
			var defaults = Newtonsoft.Json.JsonConvert.DefaultSettings?.Invoke()
				?? new JsonSerializerSettings();
			defaults.NullValueHandling = NullValueHandling.Ignore;
			defaults.Converters.Add(new StringEnumConverterHandlesIntegers() { AllowIntegerValues = true });
			defaults.Converters.Add(new DateTimeConverter());
			defaults.Converters.Add(new LeaveJsonAloneConverter());
			defaults.ContractResolver = new DefaultValueAwareContractResolver(this);
			return defaults;
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
