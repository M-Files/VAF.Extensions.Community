using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.ScheduledExecution;
using System;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	public class Frequency
		: IRecurrenceConfiguration
	{
		/// <summary>
		/// The currently-configured type of recurrance.
		/// </summary>
		[DataMember]
		[JsonConfEditor(Label = "Type")]
		public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Unknown;

		[DataMember]
		[JsonConfEditor
		(
			Label = "Configuration",
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'RecurrenceType' && .value == 'Interval' }",
			TypeEditor = "time"
		)]
		public TimeSpan Interval { get; set; }

		[DataMember]
		[JsonConfEditor
		(
			Label = "Configuration",
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'RecurrenceType' && .value == 'Schedule' }"
		)]
		public Schedule Schedule { get; set; }

		/// <inheritdoc />
		public DateTime? GetNextExecution(DateTime? after = null)
		{
			switch (this.RecurrenceType)
			{
				case RecurrenceType.Interval:
					return (after ?? DateTime.UtcNow).Add(this.Interval);
				case RecurrenceType.Schedule:
					return this.Schedule?.GetNextExecution(after);
				case RecurrenceType.Unknown:
					return null;
				default:
					throw new InvalidOperationException($"Recurrance type of {this.RecurrenceType} is not supported.");
			}
		}

		/// <inheritdoc />
		public string ToDashboardDisplayString()
		{
			switch (this.RecurrenceType)
			{
				case RecurrenceType.Interval:
					return this.Interval.ToDashboardDisplayString();
				case RecurrenceType.Schedule:
					return this.Schedule?.ToDashboardDisplayString();
				case RecurrenceType.Unknown:
					return ((TimeSpan?)null).ToDashboardDisplayString();
				default:
					throw new InvalidOperationException($"Recurrance type of {this.RecurrenceType} is not supported.");
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
}