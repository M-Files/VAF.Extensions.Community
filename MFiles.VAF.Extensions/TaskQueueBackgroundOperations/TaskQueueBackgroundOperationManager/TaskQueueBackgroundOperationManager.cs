using MFiles.VAF.Common;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.MultiserverMode;
using Newtonsoft.Json;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFiles.VAF.AppTasks;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Manages one or more background operations within a single queue.
	/// </summary>
	public partial class TaskQueueBackgroundOperationManager<TSecureConfiguration>
		where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// Lock for <see cref="CurrentServer"/>.
		/// </summary>
		private static readonly object _lock = new object();

		/// <summary>
		/// The server that this application is running on.
		/// </summary>
		internal static VaultServerAttachment CurrentServer { get; private set; }

		/// <summary>
		/// If multiple managers are used, this value will be used to sort their background operations
		/// on the dashboard (ascending order).
		/// </summary>
		public int DashboardSortOrder { get; set; } = 0;

		/// <summary>
		/// The background operations managed by this instance.
		/// </summary>
		internal readonly Dictionary<string, TaskQueueBackgroundOperation<TSecureConfiguration>> BackgroundOperations
			= new Dictionary<string, TaskQueueBackgroundOperation<TSecureConfiguration>>();

		/// <summary>
		/// Ensures that <see cref="CurrentServer"/> is set correctly.
		/// </summary>
		/// <param name="vaultApplication">The vault application where this code is running.</param>
		internal static void SetCurrentServer(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));

			// Ensure that we have a current server.
			lock (_lock)
			{
				if (null == CurrentServer)
				{
					CurrentServer
						= vaultApplication
							.PermanentVault?
							.GetVaultServerAttachments()?
							.GetCurrent();
				}
			}
		}

		/// <summary>
		/// The queue Id.
		/// </summary>
		public string QueueId { get;set; }

		/// <summary>
		/// The vault application that contains this background operation manager.
		/// </summary>
		public ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; private set; }

		/// <summary>
		/// The cancellation token source, if cancellation should be supported.
		/// </summary>
		public CancellationTokenSource CancellationTokenSource { get; private set; }

		/// <summary>
		/// The task processor for this queue.
		/// </summary>
		public AppTasks.ITaskQueueProcessor TaskQueueProcessor { get; private set; }

		/// <summary>
		/// Creates a background operation manager for a given queue.
		/// </summary>
		/// <param name="vaultApplication">The vault application that contains this background operation manager.</param>
		/// <param name="queueId">The queue Id.</param>
		/// <param name="cancellationTokenSource">The cancellation token source, if cancellation should be supported.</param>
		public TaskQueueBackgroundOperationManager
		(
			ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication,
			CancellationTokenSource cancellationTokenSource = default
		) : this
		(
			vaultApplication,
			$"{vaultApplication?.GetType()?.FullName?.Replace(".", "-")}-BackgroundOperations",
			cancellationTokenSource
		)
		{
		}

		/// <summary>
		/// Creates a background operation manager for a given queue.
		/// </summary>
		/// <param name="vaultApplication">The vault application that contains this background operation manager.</param>
		/// <param name="queueId">The queue Id.</param>
		/// <param name="cancellationTokenSource">The cancellation token source, if cancellation should be supported.</param>
		public TaskQueueBackgroundOperationManager
		(
			ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication,
			string queueId,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(queueId))
				throw new ArgumentException("The queue id cannot be null or whitespace.", nameof(queueId));

			// Assign.
			this.CancellationTokenSource = cancellationTokenSource;
			this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
			this.QueueId = queueId;

			// Set up the task processor.
			this.TaskProcessor = new AppTasks.TaskProcessor<BackgroundOperationTaskDirective>
			(
				TaskQueueBackgroundOperation<BackgroundOperationTaskDirective, TSecureConfiguration>.TaskTypeId,
				new AppTasks.TaskProcessor.Handler<BackgroundOperationTaskDirective>(ProcessJobHandler),
				transactionMode: AppTasks.TransactionMode.Unsafe
			);

			// Set up the task queue processor.
			this.TaskQueueProcessor = this
				.VaultApplication
				.TaskManager
				.RegisterQueue
				(
					this.QueueId,
					new[] { this.TaskProcessor }
				);

			// Ensure we have a current server.
			SetCurrentServer(vaultApplication);
		}

		protected AppTasks.TaskProcessor TaskProcessor { get; }

		/// <summary>
		/// Handles when a job from a task requires processing.
		/// Note that all tasks/jobs in the queue use the same task type ID,
		/// so the directive <see cref="BackgroundOperationTaskDirective.BackgroundOperationName"/>
		/// is used to identify which tasks should be executed by
		/// which background operation.
		/// </summary>
		/// <param name="job">The job to process.</param>
		protected virtual void ProcessJobHandler(AppTasks.ITaskProcessingJob<BackgroundOperationTaskDirective> job)
		{
			// Sanity.
			if(null == job)
				return;

			// Ensure cancellation has not been requested.
			job.ThrowIfTaskCancellationRequested();

			// What is the current state?
			var state = job.GetStatus();

			// Sanity.
			if (null == job.Directive)
			{
				// This is an issue.  We have no way to decide what background operation should run it.  Die.
				SysUtils.ReportErrorToEventLog
				(
					$"Job loaded with no directive (queue: {this.QueueId}, task type: {job.TaskInfo.TaskType}, task id: {job.TaskInfo.TaskID})."
				);
				return;
			}

			// Check that we know the job this was associated with.
			if (string.IsNullOrWhiteSpace(job.Directive.BackgroundOperationName))
			{
				// This is an issue.  We have no way to decide what background operation should run it.  Die.
				SysUtils.ReportErrorToEventLog
				(
					$"Job loaded with no background operation name loaded (queue: {this.QueueId}, task type: {job.TaskInfo.TaskType}, task id: {job.TaskInfo.TaskID})."
				);
				return;
			}

			// If we have a directive then extract it.
			var dir = job.Directive.GetParsedInternalDirective();

			// Find the background operation to run.
			TaskQueueBackgroundOperation<TSecureConfiguration> bo = null;
			lock (_lock)
			{
				if (false == this.BackgroundOperations.TryGetValue(job.Directive.BackgroundOperationName, out bo))
				{
					// We have no registered background operation to handle the callback.
					SysUtils.ReportErrorToEventLog
					(
						$"No background operation found with name {job.Directive.BackgroundOperationName} (queue: {this.QueueId}, task type: {job.TaskInfo.TaskType}, task id: {job.TaskInfo.TaskID})."
					);
					return;
				}
			}

			// Should we repeat?
			DateTime? nextRun = null;
			switch (bo.RepeatType)
			{
				case TaskQueueBackgroundOperationRepeatType.Interval:
					
					// Add the interval to the current datetime.
					if (bo.Interval.HasValue)
						nextRun = DateTime.UtcNow.Add(bo.Interval.Value);
					break;

				case TaskQueueBackgroundOperationRepeatType.Schedule:

					// Get the next execution time from the schedule.
					nextRun = bo.Schedule?.GetNextExecution(DateTime.Now);
					break;
			}

			// If we have a next run time then re-run.
			if (null != nextRun)
			{
				// Bind to the completed event ( called always ) of the job.
				// That way even if the job is canceled, fails, or finishes successfully
				// ...we always schedule the next run.
				job.Completed += (s, op)
					=>
					{
						// Ensure that if two threads both run this at once we don't end up with a race condition.
						lock (_lock)
						{
							// Cancel any future executions (we only want the single one created below).
							this.CancelFutureExecutions(bo.Name);

							// Now schedule it to run according to the interval.
							this.RunOnce
							(
								bo.Name,
								nextRun.Value.ToUniversalTime(),
								dir
							);
						}
					};
			}

			// Bind to the life-cycle events.
			job.Succeeded += (sender, op) => CompleteJob(job, MFTaskState.MFTaskStateCompleted);
			job.Failed += (sender, args) => CompleteJob(job, MFTaskState.MFTaskStateFailed, args.Exception);

			// Perform the action.
			// Mark it as started.
			job.Update
			(
				new TaskInformation()
				{
					Started = DateTime.Now,
					LastActivity = DateTime.Now,
					CurrentTaskState = MFTaskState.MFTaskStateInProgress
				}
			);

			// NOTE: this should not have any error handling around it here unless it re-throws the error
			// Catching the exception here can result in job.ProcessingFailed not being called
			// Delegate to the background operation.
			bo.RunJob
			(
				// The TaskProcessorJobEx class wraps the job and allows easy updates.
				new TaskProcessorJobEx<BackgroundOperationTaskDirective, TSecureConfiguration>()
				{
					Job = job,
					TaskQueueBackgroundOperationManager = this
				},
				dir
			);
		}

		/// <summary>
		/// Marks the <paramref name="job"/> to have the completed state of <paramref name="targetState"/>.
		/// </summary>
		/// <param name="job">The job to update.</param>
		/// <param name="targetState">The final completed state.</param>
		/// <param name="exception">The exception, if the state is failed.</param>
		protected void CompleteJob
		(
			AppTasks.ITaskProcessingJob<BackgroundOperationTaskDirective> job, 
			MFTaskState targetState,
			Exception exception = null
		)
		{
			// Skip nulls.
			if (null == job)
				return;

			// Update the task.
			try
			{
				// Build up the task information with new data.
				var taskInformation = new TaskInformation(job.GetStatus()?.Data);
				taskInformation.CurrentTaskState = targetState;
				taskInformation.Completed = DateTime.Now;
				taskInformation.LastActivity = DateTime.Now;
				if( exception != null )
				{
					taskInformation.StatusDetails = exception.Message;
				}

				// Update the task information.
				job.Update(taskInformation);
			}
			catch
			{
#if DEBUG
				throw;
#endif
			}
		}
	}
}