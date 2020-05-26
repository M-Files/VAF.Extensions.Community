using System;
using System.Collections.Generic;
using System.Threading;
using MFiles.VAF;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// Allows easy creation/management of a sequential task queue of a single task type.
	/// </summary>
	/// <typeparam name="TDirective">The directive type.</typeparam>
	public class SequentialTaskManager<TDirective>
		: TaskManagerBase<SequentialTaskProcessor, AppTaskProcessorSettings, TDirective>
		where TDirective : TaskQueueDirective
	{
		/// <summary>
		/// Instantiates a <see cref="SequentialTaskManager{TDirective}"/>
		/// to process tasks of a single task type sequentially.
		/// </summary>
		/// <param name="vaultApplication">The vault application in which this manager lives.</param>
		/// <param name="queueId">The ID of the queue being managed.</param>
		/// <param name="taskType">The type of the task type being managed.</param>
		/// <param name="jobHandler">The handler for the job.</param>
		/// <param name="tokenSource">The token source.</param>
		/// <param name="maxPollingInterval">The maximum polling interval.</param>
		/// <param name="automaticallyRegisterQueues">Whether to automatically register the task queues and start processing.</param>
		public SequentialTaskManager
		(
			VaultApplicationBase vaultApplication,
			string queueId,
			string taskType,
			Action<TaskProcessorJob, TDirective> jobHandler,
			CancellationTokenSource tokenSource = null,
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
		}

		#region Overrides of TaskManagerBase<AppTaskBatchProcessor,AppTaskBatchProcessorSettings,TDirective>

		/// <inheritdoc />
		public override SequentialTaskProcessor CreateTaskProcessor()
		{
			return this.VaultApplication.CreateSequentialTaskProcessor
			(
				this.QueueId,
				new Dictionary<string, TaskProcessorJobHandler>
				{
					{ this.TaskType, this.ProcessJobHandler }
				},
				this.TokenSource,
				this.MaxPollingInterval,
				this.AutomaticallyRegisterQueues
			);
		}

		#endregion

	}
}