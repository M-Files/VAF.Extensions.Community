using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A task queue directive with a display name.
	/// </summary>
	public abstract class TaskQueueDirectiveWithDisplayName
		: TaskQueueDirective
	{
		/// <summary>
		/// The display name for the action/directive.
		/// This may be shown on dashboards to identify what this task does.
		/// </summary>
		/// <example>"Convert Document 123 to PDF"</example>
		/// <example>"Import AAABBBCCC.pdf"</example>
		public string DisplayName { get; set; }
	}
}
