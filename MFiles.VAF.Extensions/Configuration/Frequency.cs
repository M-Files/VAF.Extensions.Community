﻿using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.ScheduledExecution;
using MFiles.VaultApplications.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	[JsonConverter(typeof(FrequencyJsonConverter))]
	public class Frequency
		: IRecurrenceConfiguration
	{
		/// <summary>
		/// The currently-configured type of Recurrence.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Frequency_RecurrenceType_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Frequency_RecurrenceType_HelpText)
		)]
		public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Unknown;

		/// <inheritdoc />
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.General_Configuration),
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'RecurrenceType' && .value == 'Interval' }"
		)]
		public TimeSpanEx Interval { get; set; }

		/// <inheritdoc />
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.General_Configuration),
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'RecurrenceType' && .value == 'Schedule' }"
		)]
		public Schedule Schedule { get; set; }

		/// <inheritdoc />
		public bool? RunOnVaultStartup
		{
			get
			{
				switch (this.RecurrenceType)
				{
					case RecurrenceType.Interval:
						return this.Interval?.RunOnVaultStartup ?? false;
					case RecurrenceType.Schedule:
						if (false == (this.Schedule?.Enabled ?? false))
							return false;
						return this.Schedule.RunOnVaultStartup;
					default:
						return false;
				}
			}
		}

		/// <inheritdoc />
		public DateTimeOffset? GetNextExecution(DateTime? after = null)
		{
			switch (this.RecurrenceType)
			{
				case RecurrenceType.Interval:
					if (null == this.Interval)
						return null;
					return (after ?? DateTime.UtcNow).Add(this.Interval);
				case RecurrenceType.Schedule:
					return this.Schedule?.GetNextExecution(after);
				case RecurrenceType.Unknown:
					return null;
				default:
					throw new InvalidOperationException(String.Format(Resources.Exceptions.Configuration.RecurrenceTypeNotSupported, this.RecurrenceType));
			}
		}

		/// <inheritdoc />
		public string ToDashboardDisplayString()
		{
			switch (this.RecurrenceType)
			{
				case RecurrenceType.Interval:
					return this.Interval?.ToDashboardDisplayString();
				case RecurrenceType.Schedule:
					return this.Schedule?.ToDashboardDisplayString();
				case RecurrenceType.Unknown:
					return ((TimeSpan?)null).ToDashboardDisplayString();
				default:
					throw new InvalidOperationException(String.Format(Resources.Exceptions.Configuration.RecurrenceTypeNotSupported, this.RecurrenceType));
			}
		}

		/// <summary>
		/// Converts the <paramref name="interval"/> provided to a <see cref="Frequency"/>
		/// representing the interval.
		/// </summary>
		/// <param name="interval">The interval to represent.</param>
		public static implicit operator Frequency(TimeSpan interval)
		{
			return new Frequency()
			{
				RecurrenceType = RecurrenceType.Interval,
				Interval = interval
			};
		}

		/// <summary>
		/// Converts the <paramref name="schedule"/> provided to a <see cref="Frequency"/>
		/// representing the schedule.
		/// </summary>
		/// <param name="schedule">The schedule to represent.</param>
		public static implicit operator Frequency(Schedule schedule)
		{
			return new Frequency()
			{
				RecurrenceType = RecurrenceType.Schedule,
				Schedule = schedule
			};
		}
	}

	/// <summary>
	/// Controls serialisation/deserialisation of <see cref="Frequency"/>.
	/// This allows the system to additionally deserialize <see cref="TimeSpan"/> data to <see cref="Frequency"/>.
	/// </summary>
	internal class FrequencyJsonConverter
		: JsonConverterBase
	{
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(FrequencyJsonConverter));

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Frequency);
		}

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
					var timeSpanEx = JsonConvert.DeserializeObject<TimeSpanEx>($"\"{reader.Value?.ToString()}\"");
					return new Frequency() { Interval = timeSpanEx, RecurrenceType = RecurrenceType.Interval };
				case JsonToken.StartObject:

					// Set up the output.
					var output = new Frequency();

					// Populate the output.
					var jToken = JToken.ReadFrom(reader);

					// If it has a recurrence type then it's a frequency.
					if (jToken[nameof(Frequency.RecurrenceType)] != null)
					{
						serializer.Populate(jToken.CreateReader(), output);
						return output;
					}

					//Check if this might be a TimeSpanEx
					if (jToken[nameof(TimeSpanEx.Hours)] != null
						|| jToken[nameof(TimeSpanEx.Minutes)] != null
						|| jToken[nameof(TimeSpanEx.Seconds)] != null)
					{
						output.RecurrenceType = RecurrenceType.Interval;
						output.Interval = new TimeSpanEx();
						serializer.Populate(jToken.CreateReader(), output.Interval);
					}
					//Check if this might be a Schedule
					else if (jToken[nameof(Schedule.Triggers)] != null)
					{
						output.RecurrenceType = RecurrenceType.Schedule;
						output.Schedule = new Schedule();
						serializer.Populate(jToken.CreateReader(), output.Schedule);
					}
					else
					{
						this.Logger?.Warn
						(
							String.Format(Resources.Exceptions.Configuration.CouldNotConvertJsonValueToFrequency, jToken)
						);
						return default;
					}

					// Return the output.
					return output;
			}

			return null;
		}

	}
}