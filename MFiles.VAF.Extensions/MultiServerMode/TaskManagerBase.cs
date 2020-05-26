using System;
using System.Threading;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public abstract class TaskManagerBase
	{
		/// <summary>
		/// Lock for <see cref="CurrentServer"/>.
		/// </summary>
		private readonly object _lock = new object();

		/// <summary>
		/// The server that this application is running on.
		/// </summary>
		protected static VaultServerAttachment CurrentServer { get; private set; }

		/// <summary>
		/// The vault application that this task manager lives within.
		/// </summary>
		public VaultApplicationBase VaultApplication { get; protected set; }

		/// <summary>
		/// The queue ID that this manager handles.
		/// </summary>
		public string QueueId { get; protected set; }

		/// <summary>
		/// The task type that this manager handles.
		/// </summary>
		public string TaskType { get; protected set; }

		/// <summary>
		/// The cancellation token source.
		/// </summary>
		public CancellationTokenSource TokenSource { get; protected set; }

		/// <summary>
		/// The maximum polling frequency.
		/// </summary>
		public int MaxPollingInterval { get; protected set; } = 10;

		/// <summary>
		/// Whether to automatically register queues.
		/// </summary>
		public bool AutomaticallyRegisterQueues { get; protected set; } = true;

		protected TaskManagerBase
		(
			VaultApplicationBase vaultApplication,
			string queueId,
			string taskType,
			CancellationTokenSource tokenSource = null,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true
		)
		{
			// Set properties.
			this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
			this.QueueId = string.IsNullOrWhiteSpace(queueId)
				? throw new ArgumentException("The queue name must be provided.", nameof(queueId))
				: queueId;
			this.TaskType = string.IsNullOrWhiteSpace(taskType)
				? throw new ArgumentException("The task type must be provided.", nameof(taskType))
				: taskType;
			
			this.TokenSource = tokenSource == null
				? new CancellationTokenSource()
				: CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
			this.MaxPollingInterval = maxPollingInterval;
			this.AutomaticallyRegisterQueues = automaticallyRegisterQueues;

			// Ensure that we have a current server.
			lock (_lock)
			{
				if (null == TaskManagerBase.CurrentServer)
				{
					TaskManagerBase.CurrentServer
						= vaultApplication
							.PermanentVault
							.GetVaultServerAttachments()
							.GetCurrent();
				}
			}
		}
	}

	/// <summary>
	/// A base class for task managers to derive from.
	/// </summary>
	/// <typeparam name="TTaskProcessor">The managed task processor type.</typeparam>
	/// <typeparam name="TSettings">The type of settings used by <typeparamref name="TTaskProcessor"/>.</typeparam>
	/// <typeparam name="TDirective">The directive type in the queue.</typeparam>
	public abstract class TaskManagerBase<TTaskProcessor, TSettings, TDirective>
		: TaskManagerBase
		where TTaskProcessor : TaskProcessorBase<TSettings>
		where TSettings : AppTaskProcessorSettings
		where TDirective : TaskQueueDirective
	{

		/// <summary>
		/// The task processor being managed.
		/// </summary>
		private TTaskProcessor taskProcessor = null;

		/// <summary>
		/// The task processor that this manager wraps.
		/// </summary>
		public TTaskProcessor TaskProcessor
		{
			get
			{
				// If we do not have a task processor then create one.
				this.taskProcessor = this.taskProcessor ?? this.CreateTaskProcessor();
				return this.taskProcessor;
			}
			private set => this.taskProcessor = value;
		}

		/// <summary>
		/// The handler function that is called to process each job.
		/// </summary>
		public Action<TaskProcessorJob, TDirective> JobHandler { get; protected set; }

		protected TaskManagerBase
		(
			VaultApplicationBase vaultApplication,
			string queueId,
			string taskType,
			Action<TaskProcessorJob, TDirective> jobHandler,
			CancellationTokenSource tokenSource = null,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true
		)
		: base(vaultApplication, queueId, taskType, tokenSource, maxPollingInterval, automaticallyRegisterQueues)
		{
			// Set the job handler (not done by base implementation);
			this.JobHandler = jobHandler ?? throw new ArgumentNullException(nameof(jobHandler));
		}

		/// <summary>
		/// Creates a task processor of the correct type.
		/// </summary>
		/// <returns>The task processor.</returns>
		public abstract TTaskProcessor CreateTaskProcessor();

		/// <summary>
		/// Adds a task to the sequential task queue.
		/// </summary>
		/// <param name="directive">A custom directive to pass to the task, if needed.</param>
		/// <param name="allowRetry">Whether to allow retrying.</param>
		/// <returns>The ID of the task in the queue.</returns>
		public virtual string AddTaskToQueue
		(
			TDirective directive = null,
			bool allowRetry = true
		)
		{
			// Add it.
			return this.TaskProcessor.CreateApplicationTaskSafe
			(
				allowRetry,
				this.QueueId,
				this.TaskType,
				directive?.ToBytes()
			);
		}

		/// <summary>
		/// Updates the task information within the task queue for the <paramref name="task"/>.
		/// </summary>
		/// <param name="task">The task to update.</param>
		/// <param name="remarks">The remarks for the task.</param>
		/// <param name="state">The current task state.</param>
		/// <param name="appendRemarks">Append to any existing remarks (if true), or replace in full (if false).</param>
		public virtual void UpdateTaskInfo
		(
			ApplicationTask task,
			string remarks,
			MFTaskState state = MFTaskState.MFTaskStateInProgress,
			bool appendRemarks = false
		)
		{
			// Use the task processor implementation.
			this.TaskProcessor.UpdateTaskInfo
			(
				task,
				state,
				remarks,
				appendRemarks
			);
		}

		/// <summary>
		/// Updates the task information within the task queue for the <paramref name="job"/>.
		/// </summary>
		/// <param name="job">The job being processed.</param>
		/// <param name="remarks">The remarks for the task.</param>
		/// <param name="state">The current task state.</param>
		/// <param name="appendRemarks">Append to any existing remarks (if true), or replace in full (if false).</param>
		public void UpdateTaskInfo
		(
			TaskProcessorJob job,
			string remarks,
			MFTaskState state = MFTaskState.MFTaskStateInProgress,
			bool appendRemarks = false
		)
		{
			// Sanity.
			if (null == job)
				throw new ArgumentNullException(nameof(job));

			// Use the other overload.
			this.UpdateTaskInfo
			(
				job.Data?.Value,
				remarks,
				state,
				appendRemarks
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

			// Sanity.
			if (null == dir)
			{
				return;
			}

			// If it is a broadcast directive, then was it generated on the same server?
			// If so then ignore.
			{
				if (dir is BroadcastDirective broadcastDirective)
				{
					if (broadcastDirective.GeneratedFromGuid == TaskManagerBase.CurrentServer.ServerID)
						return;
				}
			}

			// Perform the action.
			try
			{
				// Delegate to the hander.
				this.JobHandler(job, dir);
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