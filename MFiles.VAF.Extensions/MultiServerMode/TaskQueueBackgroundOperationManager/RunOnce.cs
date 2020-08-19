using System;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public partial class TaskQueueBackgroundOperationManager
	{

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <param name="backgroundOperationName">The name of the background operation that should be invoked when this job is run.</param>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive - if any - to pass to the job.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce
		(
			string backgroundOperationName,
			DateTime? runAt = null,
			TaskQueueDirective directive = null
		)
		{
			// Use the other overload.
			this.RunOnce<TaskQueueDirective>
			(
				backgroundOperationName,
				runAt,
				directive
			);
		}

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <param name="backgroundOperationName">The name of the background operation that should be invoked when this job is run.</param>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive - if any - to pass to the job.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce<TDirective>
		(
			string backgroundOperationName,
			DateTime? runAt = null,
			TDirective directive = null
		)
			where TDirective : TaskQueueDirective
		{
			// Create our actual directive.
			var backgroundOperationDirective =
				new BackgroundOperationTaskQueueDirective(backgroundOperationName, directive);

			// Schedule the next task to execute ASAP.
			this.TaskProcessor.CreateApplicationTaskSafe
			(
				true,
				this.QueueId,
				TaskQueueBackgroundOperation.TaskTypeId,
				backgroundOperationDirective?.ToBytes(),
				runAt.HasValue ? runAt.Value.ToUniversalTime() : DateTime.UtcNow
			);
		}

	}
}