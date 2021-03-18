using System;

namespace MFiles.VAF.Extensions.MultiServerMode.ScheduledExecution

{
	public abstract class TriggerBase
	{
		/// <summary>
		/// The type of trigger this is (e.g. Daily, Weekly).
		/// </summary>
		public TriggerType Type { get; set; } = TriggerType.Unknown;

		/// <summary>
		/// Gets the next execution datetime for this trigger.
		/// </summary>
		/// <param name="after">The time after which the schedule should run.  Defaults to now (i.e. next-run time) if not provided.</param>
		/// <returns>The next execution time.</returns>
		public abstract DateTime? GetNextExecution(DateTime? after = null);
	}
}