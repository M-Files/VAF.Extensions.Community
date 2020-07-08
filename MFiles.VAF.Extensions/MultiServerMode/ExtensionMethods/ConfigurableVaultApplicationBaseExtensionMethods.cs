using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods
{
	public static class ConfigurableVaultApplicationBaseExtensionMethods
	{
		/// <summary>
		/// Enables rebroadcasting of the configuration data to all servers.
		/// </summary>
		/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
		/// <param name="vaultApplication">The vault application to enable rebroadcasting for.</param>
		/// <param name="broadcastTaskProcessor">The processor used to process the configuration rebroadcasting queue.</param>
		/// <param name="broadcastTaskQueueId">The queue Id used for the configuration rebroadcasting.</param>
		/// <param name="taskHandlers">Handlers for any additional tasks that the queue should handle.</param>
		/// <param name="maxConcurrentBatches">The maximum number of concurrent batches.</param>
		/// <param name="maxConcurrentJobs">The maximum number of concurrent jobs.</param>
		/// <param name="maxPollingInterval">The maximum polling interval.</param>
		/// <param name="cancellationTokenSource">The token source for cancellation.</param>
		public static void EnableConfigurationRebroadcasting<TSecureConfiguration>
		(
			this ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication,
			out AppTaskBatchProcessor broadcastTaskProcessor,
			out string broadcastTaskQueueId,
			Dictionary<string, TaskProcessorJobHandler> taskHandlers = null,
			int maxConcurrentBatches = 5,
			int maxConcurrentJobs = 5,
			int maxPollingInterval = 10,
			CancellationTokenSource cancellationTokenSource = default
		)
			where TSecureConfiguration : class, new()
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));

			// We must have at least one task handler or the underlying implementation throws.
			taskHandlers = taskHandlers ?? new Dictionary<string, TaskProcessorJobHandler>();
			if (taskHandlers.Count == 0)
				taskHandlers.Add(Guid.NewGuid().ToString(), (j) => { });

			// Set up the broadcast task queue ID.  This is specific for this application.
			broadcastTaskQueueId = $"{vaultApplication.GetType().FullName.Replace(".", "-")}-ConfigurationRebroadcastQueue";

			// Create the settings instance.
			var processorSettings = new AppTaskBatchProcessorSettings
			{
				QueueDef = new TaskQueueDef
				{
					TaskType = TaskQueueManager.TaskType.BroadcastMessages,
					Id = broadcastTaskQueueId,
					ProcessingBehavior = MFTaskQueueProcessingBehavior.MFProcessingBehaviorConcurrent,
					MaximumPollingIntervalInSeconds = maxPollingInterval,
					LastBroadcastId = ""
				},
				PermanentVault = vaultApplication.PermanentVault,
				MaxConcurrentBatches = maxConcurrentBatches,
				MaxConcurrentJobs = maxConcurrentJobs,
				TaskHandlers = taskHandlers,
				TaskQueueManager = vaultApplication.TaskQueueManager,
				EnableAutomaticTaskUpdates = true,
				DisableAutomaticProgressUpdates = false,
				PollTasksOnJobCompletion = true,
				VaultExtensionMethodProxyId = vaultApplication.GetVaultExtensionMethodEventHandlerProxyName()
			};

			// Set up the cancellation token source.
			cancellationTokenSource = cancellationTokenSource == null
				? new CancellationTokenSource()
				: CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
			
			// Create the broadcast processor using the vault extension method.
			broadcastTaskProcessor = vaultApplication.CreateBroadcastTaskProcessor
			(
				processorSettings,
				cancellationTokenSource: cancellationTokenSource
			);
		}
	}
}
