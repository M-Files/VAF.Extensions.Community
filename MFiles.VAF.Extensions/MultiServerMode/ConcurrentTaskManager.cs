using System;
using System.Collections.Generic;
using System.Threading;
using MFiles.VAF;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// Allows easy creation/management of a concurrent task queue of a single task type.
	/// </summary>
	/// <typeparam name="TDirective">The directive type.</typeparam>
	public class ConcurrentTaskManager<TDirective>
		: TaskManagerBase<AppTaskBatchProcessor, AppTaskBatchProcessorSettings, TDirective>
		where TDirective : TaskQueueDirective
	{
		/// <summary>
		/// The maximum number of concurrent batches.
		/// </summary>
		protected int MaxConcurrentBatches { get; private set; } = 5;

		/// <summary>
		/// The maximum number of concurrent jobs.
		/// </summary>
		protected int MaxConcurrentJobs { get; private set; } = 5;

		/// <summary>
		/// Instantiates a <see cref="ConcurrentTaskManager{TDirective}"/>
		/// to process tasks of a single task type concurrently.
		/// </summary>
		/// <param name="vaultApplication">The vault application in which this manager lives.</param>
		/// <param name="queueId">The ID of the queue being managed.</param>
		/// <param name="taskType">The type of the task type being managed.</param>
		/// <param name="jobHandler">The handler for the job.</param>
		/// <param name="tokenSource">The token source.</param>
		/// <param name="maxConcurrentBatches">The maximum number of concurrent batches.</param>
		/// <param name="maxConcurrentJobs">The maximum number of concurrent jobs per batch.</param>
		/// <param name="maxPollingInterval">The maximum polling interval.</param>
		/// <param name="automaticallyRegisterQueues">Whether to automatically register the task queues and start processing.</param>
		public ConcurrentTaskManager
		(
			VaultApplicationBase vaultApplication,
			string queueId,
			string taskType,
			Action<TaskProcessorJob, TDirective> jobHandler,
			CancellationTokenSource tokenSource = null,
			int maxConcurrentBatches = 5,
			int maxConcurrentJobs = 5,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true
		)
			: base
			(
				vaultApplication, 
				queueId, 
				taskType, 
				jobHandler, 
				tokenSource,
				maxPollingInterval,
				automaticallyRegisterQueues
			)
		{
			this.MaxConcurrentBatches = maxConcurrentBatches;
			this.MaxConcurrentJobs = maxConcurrentJobs;
		}

		#region Overrides of TaskManagerBase<AppTaskBatchProcessor,AppTaskBatchProcessorSettings,TDirective>

		/// <inheritdoc />
		public override AppTaskBatchProcessor CreateTaskProcessor()
		{
			return this.VaultApplication.CreateConcurrentTaskProcessor
			(
				this.QueueId,
				new Dictionary<string, TaskProcessorJobHandler>
				{
					{ this.TaskType, this.ProcessJobHandler }
				},
				this.TokenSource,
				this.MaxConcurrentBatches,
				this.MaxConcurrentJobs,
				this.MaxPollingInterval,
				this.AutomaticallyRegisterQueues
			);
		}

		#endregion

	}
}