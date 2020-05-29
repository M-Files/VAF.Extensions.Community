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
	/// <typeparam name="TDirective">The directive type supported by the tasks in this queue.</typeparam>
	public class TaskQueueBackgroundOperationManager
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
							.PermanentVault
							.GetVaultServerAttachments()
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
			var wrappedDirective = new BackgroundOperationTaskQueueDirective(backgroundOperationName, directive);

			// Schedule the next task to execute ASAP.
			this.TaskProcessor.CreateApplicationTaskSafe
			(
				true,
				this.QueueId,
				TaskQueueBackgroundOperation.TaskTypeId,
				wrappedDirective?.ToBytes(),
				runAt.HasValue ? runAt.Value.ToUniversalTime() : DateTime.UtcNow
			);
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
					cancellationTokenSource == null
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
		/// so the directive data is used to identify which tasks should be executed by
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
			this.TaskProcessor.UpdateTaskAsAssignedToProcessor( job );

			// Sanity.
			if (null == job.Data?.Value)
			{
				return;
			}

			// Deserialize the background directive.
			var backgroundOperationDirective = job.GetTaskQueueDirective<BackgroundOperationTaskQueueDirective>();
			if (null == backgroundOperationDirective)
			{
				// This is an issue.  We have no way to decide what background operation should run it.  Die.
				SysUtils.ReportErrorToEventLog
				(
					$"Job loaded with no background operation name loaded (queue: {job.AppTaskQueueId}, task type: {job.AppTaskId})."
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
						$"No background operation found with name {backgroundOperationDirective.BackgroundOperationName}(queue: {job.AppTaskQueueId}, task type: {job.AppTaskId})."
					);
					return;
				} 
			}

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

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A scheduled background operation.</returns>
		public TaskQueueBackgroundOperation StartRecurringBackgroundOperation
		(
			string name,
			TimeSpan interval,
			Action method
		)
		{
			return this.StartRecurringBackgroundOperation
			(
				name,
				interval,
				(j, d) => method()
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A scheduled background operation.</returns>
		public TaskQueueBackgroundOperation StartRecurringBackgroundOperation
		(
			string name,
			TimeSpan interval,
			Action<TaskProcessorJob> method
		)
		{
			return this.StartRecurringBackgroundOperation
			(
				name,
				interval,
				(j, d) => method(j)
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A started background operation.</returns>
		public TaskQueueBackgroundOperation StartRecurringBackgroundOperation
		(
			string name,
			TimeSpan interval,
			Action<TaskProcessorJob, TaskQueueDirective> method
		)
		{
			// Create the background operation.
			var backgroundOperation = this.CreateBackgroundOperation
			(
				name,
				method
			);
			
			// Start it running.
			backgroundOperation.RunAtIntervals(interval);

			// Return the background operation.
			return backgroundOperation;
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation CreateBackgroundOperation
		(
			string name, 
			Action method
		)
		{
			return this.CreateBackgroundOperation
			(
				name,
				(j, d) => method()
			);
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation CreateBackgroundOperation
		(
			string name, 
			Action<TaskProcessorJob> method
		)
		{
			return this.CreateBackgroundOperation
			(
				name,
				(j, d) => method(j)
			);
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation CreateBackgroundOperation
		(
			string name,
			Action<TaskProcessorJob, TaskQueueDirective> method
		)
		{
			TaskQueueBackgroundOperation backgroundOperation;

			lock (_lock)
			{
				if (this.BackgroundOperations.ContainsKey(name))
					throw new ArgumentException($"A background operation with the name {name} in queue {this.QueueId} could not be found.", nameof(name));

				// Create the background operation.
				backgroundOperation = new TaskQueueBackgroundOperation
				(
					this,
					name,
					method,
					this.CancellationTokenSource
				);

				// Add it to the dictionary.
				this.BackgroundOperations.Add(name, backgroundOperation);
			}

			// Return it.
			return backgroundOperation;
		}

	}
}