using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.ScheduledExecution;
using System;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	public class CustomizableRecurrence
		: IRecurrenceConfiguration
	{
		/// <summary>
		/// The currently-configured type of recurrance.
		/// </summary>
		[DataMember]
		[JsonConfEditor(DefaultValue = RecurrenceType.Interval)]
		public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Interval;

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

		public DateTime? GetNextExecution(DateTime? after = null)
		{
			switch (this.RecurrenceType)
			{
				case RecurrenceType.Interval:
					return (after ?? DateTime.UtcNow).Add(this.Interval);
				case RecurrenceType.Schedule:
					return this.Schedule?.GetNextExecution(after);
				default:
					throw new InvalidOperationException($"Recurrance type of {this.RecurrenceType} is not supported.");
			}
		}

		public string ToDashboardDisplayString()
		{
			switch (this.RecurrenceType)
			{
				case RecurrenceType.Interval:
					return this.Interval.ToDashboardDisplayString();
				case RecurrenceType.Schedule:
					return this.Schedule?.ToDashboardDisplayString();
				default:
					throw new InvalidOperationException($"Recurrance type of {this.RecurrenceType} is not supported.");
			}
		}

		public static implicit operator CustomizableRecurrence(TimeSpan interval)
		{
			return new CustomizableRecurrence()
			{
				RecurrenceType = RecurrenceType.Interval,
				Interval = interval
			};
		}

		public static implicit operator CustomizableRecurrence(Schedule schedule)
		{
			return new CustomizableRecurrence()
			{
				RecurrenceType = RecurrenceType.Schedule,
				Schedule = schedule
			};
		}
	}
}