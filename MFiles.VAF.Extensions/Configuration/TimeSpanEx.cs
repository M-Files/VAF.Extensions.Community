using MFiles.VAF.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
		public bool? RunOnVaultStartup { get; set; } = true;

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
			if (this.RunOnVaultStartup.HasValue && this.RunOnVaultStartup.Value)
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
		: JsonConverterBase
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
	}
}