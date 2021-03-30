using MFiles.VAF.Configuration;
using System;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.MultiServerMode.ScheduledExecution

{
	/// <summary>
	/// Class used for configuration purposes.
	/// </summary>
	[DataContract]
	public class Trigger
		: TriggerBase
	{
		/// <summary>
		/// The type of trigger this is (e.g. Daily, Weekly).
		/// </summary>
		[DataMember]
		public new ScheduleTriggerType Type
		{
			get => base.Type;
			protected set => base.Type = value;
		}

		[DataMember]
		[JsonConfEditor
		(
			Label = "Configuration",
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'Type' && .value == 'Daily' }"
		)]
		public DailyTrigger DailyTriggerConfiguration { get; set; }
			= new DailyTrigger();

		[DataMember]
		[JsonConfEditor
		(
			Label = "Configuration",
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'Type' && .value == 'Weekly' }"
		)]
		public WeeklyTrigger WeeklyTriggerConfiguration { get; set; }
			= new WeeklyTrigger();

		[DataMember]
		[JsonConfEditor
		(
			Label = "Configuration",
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'Type' && .value == 'Monthly' }"
		)]
		public DayOfMonthTrigger DayOfMonthTriggerConfiguration { get; set; }
			= new DayOfMonthTrigger();

		/// <inheritdoc />
		public override DateTime? GetNextExecution(DateTime? after = null)
		{
			switch (this.Type)
			{
				case ScheduleTriggerType.Daily:
					return this.DailyTriggerConfiguration?.GetNextExecution(after);
				case ScheduleTriggerType.Weekly:
					return this.WeeklyTriggerConfiguration?.GetNextExecution(after);
				case ScheduleTriggerType.Monthly:
					return this.DayOfMonthTriggerConfiguration?.GetNextExecution(after);
				default:
					return null;
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			switch (this.Type)
			{
				case ScheduleTriggerType.Daily:
					return this.DailyTriggerConfiguration?.ToString();
				case ScheduleTriggerType.Weekly:
					return this.WeeklyTriggerConfiguration?.ToString();
				case ScheduleTriggerType.Monthly:
					return this.DayOfMonthTriggerConfiguration?.ToString();
				default:
					return null;
			}
		}

		internal static Trigger FromTriggerBase(TriggerBase trigger)
		{
			switch (trigger.Type)
			{
				case ScheduleTriggerType.Daily:
					{
						return new Trigger()
						{
							Type = ScheduleTriggerType.Daily,
							DailyTriggerConfiguration = (trigger as DailyTrigger)
						};
					}
				case ScheduleTriggerType.Weekly:
					{
						return new Trigger()
						{
							Type = ScheduleTriggerType.Weekly,
							WeeklyTriggerConfiguration = (trigger as WeeklyTrigger)
						};
					}
				case ScheduleTriggerType.Monthly:
					{
						return new Trigger()
						{
							Type = ScheduleTriggerType.Monthly,
							DayOfMonthTriggerConfiguration = (trigger as DayOfMonthTrigger)
						};
					}
				case ScheduleTriggerType.Unknown:
				default:
					return null;
			}
		}
	}
	public abstract class TriggerBase
	{
		/// <summary>
		/// The type of trigger this is (e.g. Daily, Weekly).
		/// </summary>
		public ScheduleTriggerType Type { get; set; } = ScheduleTriggerType.Unknown;

		/// <summary>
		/// Gets the next execution datetime for this trigger.
		/// </summary>
		/// <param name="after">The time after which the schedule should run.  Defaults to now (i.e. next-run time) if not provided.</param>
		/// <returns>The next execution time.</returns>
		public abstract DateTime? GetNextExecution(DateTime? after = null);
	}
}