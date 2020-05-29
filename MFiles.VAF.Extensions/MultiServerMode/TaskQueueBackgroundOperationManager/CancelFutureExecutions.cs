using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFilesAPI;
using System;
using System.Linq;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public partial class TaskQueueBackgroundOperationManager
	{

		/// <summary>
		/// Marks any future executions of this job in this queue as cancelled.
		/// </summary>
		public void CancelFutureExecutions(string remarks = null)
		{
			try
			{
				// Cancel any tasks that are already scheduled.
				var tasksToCancel = TaskQueueAdministrator.FindTasks
				(
					this.VaultApplication.PermanentVault,
					this.QueueId,
					t => t.Type == TaskQueueBackgroundOperation.TaskTypeId,
					new[] { MFTaskState.MFTaskStateWaiting, MFTaskState.MFTaskStateInProgress }
				);
				foreach (var task in tasksToCancel.Cast<ApplicationTaskInfo>())
				{
					// Mark each task as superseded.
					this.TaskProcessor.UpdateCancelledJobInTaskQueue
					(
						task.ToApplicationTask(),
						string.Empty,
						remarks
					);
				}
			}
			catch(Exception e)
			{
				SysUtils.ReportErrorToEventLog
				(
					$"Exception cancelling tasks of type {TaskQueueBackgroundOperation.TaskTypeId} on queue {this.QueueId}.",
					e
				);
			}
		}
	}
}