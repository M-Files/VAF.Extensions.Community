﻿using MFiles.VAF.Common;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.MultiserverMode;
using Newtonsoft.Json;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Manages one or more background operations within a single queue.
	/// </summary>
	public partial class TaskQueueBackgroundOperationManager
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
		internal readonly Dictionary<string, TaskQueueBackgroundOperation> BackgroundOperations
			= new Dictionary<string, TaskQueueBackgroundOperation>();

		/// <summary>
		/// Ensures that <see cref="CurrentServer"/> is set correctly.
		/// </summary>
		/// <param name="vaultApplication">The vault application where this code is running.</param>
		internal static void SetCurrentServer(VaultApplicationBase vaultApplication)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));

			// Ensure that we have a current server.
			lock (TaskQueueBackgroundOperationManager._lock)
			{
				if (null == TaskQueueBackgroundOperationManager.CurrentServer)
				{
					TaskQueueBackgroundOperationManager.CurrentServer
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
		public VaultApplicationBase VaultApplication { get; private set; }

		/// <summary>
		/// The cancellation token source, if cancellation should be supported.
		/// </summary>
		public CancellationTokenSource CancellationTokenSource { get; private set; }

		/// <summary>
		/// The task processor for this queue.
		/// </summary>
		public AppTaskBatchProcessor TaskProcessor { get; private set; }

		/// <summary>
		/// Creates a background operation manager for a given queue.
		/// </summary>
		/// <param name="vaultApplication">The vault application that contains this background operation manager.</param>
		/// <param name="queueId">The queue Id.</param>
		/// <param name="cancellationTokenSource">The cancellation token source, if cancellation should be supported.</param>
		public TaskQueueBackgroundOperationManager
		(
			VaultApplicationBase vaultApplication,
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
			VaultApplicationBase vaultApplication,
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

			// Set up the task processor
			this.TaskProcessor = this
				.VaultApplication
				.CreateConcurrentTaskProcessor
				(
					this.QueueId,
					new Dictionary<string, TaskProcessorJobHandler>
					{
						{ TaskQueueBackgroundOperation.TaskTypeId, this.ProcessJobHandler }
					},
					cancellationTokenSource: cancellationTokenSource == null
						? new CancellationTokenSource()
						: CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token),
					enableAutomaticTaskUpdates: false
				);

			// Ensure we have a current server.
			TaskQueueBackgroundOperationManager.SetCurrentServer(vaultApplication);

			// Register the task queues.
			this.TaskProcessor.RegisterTaskQueues();
		}

		/// <summary>
		/// Handles when a job from a task requires processing.
		/// Note that all tasks/jobs in the queue use the same task type ID,
		/// so the directive <see cref="BackgroundOperationTaskQueueDirective.BackgroundOperationName"/>
		/// is used to identify which tasks should be executed by
		/// which background operation.
		/// </summary>
		/// <param name="job">The job to process.</param>
		protected virtual void ProcessJobHandler(TaskProcessorJob job)
		{
			// Sanity.
			if(null == job)
				return;

			// Ensure cancellation has not been requested.
			job.ThrowIfCancellationRequested();

			// What is the current state?
			var state = job.AppTaskState;

			// Update the progress of the task in the task queue.
			try
			{
				this.TaskProcessor.UpdateTaskAsAssignedToProcessor(job);
			}
			catch
			{
				// Could not mark the task as assigned to a processor.
				SysUtils.ReportToEventLog
				(
					$"Could not mark task {job.AppTaskId} as assigned to a processor (queue id: {job.AppTaskQueueId}, state: {state}).",
					System.Diagnostics.EventLogEntryType.Warning
				);
				return;
			}

			// Sanity.
			if (null == job.Data?.Value)
			{
				// This is an issue.  We have no way to decide what background operation should run it.  Die.
				SysUtils.ReportErrorToEventLog
				(
					$"Job loaded with no application task (queue: {job.AppTaskQueueId}, task id: {job.AppTaskId})."
				);
				return;
			}

			// Deserialize the background directive.
			var backgroundOperationDirective = job.GetTaskQueueDirective<BackgroundOperationTaskQueueDirective>();
			if (null == backgroundOperationDirective)
			{
				// This is an issue.  We have no way to decide what background operation should run it.  Die.
				SysUtils.ReportErrorToEventLog
				(
					$"Job loaded with no background operation name loaded (queue: {job.AppTaskQueueId}, task id: {job.AppTaskId})."
				);
				return;
			}

			// If we have a directive then extract it.
			var dir = backgroundOperationDirective.GetParsedInternalDirective();

			// If it is a broadcast directive, then was it generated on the same server?
			// If so then ignore.
			if (dir is BroadcastDirective broadcastDirective)
			{
				if (broadcastDirective.GeneratedFromGuid == TaskQueueBackgroundOperationManager.CurrentServer.ServerID)
					return;
			}

			// Find the background operation to run.
			TaskQueueBackgroundOperation bo = null;
			lock (_lock)
			{
				if (false == this.BackgroundOperations.TryGetValue(backgroundOperationDirective.BackgroundOperationName, out bo))
				{
					// We have no registered background operation to handle the callback.
					SysUtils.ReportErrorToEventLog
					(
						$"No background operation found with name {backgroundOperationDirective.BackgroundOperationName}(queue: {job.AppTaskQueueId}, task id: {job.AppTaskId})."
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
				job.ProcessingCompleted += (s, op)
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
			job.ProcessingCompleted += (sender, op) => CompleteJob(job, MFTaskState.MFTaskStateCompleted);
			job.CancellationRequested += (sender, op) => CompleteJob(job, MFTaskState.MFTaskStateCanceled);
			job.ProcessingFailed += (sender, args) => CompleteJob(job, MFTaskState.MFTaskStateFailed, args.Exception);

			// Perform the action.
			// Mark it as started.
			job.Data.Value = this.TaskProcessor.UpdateTaskInfo
			(
				job.Data?.Value,
				MFTaskState.MFTaskStateInProgress,
				JsonConvert.SerializeObject
				(
					new TaskInformation()
					{
						Started = DateTime.Now,
						LastActivity = DateTime.Now,
						CurrentTaskState = MFTaskState.MFTaskStateInProgress
					}
				),
				false
			);

			// NOTE: this should not have any error handling around it here unless it re-throws the error
			// Catching the exception here can result in job.ProcessingFailed not being called
			// Delegate to the background operation.
			bo.RunJob
			(
				// The TaskProcessorJobEx class wraps the job and allows easy updates.
				new TaskProcessorJobEx()
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
		protected void CompleteJob(TaskProcessorJob job, MFTaskState targetState, Exception exception = null)
		{
			var appTask = job?.Data?.Value;

			// Skip nulls.
			if (appTask == null)
				return;

			// Update the task.
			try
			{
				var info = job?.Data?.Value?.RetrieveTaskInfo()
					?? new TaskInformation();
				info.Completed = DateTime.Now;
				info.LastActivity = DateTime.Now;

				if( exception != null )
				{
					info.StatusDetails = exception.Message;
				}

				// Update the task information.
				job.Data.Value = this.TaskProcessor.UpdateTaskInfo
				(
					appTask,
					targetState,
					JsonConvert.SerializeObject(info),
					appendRemarks: false
				);
			}
			catch(Exception e)
			{
#if DEBUG
				throw;
#endif
			}
		}
	}
}