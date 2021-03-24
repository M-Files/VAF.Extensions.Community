using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.MultiServerMode.ScheduledExecution

{
	/// <summary>
	/// Represents a trigger that runs every day, potentially multiple times per day.
	/// </summary>
	public class DailyTrigger
		: TriggerBase
	{
		/// <summary>
		/// The times of day to trigger the schedule.
		/// There must be at least one item in this collection for the trigger to be active.
		/// </summary>
		public List<TimeSpan> TriggerTimes { get; set; } = new List<TimeSpan>();

		/// <inheritdoc />
		public override DateTime? GetNextExecution(DateTime? after = null)
		{
			// Sanity.
			if (
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				)
				return null;

			// When should we start looking?
			after = (after ?? DateTime.UtcNow).ToLocalTime();

			// Get the next execution time.
			return this.TriggerTimes
				.Select(
					t =>
						after.Value.TimeOfDay < t
							? after.Value.Date.Add(t) // Time is yet to come today.
							: after.Value.Date.AddDays(1).Add(t) // Time has passed - return tomorrow.
				)
				.OrderBy(d => d)
				.FirstOrDefault();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			// Sanity.
			if (null == this.TriggerTimes || this.TriggerTimes.Count == 0)
				return null;

			return $"Daily at the following times: {string.Join(", ", this.TriggerTimes.Select(t => t.ToString()))}.";
		}

	}
}