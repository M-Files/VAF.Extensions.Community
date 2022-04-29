using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF;
using MFilesAPI;
using System;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Contains helper methods for <see cref="TaskQueueManager"/>.
	/// </summary>
	public static class TaskQueueManagerExtensionMethods
	{
		/// <summary>
		/// Updates the data associated with <paramref name="job"/>,
		/// setting the state to <paramref name="state"/> and the progress
		/// data to <paramref name="progressData"/>.
		/// </summary>
		/// <param name="taskQueueManager">The task manager to update the job using.</param>
		/// <param name="job">The job to update.</param>
		/// <param name="state">The new job state.</param>
		/// <param name="progressData">Any progress data to report.</param>
		[Obsolete("You should migrate to using VAF 2.3+ task queues (in the MFiles.VAF.AppTasks namespace), not VAF 2.2 task queues (in the MFiles.VAF.MultiserverMode namespace)")]
		public static void UpdateTask
		(
			this TaskQueueManager taskQueueManager,
			TaskProcessorJob job,
			MFTaskState state,
			string progressData = ""
		)
		{
			// Sanity.
			if (null == taskQueueManager)
				throw new ArgumentNullException(nameof(taskQueueManager));
			if (null == job)
				throw new ArgumentNullException(nameof(job));

			// Use the default UpdateTask implementation.
			taskQueueManager.UpdateTask(job.AppTaskId, state, progressData);
		}

		/// <summary>
		/// Updates the data associated with <paramref name="job"/>.
		/// Only use this overload to represent an exception having occurred whilst processing a job.
		/// </summary>
		/// <param name="taskQueueManager">The task manager to update the job using.</param>
		/// <param name="job">The job to update.</param>
		/// <param name="exception">The exception that was thrown.</param>
		[Obsolete("You should migrate to using VAF 2.3+ task queues (in the MFiles.VAF.AppTasks namespace), not VAF 2.2 task queues (in the MFiles.VAF.MultiserverMode namespace)")]
		public static void UpdateTask
		(
			this TaskQueueManager taskQueueManager,
			TaskProcessorJob job,
			Exception exception
		)
		{
			// Sanity.
			if (null == taskQueueManager)
				throw new ArgumentNullException(nameof(taskQueueManager));
			if (null == job)
				throw new ArgumentNullException(nameof(job));
			if (null == exception)
				throw new ArgumentNullException(nameof(exception));

			// Use the other overload.
			taskQueueManager.UpdateTask
			(
				job.AppTaskId, 
				MFTaskState.MFTaskStateFailed,
				exception.ToString()
			);
		}
	}
}
