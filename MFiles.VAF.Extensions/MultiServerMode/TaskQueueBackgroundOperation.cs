using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public class TaskQueueBackgroundOperation<TDirective>
	where TDirective : TaskQueueDirective
	{
		/// <summary>
		/// The task queue manager for this task.
		/// </summary>
		public AppTaskBatchProcessor TaskProcessor { get; private set; }

		/// <summary>
		/// The manager that manages this operation.
		/// </summary>
		public TaskQueueBackgroundOperationManager<TDirective> BackgroundOperationManager { get; private set; }

		/// <summary>
		/// The name of the vault running the background operation.
		/// </summary>
		public string VaultName => this
			.BackgroundOperationManager
			.VaultApplication?
			.PermanentVault?
			.Name;

		/// <summary>
		/// The name of the background operation.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The task type for this background operation.
		/// </summary>
		public string TaskTypeId { get; private set; }

		/// <summary>
		/// The cancellation token for the operation.
		/// </summary>
		public CancellationTokenSource CancellationTokenSource { get; private set; }

		/// <summary>
		/// Flag to show whether the task has been cancelled.
		/// </summary>
		public bool ExplicitlyCancelled { get; set; }

		/// <summary>
		/// Is the operation cancelled. A cancelled operation cannot be used and is waiting to be disposed.
		/// </summary>
		public bool Cancelled => this.ExplicitlyCancelled || (this.CancellationTokenSource?.Token.IsCancellationRequested ?? false);

		/// <summary>
		/// Is the operation recurring.
		/// </summary>
		public bool Recurring { get; private set; }

		/// <summary>
		/// The interval between runs, when the operation is recurring. Only available when the operation is recurring.
		/// </summary>
		public TimeSpan? Interval { get; private set; }

		/// <summary>
		/// The method to run.
		/// </summary>
		public Action<TaskProcessorJob, TDirective> UserMethod { get; private set; }

		/// <summary>
		/// Creates a new background operation that runs the method in separate task.
		/// </summary>
		/// <param name="name">The name of the background operation.</param>
		/// <param name="method">The method to invoke. The background operation will be passed to the method.</param>
		/// <param name="backgroundOperationManager">The background operation manager that manages this operation.</param>
		/// <param name="taskTypeId">The type of this background operation.</param>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		public TaskQueueBackgroundOperation
		(
			TaskQueueBackgroundOperationManager<TDirective> backgroundOperationManager,
			string taskTypeId,
			string name,
			Action<TaskProcessorJob, TDirective> method,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(taskTypeId))
				throw new ArgumentException("The task type cannot be null or whitespace.", nameof(taskTypeId));

			// Save parameters.
			this.CancellationTokenSource = cancellationTokenSource;
			this.TaskTypeId = taskTypeId;
			this.BackgroundOperationManager = backgroundOperationManager ?? throw new ArgumentNullException(nameof(backgroundOperationManager));
			this.TaskProcessor = backgroundOperationManager
				.VaultApplication
				.CreateConcurrentTaskProcessor
				(
					this.BackgroundOperationManager.QueueId,
					new Dictionary<string, TaskProcessorJobHandler>
					{
						{ taskTypeId, this.ProcessJobHandler }
					},
					cancellationTokenSource == null
						? new CancellationTokenSource()
						: CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token)
				);
			this.UserMethod = method ?? throw new ArgumentNullException( nameof(method) );
			this.Name = name ?? throw new ArgumentNullException( nameof(name) );

			// Initialize default values.
			this.Recurring = false;
			this.Interval = null;

			// Register the task queues.
			this.TaskProcessor.RegisterTaskQueues();
		}

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce(TDirective directive = null)
		{
			// Schedule the next task to execute ASAP.
			this.TaskProcessor.CreateApplicationTaskSafe
			(
				true,
				this.BackgroundOperationManager.QueueId,
				this.TaskTypeId,
				directive?.ToBytes(),
				DateTime.UtcNow
			);
		}

		/// <summary>
		/// Processes a job in the queue.
		/// Performs validation then delegates to <see cref="TaskManagerBase{TTaskProcessor, TSettings, TDirective}.JobHandler"/>.
		/// </summary>
		/// <param name="job">The job to process.</param>
		protected virtual void ProcessJobHandler(TaskProcessorJob job)
		{
			// Sanity.
			if(null == job)
				return;

			// Ensure cancellation has not been requested.
			job.ThrowIfCancellationRequested();

			// Update the progress of the task in the task queue.
			this.TaskProcessor.UpdateTaskAsAssignedToProcessor( job );

			// Sanity.
			if (null == job.Data?.Value)
			{
				return;
			}

			// Deserialize the directive.
			var dir = TaskQueueDirective.Parse<TDirective>( job.Data?.Value );

			// If it is a broadcast directive, then was it generated on the same server?
			// If so then ignore.
			if (dir is BroadcastDirective broadcastDirective)
			{
				{
					if (broadcastDirective.GeneratedFromGuid
						== TaskQueueBackgroundOperationManager.CurrentServer.ServerID)
						return;
				}
			}

			// Perform the action.
			try
			{
				// Delegate to the hander.
				this.RunJob(job, dir);
			}
			catch(Exception e)
			{
				// Exception.
				this.TaskProcessor.UpdateTaskInfo
				(
					job.Data?.Value,
					MFTaskState.MFTaskStateFailed,
					e.Message,
					false
				);
				// TODO: throw?
			}
		}

		/// <summary>
		/// Begins running the operation at given intervals. If a run takes longer than the interval, the next run starts immediately after the previous run.
		/// </summary>
		/// <param name="interval">The interval between consecutive runs.</param>
		public void RunAtIntervals( TimeSpan interval )
		{
			// Check for validity.
			if( interval < TimeSpan.Zero )
				throw new ArgumentOutOfRangeException
				(
					nameof(interval),
					"The timer interval cannot be less than zero." 
				);

			// Cancel any existing executions.
			this.StopRunningAtIntervals();

			// Set up the recurrance data.
			this.Interval = interval;
			this.Recurring = true;
			
			// Run (which will set up the next iteration).
			this.RunOnce();
		}

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
					this.BackgroundOperationManager.VaultApplication.PermanentVault,
					this.BackgroundOperationManager.QueueId,
					t => t.Type == this.TaskTypeId,
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
					$"Exception cancelling tasks of type {this.TaskTypeId} on queue {this.BackgroundOperationManager.QueueId}.",
					e
				);
			}
		}

		/// <summary>
		/// Stops running the operation at intervals.
		/// Cancels any scheduled executions.
		/// </summary>
		public void StopRunningAtIntervals()
		{
			// Stop any new ones.
			this.Recurring = false;

			// Cancel any future executions.
			this.CancelFutureExecutions("Superseded.");
		}

		/// <summary>
		/// Cancels the operation. A cancelled operation cannot be used and is waiting to be disposed.
		/// </summary>
		public void Cancel()
		{
			this.CancellationTokenSource?.Cancel();
		}

		private void RunJob(TaskProcessorJob job, TDirective directive)
		{
			if (this.Recurring && this.Interval.HasValue)
			{
				// Bind to the completed event ( called always ) of the job.
				// That way even if the job is canceled, fails, or finishes successfully
				// ...we always schedule the next run.
				job.ProcessingCompleted += (s, op)
					=> this.TaskProcessor.CreateApplicationTaskSafe(
						true,
						this.BackgroundOperationManager.QueueId,
						this.TaskTypeId,
						directive?.ToBytes(),
						DateTime.UtcNow.Add(this.Interval.Value)
					);
			}

			// The hourly task has come due and is being processed.
			job.ThrowIfCancellationRequested();

			// Update has having been assigned.
			this.TaskProcessor.UpdateTaskAsAssignedToProcessor( job );

			// Execute the callback.
			this.UserMethod(job, directive);
		}
	}
}