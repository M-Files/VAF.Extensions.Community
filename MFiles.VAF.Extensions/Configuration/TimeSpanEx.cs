using MFiles.VAF.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	[JsonConverter(typeof(TimeSpanExJsonConverter))]
	public class TimeSpanEx
		: IRecurrenceConfiguration
	{
		[DataMember]
		[JsonConfEditor(TypeEditor = "time")]
		public TimeSpan Interval { get; set; }

		[DataMember]
		[JsonConfEditor
		(
			Label = "Run on vault start",
			HelpText = "If true, runs when the vault starts.  If false, the first run is scheduled to be after the interval has elapsed.",
			DefaultValue = true
		)]
		public bool RunOnVaultStartup { get; set; } = true;

		public DateTime? GetNextExecution(DateTime? after = null)
		{
			return (after?.ToUniversalTime() ?? DateTime.UtcNow).Add(this.Interval);
		}

		public string ToDashboardDisplayString()
		{
			// Sanity.
			if (null == this?.Interval || this.Interval <= TimeSpan.Zero)
				return "<p>No timespan specified; does not repeat.<br /></p>";

			var prefix = "<p>Runs";
			if (this.RunOnVaultStartup)
				prefix += " on vault startup and";
			var suffix = ".<br /></p>";

			// Seconds be easy.
			if (this.Interval <= TimeSpan.FromSeconds(120))
				return $"{prefix} every {(int)this.Interval.TotalSeconds} seconds{suffix}";

			// Build a text representation
			var components = new List<string>();
			if (this.Interval.Days > 0)
				components.Add($"{this.Interval.Days} day{(this.Interval.Days != 1 ? "s" : "")}");
			if (this.Interval.Hours > 0)
				components.Add($"{this.Interval.Hours} hour{(this.Interval.Hours != 1 ? "s" : "")}");
			if (this.Interval.Minutes > 0)
				components.Add($"{this.Interval.Minutes} minute{(this.Interval.Minutes != 1 ? "s" : "")}");
			if (this.Interval.Seconds > 0)
				components.Add($"{this.Interval.Seconds} second{(this.Interval.Seconds != 1 ? "s" : "")}");

			// Build a text representation
			var output = prefix + " every ";
			for (var i = 0; i < components.Count; i++)
			{
				if (i == 0)
				{
					output += components[i];
				}
				else if (i == components.Count - 1)
				{
					output += ", and " + components[i];
				}
				else
				{
					output += ", " + components[i];
				}
			}
			return output + suffix;
		}

		public static implicit operator TimeSpan(TimeSpanEx input)
		{
			return input?.Interval ?? TimeSpan.Zero;
		}

		public static implicit operator TimeSpanEx(TimeSpan input)
		{
			return new TimeSpanEx() { Interval = input };
		}
	}

	/// <summary>
	/// Controls serialisation/deserialisation of <see cref="TimeSpanEx"/>.
	/// This allows the system to additionally deserialize <see cref="TimeSpan"/> data to <see cref="TimeSpanEx"/>.
	/// </summary>
	internal class TimeSpanExJsonConverter
		: JsonConverter
	{
		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(TimeSpanEx);
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.String:
					return new TimeSpanEx() { Interval = TimeSpan.Parse(reader.Value?.ToString()) };
				case JsonToken.StartObject:

					// Set up the output.
					var output = new TimeSpanEx();

					// Populate the output.
					var jObject = JToken.ReadFrom(reader);
					serializer.Populate(jObject.CreateReader(), output);

					// Return the output.
					return output;
			}
			return null;
		}

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
				writer.WriteRawValue(JsonConvert.SerializeObject(memberValue, Formatting.Indented));

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
				writer.WriteRawValue(JsonConvert.SerializeObject(memberValue, Formatting.Indented));

			}

			// End the object.
			writer.WriteEndObject();

		}
	}
}