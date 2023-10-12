using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	public abstract class JsonConverterBase
		: JsonConverter
	{
		protected virtual BindingFlags GetBindingFlags()
			=> BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

		protected virtual IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags? bindingFlags = null)
			=> type == null 
			? Array.Empty<PropertyInfo>()
			: type.GetProperties(bindingFlags ?? this.GetBindingFlags())
				.Union
				(
					(((bindingFlags ?? this.GetBindingFlags()) & BindingFlags.DeclaredOnly) > 0)
					? this.GetProperties(type.BaseType)
					: Array.Empty<PropertyInfo>()
				);

		protected virtual IEnumerable<FieldInfo> GetFields(Type type, BindingFlags? bindingFlags = null)
			=> type == null
			? Array.Empty<FieldInfo>()
			: type.GetFields(bindingFlags ?? this.GetBindingFlags())
				.Union
				(
					(((bindingFlags ?? this.GetBindingFlags()) & BindingFlags.DeclaredOnly) > 0)
					? this.GetFields(type.BaseType)
					: Array.Empty<FieldInfo>()
				);

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			System.Diagnostics.Debugger.Launch();
			if (value == null)
				return;
			var converter = new NewtonsoftJsonConvert();

			var valueType = value.GetType();

			// Start the object.
			writer.WriteStartObject();

			var outputProperties = new HashSet<string>();

			// Output any properties.
			foreach (var p in this.GetProperties(valueType))
			{
				Debug.WriteLine($"Property: {p.Name}");
				if(p.Name == "UsesAdvancedConfiguration")
				{
					System.Diagnostics.Debugger.Break();
				} 
				
				// Skip null values.
				var memberValue = p.GetValue(value);
				if (null == memberValue || default == memberValue)
					continue;

				// Skip items that are already output.
				if (outputProperties.Contains(p.Name))
					continue;
				outputProperties.Add(p.Name);

				// Only process data members.
				var dataMemberAttribute = p.GetCustomAttributes(typeof(DataMemberAttribute), true).FirstOrDefault() as DataMemberAttribute;
				if (null == dataMemberAttribute)
					continue;
				if (p.GetCustomAttribute<JsonIgnoreAttribute>() != null)
					continue;

				// What should this be called?
				var name = string.IsNullOrWhiteSpace(dataMemberAttribute.Name)
					? p.Name
					: dataMemberAttribute.Name;

				// Get the value.
				var v = converter.Serialize(memberValue);

				// Add it to the object.
				if (null != v && v != "{}")
				{
					writer.WritePropertyName(name);
					writer.WriteRawValue(v);
				}

			}

			// Output any fields.
			foreach (var f in this.GetFields(valueType))
			{

				// Skip null values.
				var memberValue = f.GetValue(value);
				if (null == memberValue || default == memberValue)
					continue;

				// Skip items that are already output.
				if (outputProperties.Contains(f.Name))
					continue;
				outputProperties.Add(f.Name);

				// Only process data members.
				var dataMemberAttribute = f.GetCustomAttributes(typeof(DataMemberAttribute), true).FirstOrDefault() as DataMemberAttribute;
				if (null == dataMemberAttribute)
					continue;
				if (f.GetCustomAttribute<JsonIgnoreAttribute>() != null)
					continue;

				// What should this be called?
				var name = string.IsNullOrWhiteSpace(dataMemberAttribute.Name)
					? f.Name
					: dataMemberAttribute.Name;

				// Get the value.
				var v = converter.Serialize(memberValue);

				// Add it to the object.
				if (null != v && v != "{}")
				{
					writer.WritePropertyName(name);
					writer.WriteRawValue(v);
				}

			}

			// End the object.
			writer.WriteEndObject();

		}
	}
}