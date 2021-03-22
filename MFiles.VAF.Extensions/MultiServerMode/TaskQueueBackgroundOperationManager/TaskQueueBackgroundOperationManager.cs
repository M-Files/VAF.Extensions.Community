using MFiles.VAF.Common;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods;

namespace MFiles.VAF.Extensions.MultiServerMode
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
						: CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token)
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
					$"Could not mark task {job.AppTaskId} as assigned to a processor (queue id: {job.AppTaskQueueId}, state: {job.AppTaskState}).",
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

			// If it is recurring then use the ProcessingComplete event to schedule the next execution.
			if (bo.Recurring && bo.Interval.HasValue)
			{
				// Bind to the completed event ( called always ) of the job.
				// That way even if the job is canceled, fails, or finishes successfully
				// ...we always schedule the next run.
				job.ProcessingCompleted += (s, op)
					=> this.RunOnce
					(
						bo.Name,
						DateTime.UtcNow.Add(bo.Interval.Value),
						dir
					);
			}

			// Perform the action.
			try
			{
				// Delegate to the background operation.
				bo.RunJob(job, dir);
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
	}
}