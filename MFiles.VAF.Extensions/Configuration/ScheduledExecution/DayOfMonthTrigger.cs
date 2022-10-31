using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	[UsesConfigurationResources]
	public enum UnrepresentableDateHandling
	{
		/// <summary>
		/// Skips any dates that are unrepresentable.
		/// </summary>
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_UnrepresentableDateHandling_Skip))]
		Skip = 0,

		/// <summary>
		/// Attempts to use the last day of the same month.
		/// </summary>
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_UnrepresentableDateHandling_LastDayOfMonth))]
		LastDayOfMonth = 1
	}
	/// <summary>
	/// Represents a trigger that runs on specifically-numbered days of the month
	/// (e.g. 1st, 5th, 12th).
	/// </summary>
	[UsesConfigurationResources]
	public class DayOfMonthTrigger
		: DailyTrigger
	{
		/// <summary>
		/// How to handle unrepresentable dates (e.g. 30th February).
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_UnrepresentableDateHandling_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_UnrepresentableDateHandling_HelpText)
		)]
		public UnrepresentableDateHandling UnrepresentableDateHandling { get; set; }
			= UnrepresentableDateHandling.Skip;

		/// <summary>
		/// The days of the month to trigger the schedule.
		/// Days outside of a valid range (e.g. 30th February, or 99th October) are handled
		/// as per <see cref="UnrepresentableDateHandling"/>.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_TriggerDays_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_TriggerDays_HelpText),
			ChildName = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_TriggerDays_ChildName)
		)]
		public List<int> TriggerDays { get; set; } = new List<int>();

		/// <summary>
		/// Creates a <see cref="DayOfMonthTrigger"/> instance.
		/// </summary>
		public DayOfMonthTrigger()
		{
			base.Type = ScheduleTriggerType.Monthly;
		}

		/// <inheritdoc />
		public override DateTimeOffset? GetNextExecution(DateTimeOffset? after = null, TimeZoneInfo timeZoneInfo = null)
		{
			// Sanity.
			if (
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				||
				(null == this.TriggerDays || 0 == this.TriggerDays.Count)
				)
				return null;

			// When should we start looking?
			after = (after ?? DateTime.UtcNow).ToUniversalTime();

			// Convert the time into the timezone we're after.
			after = TimeZoneInfo.ConvertTime(after.Value, timeZoneInfo ?? TimeZoneInfo.Local);

			// Get the times to run, filtered to those in the future.
			return this.TriggerDays
				.SelectMany
				(
					d => GetNextDayOfMonth(after.Value, d, this.UnrepresentableDateHandling)
				)
				.Select
				(
					d => new DailyTrigger() { Type = ScheduleTriggerType.Daily, TriggerTimes = this.TriggerTimes }
						.GetNextExecutionIncludingNextDay(d, timeZoneInfo, false)
				)
				.Where(d => d > after.Value)
				.Select(d => d.Value)
				.OrderBy(d => d)
				.Select(d => d.ToUniversalTime())
				.FirstOrDefault();
		}

		/// <summary>
		/// Helper method to return the next potential run date within a schedule.
		/// </summary>
		/// <param name="after">The day to start looking on.</param>
		/// <param name="dayOfMonth">The day of the month to return.</param>
		/// <returns>
		/// If <paramref name="after"/> is the same day as <paramref name="dayOfWeek"/> then will return
		/// two items - one for today and one for the same day month.
		/// If not then it will return one item - for the next time that this day occurs
		/// (later this month or next, depending on parameters).
		/// </returns>
		internal static IEnumerable<DateTimeOffset> GetNextDayOfMonth
		(
			DateTimeOffset after, 
			int dayOfMonth,
			UnrepresentableDateHandling unrepresentableDateHandling
		)
		{
			// If the day of the month is invalid then return no values.
			if (dayOfMonth < 1 || dayOfMonth > 32)
				yield break;

			// Switch logic depending on the current day.
			if (dayOfMonth == after.Day)
			{
				// It's today.
				// We could be running today or the same day next month (depending on trigger times).
				// Return both options.
				yield return after;

				yield return
					new DateTimeOffset(after.Year, after.Month, 1, 0, 0, 0, after.Offset)
						.AddMonths(1) // One month
						.AddDays(dayOfMonth - 1); // Move forward to the correct day.
			}
			else if (dayOfMonth < after.Day)
			{
				// This day has already passed.
				// Return the correct day next month.
				yield return new DateTimeOffset(after.Year, after.Month, dayOfMonth, 0, 0, 0, after.Offset)
					.AddMonths(1);
			}
			else
			{
				// Day is in the future this month.
				var sanity = 0;
				var month = after.Month;
				while (sanity++ < 6)
				{
					DateTimeOffset? date = null;
					try
					{
						// Can we represent this date?
						// If not then we've asked for 30th Feb or similar.
						date = new DateTimeOffset(after.Year, month, dayOfMonth, 0, 0, 0, 0, after.Offset);
					}
					catch
					{
						// What should we do?
						switch (unrepresentableDateHandling)
						{
							case UnrepresentableDateHandling.LastDayOfMonth:
								// Get the last day of this month instead.
								date = new DateTimeOffset(after.Year, month, 1, 0, 0, 0, after.Offset)
									.AddMonths(1)
									.AddDays(-1);
								break;
							default:
								// Allow it to try the next month.
								date = null;
								month++;
								break;
						}
					}

					// If we can represent it then return it, otherwise move to next month.
					if (date.HasValue)
					{
						yield return date.Value;
						break;
					}
				}
			}
		}

		/// <inheritdoc />
		public override string ToString(TriggerTimeType triggerTimeType, TimeZoneInfo customTimeZone)
		{
			// Sanity.
			if (null == this.TriggerDays || this.TriggerDays.Count == 0)
				return null;
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
			return Resources.Schedule.Triggers_DayOfMonthTrigger.EscapeXmlForDashboard
				(
					string.Join(", ", this.TriggerDays.OrderBy(t => t)),
					times
				);
		}
	}
}