using Newtonsoft.Json;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	public abstract class JsonConverterBase
		: JsonConverter
	{

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
				return;

			var valueType = value.GetType();

			// Start the object.
			writer.WriteStartObject();

			// Output any properties.
			foreach (var p in valueType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				// Skip null values.
				var memberValue = p.GetValue(value);
				if (null == memberValue)
					continue;

				// Only process data members.
				var dataMemberAttribute = p.GetCustomAttributes(typeof(DataMemberAttribute), true).FirstOrDefault() as DataMemberAttribute;
				if (null == dataMemberAttribute)
					continue;

				// What should this be called?
				var name = string.IsNullOrWhiteSpace(dataMemberAttribute.Name)
					? p.Name
					: dataMemberAttribute.Name;

				// Add it to the object.
				writer.WritePropertyName(name);
				writer.WriteRawValue(Newtonsoft.Json.JsonConvert.SerializeObject(memberValue, Formatting.Indented));

			}

			// Output any fields.
			foreach (var f in valueType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				// Skip null values.
				var memberValue = f.GetValue(value);
				if (null == memberValue)
					continue;

				// Only process data members.
				var dataMemberAttribute = f.GetCustomAttributes(typeof(DataMemberAttribute), true).FirstOrDefault() as DataMemberAttribute;
				if (null == dataMemberAttribute)
					continue;

				// What should this be called?
				var name = string.IsNullOrWhiteSpace(dataMemberAttribute.Name)
					? f.Name
					: dataMemberAttribute.Name;

				// Add it to the object.
				writer.WritePropertyName(name);
				writer.WriteRawValue(Newtonsoft.Json.JsonConvert.SerializeObject(memberValue, Formatting.Indented));

			}

			// End the object.
			writer.WriteEndObject();

		}
	}
}