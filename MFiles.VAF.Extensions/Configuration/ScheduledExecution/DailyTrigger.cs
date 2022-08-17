using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	/// <summary>
	/// Represents a trigger that runs every day, potentially multiple times per day.
	/// </summary>
	[DataContract]
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
		public override DateTimeOffset? GetNextExecution(DateTime? after = null, TimeZoneInfo timeZoneInfo = null)
		{
			// Sanity.
			if (
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				)
				return null;

			// When should we start looking?
			timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Local;

			// Get the next execution time.
			return this.TriggerTimes
				.Select
				(
					t =>
					{
						// Convert the "after" time to the time in the given timezone.
						after = TimeZoneInfo.ConvertTimeFromUtc((after ?? DateTime.UtcNow).ToUniversalTime(), timeZoneInfo);

						// Set up the next execution time which is at midnight in the correct timezone.
						var output = new DateTimeOffset(after.Value.Date, timeZoneInfo.GetUtcOffset(after.Value.Date));

						if (after.Value.TimeOfDay <= t)
							output = output.Add(t); // Time is yet to come today (or is now).
						else
							output = output.AddDays(1).Add(t); // Time has passed - return tomorrow.
						return output;
					}
				)
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