using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	/// <summary>
	/// Represents a trigger that runs every day, potentially multiple times per day.
	/// </summary>
	[DataContract]
	[UsesConfigurationResources]
	public class DailyTrigger
		: TriggerBase
	{
		/// <summary>
		/// The times of day to trigger the schedule.
		/// There must be at least one item in this collection for the trigger to be active.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DailyTrigger_TriggerTimes_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DailyTrigger_TriggerTimes_HelpText),
			ChildTypeEditor = "time"
		)]
		public List<TimeSpan> TriggerTimes { get; set; } = new List<TimeSpan>();

		/// <summary>
		/// Creates a <see cref="DailyTrigger"/> instance.
		/// </summary>
		public DailyTrigger()
		{
			base.Type = ScheduleTriggerType.Daily;
		}

		/// <inheritdoc />
		public override DateTimeOffset? GetNextExecution(DateTimeOffset? after = null, TimeZoneInfo timeZoneInfo = null)
			=> this.GetNextExecutionIncludingNextDay(after, timeZoneInfo, true);

		/// <summary>
		/// Gets the date of the next execution.
		/// </summary>
		/// <param name="after"></param>
		/// <param name="timeZoneInfo"></param>
		/// <param name="allowNextDay"></param>
		/// <returns></returns>
		public virtual DateTimeOffset? GetNextExecutionIncludingNextDay(DateTimeOffset? after = null, TimeZoneInfo timeZoneInfo = null, bool allowNextDay = true)
		{
			// Sanity.
			if (
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				)
				return null;

			// When should we start looking?
			timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Local;

			// Ensure we have a value.
			if (null == after)
				after = DateTimeOffset.UtcNow;

			// Get the next execution time.
			return this.TriggerTimes
				.Select<TimeSpan, DateTimeOffset?>
				(
					t =>
					{
						// What is the potential time that this will run?
						DateTimeOffset potential;
						{
							var dateTime = after.Value.Date.Add(t);
							potential = new DateTimeOffset(dateTime, timeZoneInfo.GetUtcOffset(dateTime));
						}

						// If the potential run time is in the future then we can return it.
						if (potential >= after.Value)
							return potential;

						// If we can't go to the next day then die.
						if (false == allowNextDay)
							return null;

						// The potential run time is in the past, so we need to work out the next day.

						// If the next day has gone to/from DST then we need to deal with it.
						var oldoffset = potential.Offset;
						potential = potential.AddDays(1);
						var newoffset = timeZoneInfo.GetUtcOffset(potential);
						if (oldoffset != newoffset)
						{
							potential = potential.Subtract(newoffset - oldoffset);
						}

						return potential;
					}
				)
				.Where(d => d != null)
				.Select(d => d.Value)
				.OrderBy(d => d)
				.Select(d => d.ToUniversalTime())
				.FirstOrDefault();
		}

		/// <inheritdoc />
		public virtual string ToString(TriggerTimeType triggerTimeType, TimeZoneInfo customTimeZone)
		{
			// Sanity.
			if (null == this.TriggerTimes || this.TriggerTimes.Count == 0)
				return null;

			// Append the time zone if we can.
			var times = string.Join(", ", this.TriggerTimes.OrderBy(t => t).Select(t => t.ToString()));
			if (customTimeZone != null)
				if (customTimeZone == TimeZoneInfo.Local)
					times += " (server time)";
				else if (customTimeZone == TimeZoneInfo.Utc)
					times += " (UTC)";
				else
					times += $" ({customTimeZone.DisplayName})";
			return Resources.Schedule.Triggers_DailyTrigger.EscapeXmlForDashboard(times);
		}

		/// <summary>
		/// Automatically converts <paramref name="trigger"/> to a <see cref="Trigger"/>.
		/// </summary>
		/// <param name="trigger">The trigger to convert.</param>
		public static implicit operator Trigger(DailyTrigger triggerConfiguration)
		{
			// Sanity.
			if (null == triggerConfiguration)
				throw new ArgumentNullException(nameof(triggerConfiguration));
			return new Trigger(triggerConfiguration);
		}

	}
}