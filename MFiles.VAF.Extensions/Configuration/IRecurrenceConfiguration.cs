using System;

namespace MFiles.VAF.Extensions
{
	public interface IRecurrenceConfiguration
	{
		/// <summary>
		/// Returns a string that describes the current recurrence configuration.  Must be HTML-formatted.
		/// </summary>
		/// <returns>The string</returns>
		/// <example>&lt;p&gt;Runs every 10 minutes.&lt;/p&gt;</example>
		string ToDashboardDisplayString();

		/// <summary>
		/// Returns the next time the recurrence should run.
		/// </summary>
		/// <param name="after">The current execution time, or null to use the current time.</param>
		/// <returns>The next-run time.  May be null if there are no valid next-run times.</returns>
		DateTime? GetNextExecution(DateTime? after = null);
	}
}