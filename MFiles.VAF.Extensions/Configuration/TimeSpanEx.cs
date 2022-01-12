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
				return $"<p>{Resources.AsynchronousOperations.RepeatType_Interval_NoTimeSpanSpecified.EscapeXmlForDashboard()}<br /></p>";

			// Does it run on startup?
			var displayString = (this.RunOnVaultStartup.HasValue && this.RunOnVaultStartup.Value)
				? Resources.Time.RepeatsOnInterval_RunsOnStartup.EscapeXmlForDashboard(this.Interval.ToDisplayString())
				: Resources.Time.RepeatsOnInterval_DoesNotRunOnStartup.EscapeXmlForDashboard(this.Interval.ToDisplayString());

			// Build a text representation.
			return $"<p>{displayString}<br /></p>";
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
				case JsonToken.None:
					// Try again.
					return reader.Read()
						? this.ReadJson(reader, objectType, existingValue, serializer)
						: default;
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