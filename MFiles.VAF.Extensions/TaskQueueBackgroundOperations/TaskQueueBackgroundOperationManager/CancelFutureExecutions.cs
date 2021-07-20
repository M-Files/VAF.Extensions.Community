using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Linq;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager
	{
		/// <summary>
		/// Marks any future executions of this job in this queue as cancelled.
		/// </summary>
		/// <param name="backgroundOperationName">If set, cancels only future executions for the specified background operation.</param>
		/// <param name="remarks">Remarks to set on the cancellation.</param>
		public void CancelFutureExecutions(string backgroundOperationName = null, string remarks = null)
		{
			try
			{
				// Cancel any tasks that are already scheduled.
				var tasksToCancel = TaskQueueAdministrator.FindTasks
				(
					this.VaultApplication.PermanentVault,
					this.QueueId,
					t => t.Type == TaskQueueBackgroundOperation.TaskTypeId,
					new[] { MFTaskState.MFTaskStateWaiting }
				);
				foreach (var task in tasksToCancel.Cast<ApplicationTaskInfo>())
				{
					var applicationTask = task.ToApplicationTask();

					// Skip any that are not for this background operation.
					if (false == string.IsNullOrWhiteSpace(backgroundOperationName))
					{
						var backgroundOperationDirective = TaskQueueDirective.Parse<BackgroundOperationTaskQueueDirective>(applicationTask);
						if (null == backgroundOperationDirective?.BackgroundOperationName)
							continue;
						if (!backgroundOperationDirective.BackgroundOperationName.Equals(backgroundOperationName))
							continue;
					}

					try
					{
						// Mark each task as superseded.
						this.TaskProcessor.UpdateCancelledJobInTaskQueue
						(
							applicationTask,
							string.Empty,
							remarks
						);
					}
					catch (Exception e)
					{
						SysUtils.ReportErrorToEventLog
						(
							$"Exception cancelling task {task.TaskID} of type {TaskQueueBackgroundOperation.TaskTypeId} on queue {this.QueueId} to cancel.",
							e
						);
					}
				}
			}
			catch (Exception e)
			{
				SysUtils.ReportErrorToEventLog
				(
					$"Exception retrieving tasks of type {TaskQueueBackgroundOperation.TaskTypeId} on queue {this.QueueId} to cancel.",
					e
				);
			}
		}
	}
}