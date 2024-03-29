﻿using MFiles.VAF.Configuration;
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
	[PreviewableTextEditor
	(
		PreviewTemplate = "Every {0}, {1}, and {2}",
		PreviewSources = new string[]
		{
			"._children{ .key == '" + nameof(TimeSpanEx.Hours) + "' }",
			"._children{ .key == '" + nameof(TimeSpanEx.Minutes) + "' }",
			"._children{ .key == '" + nameof(TimeSpanEx.Seconds) + "' }"
		},
		PreviewUnsetTexts = new string[] { "0 hours", "0 minutes", "0 seconds" },
		PreviewValueFormats = new string[] { "{0} hour(s)", "{0} minute(s)", "{0} seconds(s)" }
	)]
	[UsesConfigurationResources]
	public class TimeSpanEx
		: IRecurrenceConfiguration
	{
		private int hours;
		private int minutes;
		private int seconds;

		[DataMember]
		[JsonConfIntegerEditor
				(
					Label = ResourceMarker.Id + nameof(Resources.Configuration.TimeSpanEx_Interval_Hours),
					Min = 0
				)]
		public int Hours
		{
			get => hours;
			set => hours = value > 0
				? value
				: 0;
		}

		[DataMember]
		[JsonConfIntegerEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.TimeSpanEx_Interval_Minutes),
			Min = 0
		)]
		public int Minutes
		{
			get => minutes;
			set => minutes = value > 0
				? value
				: 0;
		}

		[DataMember]
		[JsonConfIntegerEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.TimeSpanEx_Interval_Seconds),
			Min = 0
		)]
		public int Seconds
		{
			get => seconds;
			set => seconds = value > 0
				? value
				: 0;
		}

		/// <summary>
		/// The interval, made up of the <see cref="Hours"/>, <see cref="Minutes"/>, and <see cref="Seconds"/> values.
		/// </summary>
		public TimeSpan GetInterval()
			=> new TimeSpan(this.Hours, this.Minutes, this.Seconds);

		/// <summary>
		/// Sets the interval to the associated <see cref="Hours"/>, <see cref="Minutes"/>, and <see cref="Seconds"/> members.
		/// </summary>
		/// <param name="interval"></param>
		public void SetInterval(TimeSpan interval)
		{
			if (interval == null || interval <= TimeSpan.Zero)
				interval = TimeSpan.Zero;
			this.Hours = interval.Hours;
			this.Minutes = interval.Minutes;
			this.Seconds = interval.Seconds;
		}

		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.General_RunOnVaultStart_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.General_RunOnVaultStart_HelpText),
			DefaultValue = true
		)]
		public bool? RunOnVaultStartup { get; set; } = true;

		public TimeSpanEx() { }
		public TimeSpanEx(TimeSpan timeSpan, bool? runOnVaultStartup = null)
			: this()
		{
			this.SetInterval(timeSpan);
			if (runOnVaultStartup.HasValue)
				this.RunOnVaultStartup = runOnVaultStartup.Value;
		}

		public DateTimeOffset? GetNextExecution(DateTimeOffset? after = null)
		{
			return (after?.ToUniversalTime() ?? DateTimeOffset.UtcNow).Add(this.GetInterval());
		}

		public string ToDashboardDisplayString()
		{
			// Sanity.
			var interval = this.GetInterval();
			if (null == interval || interval <= TimeSpan.Zero)
				return $"<p>{Resources.AsynchronousOperations.RepeatType_Interval_NoTimeSpanSpecified.EscapeXmlForDashboard()}<br /></p>";

			// Does it run on startup?
			var displayString = (this.RunOnVaultStartup.HasValue && this.RunOnVaultStartup.Value)
				? Resources.Time.RepeatsOnInterval_RunsOnStartup.EscapeXmlForDashboard(interval.ToDisplayString())
				: Resources.Time.RepeatsOnInterval_DoesNotRunOnStartup.EscapeXmlForDashboard(interval.ToDisplayString());

			// Build a text representation.
			return $"<p>{displayString}<br /></p>";
		}

		public static implicit operator TimeSpan(TimeSpanEx input)
		{
			return input?.GetInterval() ?? TimeSpan.Zero;
		}

		public static implicit operator TimeSpanEx(TimeSpan input)
		{
			var timeSpanEx = new TimeSpanEx();
			timeSpanEx.SetInterval(input);
			return timeSpanEx;
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
					{
						var timeSpanEx = new TimeSpanEx();
						timeSpanEx.SetInterval(TimeSpan.Parse(reader.Value?.ToString()));
						return timeSpanEx;
					}
				case JsonToken.StartObject:

					// Set up the output.
					var output = new TimeSpanEx();

					// Read the JSON data.
					var jObject = JToken.ReadFrom(reader);
					var children = jObject.Children().ToList();

					// Use the standard deserializer.
					serializer.Populate(jObject.CreateReader(), output);

					// If it was an old interval-style then parse that.
					{
						var intervalProperty = children
							.Where(t => t is JProperty)
							.Cast<JProperty>()
							.FirstOrDefault(p => p.Name == "Interval");
						if(null != intervalProperty && TimeSpan.TryParse(intervalProperty.Value?.ToString(), out TimeSpan interval))
						{
							output.Hours = interval.Hours;
							output.Minutes = interval.Minutes;
							output.Seconds = interval.Seconds;
						}
					}

					// Return the output.
					return output;
			}
			return null;
		}
	}
}