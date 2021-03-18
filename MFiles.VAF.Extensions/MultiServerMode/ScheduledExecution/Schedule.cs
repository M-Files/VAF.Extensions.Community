using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.MultiServerMode.ScheduledExecution

{
	/// <summary>
	/// Represents a schedule in which a job should be re-run.
	/// </summary>
	public class Schedule
	{
		/// <summary>
		/// The rules that should trigger the schedule to run.
		/// </summary>
		public List<TriggerBase> Triggers { get; set; } = new List<TriggerBase>();

		/// <summary>
		/// Gets the next execution datetime for this trigger.
		/// </summary>
		/// <param name="after">The time after which the schedule should run.  Defaults to now (i.e. next-run time) if not provided.</param>
		/// <returns>The next execution time.</returns>
		public DateTime? GetNextExecution(DateTime? after = null)
		{
			return this.Triggers?
				.Select(t => t.GetNextExecution(after))
				.Where(d => d.HasValue)
				.OrderBy(d => d)
				.FirstOrDefault();
		}
	}
}