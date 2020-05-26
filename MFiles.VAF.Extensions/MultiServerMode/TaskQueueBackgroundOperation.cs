using System;
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
		public ConcurrentTaskManager<TDirective> TaskQueueManager { get; private set; }

		/// <summary>
		/// The name of the vault running the background operation.
		/// </summary>
		public string VaultName => this
			.TaskQueueManager?
			.VaultApplication?
			.PermanentVault?
			.Name;

		/// <summary>
		/// The name of the background operation.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The cancellation token for the operation.
		/// </summary>
		public CancellationTokenSource CancellationTokenSource => this.TaskQueueManager?.TokenSource;

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
		public Action<Vault, TDirective> UserMethod { get; private set; }

		/// <summary>
		/// Creates a new background operation that runs the method in separate task.
		/// </summary>
		/// <param name="name">The name of the background operation.</param>
		/// <param name="method">The method to invoke. The background operation will be passed to the method.</param>
		/// <param name="vaultApplication">The vault application that this background operation is run in.</param>
		/// <param name="queueId">The queue Id that this background operation is logged within.</param>
		/// <param name="taskTypeId">The type of this background operation.</param>
		public TaskQueueBackgroundOperation
		(
			VaultApplicationBase vaultApplication,
			string queueId,
			string taskTypeId,
			string name,
			Action<Vault, TDirective> method
		)
		{
			this.Setup(new ConcurrentTaskManager<TDirective>
			(
				vaultApplication,
				queueId,
				taskTypeId,
				this.RunJob
			), name, method);
		}

		/// <summary>
		/// Creates a new background operation that runs the method in separate task.
		/// </summary>
		/// <param name="name">The name of the background operation.</param>
		/// <param name="method">The method to invoke. The background operation will be passed to the method.</param>
		/// <param name="manager">The concurrent task queue manager to use.</param>
		public TaskQueueBackgroundOperation
		(
			ConcurrentTaskManager<TDirective> manager,
			string name,
			Action<Vault, TDirective> method
		)
		{
			this.Setup(manager, name, method);
		}

		private void Setup
		(
			ConcurrentTaskManager<TDirective> manager,
			string name,
			Action<Vault, TDirective> method
		)
		{
			// Save parameters.
			this.TaskQueueManager = manager ?? throw new ArgumentNullException(nameof(manager));
			this.UserMethod = method ?? throw new ArgumentNullException( nameof(method) );
			this.Name = name ?? throw new ArgumentNullException( nameof(name) );

			// Initialize default values.
			this.Recurring = false;
			this.Interval = null;

			// Register the task queue.
			this.TaskQueueManager.TaskProcessor.RegisterTaskQueues();
		}

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce(TDirective directive = null)
		{
			// Schedule the next task to execute ASAP.
			this.TaskQueueManager.TaskProcessor.CreateApplicationTaskSafe
			(
				true,
				this.TaskQueueManager.QueueId,
				this.TaskQueueManager.TaskType,
				directive?.ToBytes(),
				DateTime.UtcNow
			);
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
		/// Stops running the operation at intervals.
		/// Cancels any scheduled executions.
		/// </summary>
		public void StopRunningAtIntervals()
		{
			// Stop any new ones.
			this.Recurring = false;

			try
			{
				// Cancel any tasks that are already scheduled.
				var tasksToCancel = TaskQueueAdministrator.FindTasks
				(
					this.TaskQueueManager.VaultApplication.PermanentVault,
					this.TaskQueueManager.QueueId,
					t => t.Type == this.TaskQueueManager.TaskType,
					new[] { MFTaskState.MFTaskStateWaiting, MFTaskState.MFTaskStateInProgress }
				);
				foreach (var task in tasksToCancel.Cast<ApplicationTaskInfo>())
				{
					// Mark each task as superseded.
					this.TaskQueueManager.TaskProcessor.UpdateCancelledJobInTaskQueue
					(
						task.ToApplicationTask(),
						string.Empty,
						"Superseded."
					);
				}
			}
			catch(Exception e)
			{
				SysUtils.ReportErrorToEventLog
				(
					$"Exception cancelling tasks of type {this.TaskQueueManager.TaskType} on queue {this.TaskQueueManager.QueueId}.",
					e
				);
			}
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
					=> this.TaskQueueManager.TaskProcessor.CreateApplicationTaskSafe(
						true,
						this.TaskQueueManager.QueueId,
						this.TaskQueueManager.TaskType,
						directive?.ToBytes(),
						DateTime.UtcNow.Add(this.Interval.Value)
					);
			}

			// The hourly task has come due and is being processed.
			job.ThrowIfCancellationRequested();

			// Update has having been assigned.
			this.TaskQueueManager.TaskProcessor.UpdateTaskAsAssignedToProcessor( job );

			// Execute the callback.
			this.UserMethod(job.Vault, directive);
		}
	}
}