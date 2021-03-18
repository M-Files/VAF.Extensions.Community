using System.Collections.Generic;

namespace MFiles.VAF.Extensions.MultiServerMode

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
	}
}