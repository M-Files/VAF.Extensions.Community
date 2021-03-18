using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.MultiServerMode

{
	/// <summary>
	/// Represents a trigger that runs on specifically-numbered days of the month
	/// (e.g. 1st, 5th, 12th).
	/// </summary>
	public class DayOfMonthMonthlyTrigger
		: DailyTrigger
	{
		/// <summary>
		/// The days of the month to trigger the schedule.
		/// Days outside of a valid range (e.g. 30th February, or 99th October) are ignored.
		/// </summary>
		public List<int> TriggerDays { get; set; } = new List<int>();

		/// <inheritdoc />
		public override DateTime? GetNextExecutionTime(DateTime? after = null)
		{
			// Sanity.
			if (
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				||
				(null == this.TriggerDays || 0 == this.TriggerDays.Count)
				)
				return null;

			// When should we start looking?
			after = (after ?? DateTime.UtcNow).ToLocalTime();

			// Get the times to run, filtered to those in the future.
			return this.TriggerDays
				.SelectMany
				(
					d => GetNextDayOfMonth(after.Value, d)
				)
				.SelectMany(d => this.TriggerTimes.Select(t => d.Add(t)))
				.Where(d => d > after.Value)
				.OrderBy(d => d)
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
		internal static IEnumerable<DateTime> GetNextDayOfMonth(DateTime after, int dayOfMonth)
		{
			// Switch logic depending on the current day.
			if (dayOfMonth == after.Day)
			{
				// It's today.
				// We could be running today or the same day next month (depending on trigger times).
				// Return both options.
				return new[]
				{
					after.Date,
					new DateTime(after.Year, after.Month, 1)
						.AddMonths(1) // One month
						.AddDays(dayOfMonth - 1) // Move forward to the correct day.
				};
			}
			else if (dayOfMonth < after.Day)
			{
				// This day has already passed.
				// Return the correct day next month.
				return new[]
				{
					new DateTime(after.Year, after.Month, dayOfMonth).AddMonths(1)
				};
			}
			else
			{
				// Day is in the future this month.
				return new[]
				{
					new DateTime(after.Year, after.Month, dayOfMonth) // This month
				};
			}
		}
	}
}