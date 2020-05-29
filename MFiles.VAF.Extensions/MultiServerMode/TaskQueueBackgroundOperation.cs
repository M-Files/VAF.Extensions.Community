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
		: TaskQueueBackgroundOperation
		where TDirective : TaskQueueDirective
	{
		/// <summary>
		/// The method to run.
		/// </summary>
		public new Action<TaskProcessorJob, TDirective> UserMethod { get; private set; }

		/// <summary>
		/// Creates a new background operation that runs the method in separate task.
		/// </summary>
		/// <param name="name">The name of the background operation.</param>
		/// <param name="method">The method to invoke. The background operation will be passed to the method.</param>
		/// <param name="backgroundOperationManager">The background operation manager that manages this operation.</param>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		public TaskQueueBackgroundOperation
		(
			TaskQueueBackgroundOperationManager backgroundOperationManager,
			string name,
			Action<TaskProcessorJob, TDirective> method,
			CancellationTokenSource cancellationTokenSource = default
		) : 
			base
				(
				backgroundOperationManager, 
				name,
				(j, d) => { }, // Ignore the method (we will set it below anyway).
				cancellationTokenSource
				)
		{
			// Save parameters.
			this.UserMethod = method ?? throw new ArgumentNullException( nameof(method) );
		}

		/// <inheritdoc />
		public override void RunJob(TaskProcessorJob job, TaskQueueDirective directive)
		{
			// Execute the callback.
			this.UserMethod(job, directive as TDirective);
		}

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce
		(
			DateTime? runAt = null,
			TDirective directive = null
		)
		{
			// Use the base implementation.
			base.RunOnce(runAt, directive);
		}

		/// <summary>
		/// Begins running the operation at given intervals. If a run takes longer than the interval, the next run starts immediately after the previous run.
		/// </summary>
		/// <param name="interval">The interval between consecutive runs.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		public void RunAtIntervals( TimeSpan interval, TDirective directive = null )
		{
			// Use the base implementation.
			base.RunAtIntervals(interval, directive);
		}
	}

	public class TaskQueueBackgroundOperation
	{
		/// <summary>
		/// The task type for this background operation.
		/// </summary>
		public const string TaskTypeId = "VaultApplication-BackgroundOperation";

		/// <summary>
		/// The manager that manages this operation.
		/// </summary>
		public TaskQueueBackgroundOperationManager BackgroundOperationManager { get; private set; }

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
		public Action<TaskProcessorJob, TaskQueueDirective> UserMethod { get; private set; }

		/// <summary>
		/// Creates a new background operation that runs the method in separate task.
		/// </summary>
		/// <param name="name">The name of the background operation.</param>
		/// <param name="method">The method to invoke. The background operation will be passed to the method.</param>
		/// <param name="backgroundOperationManager">The background operation manager that manages this operation.</param>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		public TaskQueueBackgroundOperation
		(
			TaskQueueBackgroundOperationManager backgroundOperationManager,
			string name,
			Action<TaskProcessorJob, TaskQueueDirective> method,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("The background operation name cannot be null or whitespace.", nameof(name));

			// Save parameters.
			this.CancellationTokenSource = cancellationTokenSource;
			this.BackgroundOperationManager = backgroundOperationManager ?? throw new ArgumentNullException(nameof(backgroundOperationManager));
			this.UserMethod = method ?? throw new ArgumentNullException( nameof(method) );
			this.Name = name ?? throw new ArgumentNullException( nameof(name) );

			// Initialize default values.
			this.Recurring = false;
			this.Interval = null;
		}

		/// <summary>
		/// Runs the operation at once.
		/// </summary>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce(DateTime? runAt = null, TaskQueueDirective directive = null)
		{
			// Schedule the next task to execute ASAP.
			this.BackgroundOperationManager.RunOnce(this.Name, runAt, directive);
		}

		/// <summary>
		/// Begins running the operation at given intervals. If a run takes longer than the interval, the next run starts immediately after the previous run.
		/// </summary>
		/// <param name="interval">The interval between consecutive runs.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		public void RunAtIntervals( TimeSpan interval, TaskQueueDirective directive = null )
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
			this.RunOnce(directive: directive);
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
					t => t.Type == TaskQueueBackgroundOperation.TaskTypeId,
					new[] { MFTaskState.MFTaskStateWaiting, MFTaskState.MFTaskStateInProgress }
				);
				foreach (var task in tasksToCancel.Cast<ApplicationTaskInfo>())
				{
					// If this task is not for this background operation then ignore it.
					var directive = TaskQueueDirective.Parse<BackgroundOperationTaskQueueDirective>(task.ToApplicationTask());
					if(null == directive)
						continue;
					if(directive.BackgroundOperationName != this.Name)
						continue;

					// Mark each task as superseded.
					this.BackgroundOperationManager.TaskProcessor.UpdateCancelledJobInTaskQueue
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
					$"Exception cancelling tasks for background operation {this.Name} of type {TaskQueueBackgroundOperation.TaskTypeId} on queue {this.BackgroundOperationManager.QueueId}.",
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

		/// <summary>
		/// Called by the <see cref="TaskQueueBackgroundOperationManager"/> when a task for this background operation
		/// is scheduled.
		/// </summary>
		/// <param name="job">The job to run.</param>
		/// <param name="directive">The directive, if any, passed in.</param>
		public virtual void RunJob(TaskProcessorJob job, TaskQueueDirective directive)
		{
			// Execute the callback.
			this.UserMethod(job, directive);
		}
	}
}