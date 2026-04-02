using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration.ScheduledExecution;
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
		/// Triggered on a specific date of the month, or the nth weekday of the month?
		/// If set to SpecificDate, show TriggerDays config.
		/// If set to VariableDate, show weekday and nthday
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_MonthlyTrigger_DayType_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_MonthlyTrigger_DayType_HelpText),
			DefaultValue = DayOfMonthTriggerType.SpecificDate
		)]
		public DayOfMonthTriggerType DayType { get; set; } = DayOfMonthTriggerType.SpecificDate;

		/// <summary>
		/// The days of the month to trigger the schedule.
		/// Days outside of a valid range (e.g. 30th February, or 99th October) are handled
		/// as per <see cref="UnrepresentableDateHandling"/>.
		/// Only shown when DayType = SpecificDate.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_TriggerDays_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_TriggerDays_HelpText),
			ChildName = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_TriggerDays_ChildName),
			ShowWhen = ".parent._children{.key == 'DayType' && (!.value || .value != 'VariableDate') }"
		)]
		public List<int> TriggerDays { get; set; } = new List<int>();

		/// <summary>
		/// The Nth Weekday on which to trigger the schedule.
		/// Days outside of valid range (e.g. 7th Saturday of the Month) are handled
		/// as per <see cref="UnrepresentableDateHandling"/>.
		/// Only shown when DayType = VariableDate.
		/// </summary>
		[DataMember]
		[JsonConfEditor(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_NthDay_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_NthDay_HelpText),
			ShowWhen = ".parent._children{.key == 'DayType' && .value == 'VariableDate' }"
		)]
		public int NthDay { get; set; }

		/// <summary>
		/// The weekday on which to trigger the schedule.
		/// Days outside of valid range (e.g. 7th Saturday of the Month) are handled
		/// as per <see cref="UnrepresentableDateHandling"/>.
		/// Only shown when DayType = VariableDate.
		/// </summary>
		[DataMember]
		[JsonConfEditor(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DayOfMonthTrigger_Weekday_Label),
			ShowWhen = ".parent._children{.key == 'DayType' && .value == 'VariableDate' }"
		)]
		public DayOfWeek Weekday { get; set; }


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
				(this.DayType == DayOfMonthTriggerType.SpecificDate && (null == this.TriggerDays || 0 == this.TriggerDays.Count))
				)
				return null;

			// Use local timezone as default timezone
			timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Local;

			// When should we start looking?
			after = (after ?? DateTime.UtcNow).ToUniversalTime();

			// Convert the time into the timezone we're after.
			after = TimeZoneInfo.ConvertTime(after.Value, timeZoneInfo);

			// Get the times to run, filtered to those in the future.
			// Specific Date - iterate over trigger days.
			if(this.DayType == DayOfMonthTriggerType.SpecificDate)
			{
				return this.TriggerDays
				.SelectMany
				(
					d => GetNextDayOfMonth(after.Value, d, this.UnrepresentableDateHandling, this.DayType, this.NthDay, this.Weekday)
				)
				.Select(d => new DateTimeOffset(d.DateTime, timeZoneInfo.GetUtcOffset(d.DateTime)))
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
			// Variable Date - do not iterate over trigger days.
			else
			{
				return GetNextDayOfMonth(after.Value, -1, this.UnrepresentableDateHandling, this.DayType, this.NthDay, this.Weekday)
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
			UnrepresentableDateHandling unrepresentableDateHandling,
			DayOfMonthTriggerType triggerType = DayOfMonthTriggerType.SpecificDate,
			int nthWeekday = -1,
			DayOfWeek dayOfWeek = DayOfWeek.Sunday
		)
		{
			// If variable date, determine the dayOfMonth.
			if (triggerType == DayOfMonthTriggerType.VariableDate)
			{
				// If the nthWeekday is invalid, return no values.
				if (nthWeekday < 1 || nthWeekday > 5)
				{
					yield break;
				}
				// get the desired date for THIS month.
				int thisMonth_Date = GetVariableDateOfMonth(after, nthWeekday, dayOfWeek);
				// get the desired date for NEXT runtime (next month or otherwise, depending on UnrepresentableDateHandling).
				DateTimeOffset? nextRuntime_Date = GetNextMonthWithValidVariableDate(after, unrepresentableDateHandling, nthWeekday, dayOfWeek);
				if (!nextRuntime_Date.HasValue)
				{
					// return no values.
					yield break;
				}
				// If thisMonth_Date == -1, determine how to handle via UnrepresentableDateHandling
				if (thisMonth_Date == -1)
				{
					if (unrepresentableDateHandling == UnrepresentableDateHandling.LastDayOfMonth)
					{
						DateTimeOffset thisMonth_RunDate = new DateTimeOffset(after.Year, after.Month, 1, 0, 0, 0, after.Offset)
									.AddMonths(1)
									.AddDays(-1);
						yield return thisMonth_RunDate;
						// If this is TODAY, return the next run also.
						if (thisMonth_RunDate.Day == after.Day)
						{
							yield return nextRuntime_Date.Value;
						}
					}
					else
					{
						// Skip this month entirely. Return next runtime.
						yield return nextRuntime_Date.Value;
					}
				}
				// if thisMonth_Date = today, then return it AND return the next run next month.
				else if (thisMonth_Date == after.Day)
				{
					yield return after;
					yield return nextRuntime_Date.Value;

				}
				// if thisMonth_Date is later this month, return it.
				else if (thisMonth_Date > after.Day)
				{
					yield return new DateTimeOffset(after.Year, after.Month, thisMonth_Date, 0, 0, 0, after.Offset);
				}
				else
				{
					// Return only next month's run.
					yield return nextRuntime_Date.Value;
				}
			}
			else {
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

		}
		/// <summary>
		/// Gets the Nth Weekday of the month specified by the date.
		/// If its determined that the Nth weekday of the specified month does not exist, returns -1.
		/// </summary>
		/// <param name="date"></param>
		/// <param name="nthWeekday"></param>
		/// <param name="dayOfWeek"></param>
		/// <returns></returns>
		internal static int GetVariableDateOfMonth
		(
			DateTimeOffset date,
			int nthWeekday,
			DayOfWeek dayOfWeek
		)
		{
			DateTimeOffset curDateTimeOS = new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset);
			// Get the first dayOfWeek of the month.
			while(curDateTimeOS.DayOfWeek != dayOfWeek)
			{
				curDateTimeOS = curDateTimeOS.AddDays(1);
			}
			// Add (nthWeekday - 1) * 7 days.
			curDateTimeOS = curDateTimeOS.AddDays((nthWeekday - 1) * 7);
			// Is this date outside of the specific month? (e.g. the 5th Saturday of November does not exist)/
			if(date.Month != curDateTimeOS.Month)
			{
				return -1;
			}
			else
			{
				return curDateTimeOS.Day; 
			}
		}
		/// <summary>
		/// Helper to find the next valid runtime beyond current after.Month.
		/// </summary>
		/// <param name="after"></param>
		/// <param name="unrepresentableDateHandling"></param>
		/// <param name="nthWeekday"></param>
		/// <param name="dayOfWeek"></param>
		/// <returns></returns>
		internal static DateTimeOffset? GetNextMonthWithValidVariableDate(DateTimeOffset after, UnrepresentableDateHandling unrepresentableDateHandling, int nthWeekday, DayOfWeek dayOfWeek)
		{
			DateTimeOffset? returnValue = null;
			// Get next runtime.
			if(nthWeekday < 1 || nthWeekday > 5)
			{
				return null;
			}
			var sanity = 0;
			// Guarenteed to find a 1-5th weekday within a year. 
			while (sanity++ < 13)
			{
				DateTimeOffset? date = after.AddMonths(sanity);
				// Attempt to get the next month's run date.
				int runDate = GetVariableDateOfMonth(date.Value, nthWeekday, dayOfWeek);
				if (runDate == -1)
				{
					if (unrepresentableDateHandling == UnrepresentableDateHandling.LastDayOfMonth)
					{
						// Returns the next month's last 
						returnValue = new DateTimeOffset(date.Value.Year, date.Value.Month, 1, 0, 0, 0, after.Offset)
										.AddMonths(1)
										.AddDays(-1);
						break;
					}
					else
					{
						// Try the next month.
						date = null;
					}
					
				}
				else
				{
					// Valid date.
					date = new DateTimeOffset(date.Value.Year, date.Value.Month, 1, 0, 0, 0, date.Value.Offset)
											.AddDays(runDate - 1);
				}
				// If we can represent it then return it, otherwise move to next month.
				if (date.HasValue)
				{
					returnValue =  date.Value;
					break;
				}
				}
			// Will always return a value.
			return returnValue.Value;
		}

		/// <inheritdoc />
		public override string ToString(TriggerTimeType triggerTimeType, TimeZoneInfo customTimeZone)
		{
			// Sanity.
			if (this.DayType == DayOfMonthTriggerType.SpecificDate && (null == this.TriggerDays || this.TriggerDays.Count == 0))
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
			if (this.DayType == DayOfMonthTriggerType.VariableDate)
			{
				return Resources.Schedule.Triggers_DayOfMonthTrigger_VariableDate.EscapeXmlForDashboard
				(
					System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName != "en" ? this.NthDay.ToString() : 
					(this.NthDay == 1 ? "1st" : (this.NthDay == 2 ? "2nd": (this.NthDay == 3 ? "3rd": (this.NthDay > 0 && this.NthDay < 6 ? this.NthDay.ToString() + "th": this.NthDay.ToString())))),
					this.Weekday,
					times
				);
			}
			// specific date.
			else
			{
				return Resources.Schedule.Triggers_DayOfMonthTrigger_SpecificDate.EscapeXmlForDashboard
				(
					string.Join(", ", this.TriggerDays.OrderBy(t => t)),
					times
				);
			}
			
		}
	}
}