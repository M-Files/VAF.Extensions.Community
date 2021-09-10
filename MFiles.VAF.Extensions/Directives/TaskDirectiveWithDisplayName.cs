using MFiles.VAF.AppTasks;
using MFiles.VAF.MultiserverMode;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	public interface ITaskDirectiveWithDisplayName
	{
		/// <summary>
		/// The display name for the action/directive.
		/// This may be shown on dashboards to identify what this task does.
		/// </summary>
		/// <example>"Convert Document 123 to PDF"</example>
		/// <example>"Import AAABBBCCC.pdf"</example>
		string DisplayName { get; set; }
	}

	/// <summary>
	/// A task queue directive with a display name.
	/// </summary>
	[DataContact]
	public abstract class TaskDirectiveWithDisplayName
		: TaskDirective, ITaskDirectiveWithDisplayName
	{
		/// <summary>
		/// The display name for the action/directive.
		/// This may be shown on dashboards to identify what this task does.
		/// </summary>
		/// <example>"Convert Document 123 to PDF"</example>
		/// <example>"Import AAABBBCCC.pdf"</example>
		[DataMember]
		public string DisplayName { get; set; }
	}
}
