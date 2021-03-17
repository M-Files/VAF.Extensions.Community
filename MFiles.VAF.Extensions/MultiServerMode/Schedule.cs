using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.MultiServerMode

{
	/// <summary>
	/// Represents a schedule in which a job should be re-run.
	/// </summary>
	public class Schedule
	{
		/// <summary>
		/// The times of day to trigger the schedule.
		/// There must be at least one item in this collection for the schedule to be active.
		/// </summary>
		public List<TimeSpan> TriggerTimes { get; set; } = new List<TimeSpan>();

		/// <summary>
		/// The days on which to trigger the schedule.
		/// There must be at least one item in this collection for the schedule to be active.
		/// </summary>
		public List<DayOfWeek> TriggerDays { get; set; } = new List<DayOfWeek>();

		/// <summary>
		/// Gets the next execution time for this schedule.
		/// </summary>
		/// <param name="after">The time after which the schedule should run.  Defaults to now (i.e. next-run time) if not provided.</param>
		/// <returns>The next execution time.</returns>
		public DateTime? GetNextExecutionTime(DateTime? after = null)
		{
			// Sanity.
			if (
				(null == this.TriggerDays || 0 == this.TriggerDays.Count)
				||
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				)
				return null;
			// When should we start looking?
			after = (after ?? DateTime.UtcNow).ToLocalTime();
			
			// Get the next execution time (this will not find run times today).
			return this.TriggerDays
				.SelectMany(d => GetNextDayOfWeek(after.Value, d))
				.SelectMany(d => this.TriggerTimes.Select(t => d.Add(t)))
				.Where(d => d > after.Value)
				.OrderBy(d => d)
				.FirstOrDefault();
		}

		/// <summary>
		/// Helper method to return the next potential run date within a schedule.
		/// </summary>
		/// <param name="after">The day to start looking on.</param>
		/// <param name="dayOfWeek">The day of the week to return.</param>
		/// <returns>
		/// If <paramref name="after"/> is the same day as <paramref name="dayOfWeek"/> then will return
		/// two items - one for today and one for the same day next week.
		/// If not then it will return one item - for the next time that this day occurs
		/// (later this week or next, depending on parameters).
		/// </returns>
		internal static IEnumerable<DateTime> GetNextDayOfWeek(DateTime after, DayOfWeek dayOfWeek)
		{
			// Get the number of days ahead.
			var daysToAdd = (7 - (after.DayOfWeek - dayOfWeek)) % 7;

			// If we get today then return it and also next week.
			if (daysToAdd == 0)
			{
				yield return after.Date;
				daysToAdd = 7;
			}

			// Get the next one of this day.
			yield return after.Date.AddDays(daysToAdd);
		}

	}
}