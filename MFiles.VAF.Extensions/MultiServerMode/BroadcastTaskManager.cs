using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// Allows easy creation/management of a broadcast task queue of a single task type.
	/// </summary>
	/// <typeparam name="TDirective">The directive type.</typeparam>
	public class BroadcastTaskManager<TDirective>
		: ConcurrentTaskManager<TDirective>
		where TDirective : BroadcastDirective
	{
		/// <inheritdoc />
		public BroadcastTaskManager
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
				maxConcurrentBatches,
				maxConcurrentJobs,
				maxPollingInterval, 
				automaticallyRegisterQueues
			)
		{
		}

		#region Overrides of TaskManagerBase<AppTaskBatchProcessor,AppTaskBatchProcessorSettings,TDirective>

		/// <inheritdoc />
		public override AppTaskBatchProcessor CreateTaskProcessor()
		{
			return this.VaultApplication.CreateBroadcastTaskProcessor
			(
				this.QueueId,
				new Dictionary<string, TaskProcessorJobHandler>
				{
					{ this.TaskType, this.ProcessJobHandler }
				},
				this.TokenSource?.Token ?? default,
				this.MaxConcurrentBatches,
				this.MaxConcurrentJobs,
				this.MaxPollingInterval,
				this.AutomaticallyRegisterQueues
			);
		}

		#endregion

	}
}