using MFiles.VAF.Extensions.MultiServerMode.ScheduledExecution;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode

{
	/// <summary>
	/// A directive used to represent a scheduled job.
	/// Additional directive data can be passed via <typeparamref name="TWrappedDirective"/>.
	/// </summary>
	/// <typeparam name="TWrappedDirective">The type of the additional (wrapped) directive.</typeparam>
	public class ScheduleDirective<TWrappedDirective>
		: TaskQueueDirective
		where TWrappedDirective : TaskQueueDirective
	{
		/// <summary>
		/// The schedule to run this job on.
		/// </summary>
		public Schedule Schedule { get; set; }

		/// <summary>
		/// The wrapped directive.  May be null.
		/// </summary>
		public TWrappedDirective WrappedDirective { get; set; }
	}
	/// <summary>
	/// A directive used to represent a scheduled job.
	/// If a wrapped directive is to be passed then it is typically
	/// better to use <see cref="ScheduleDirective{TWrappedDirective}"/>.
	/// </summary>
	public class ScheduleDirective
		: ScheduleDirective<TaskQueueDirective>
	{
	}
}