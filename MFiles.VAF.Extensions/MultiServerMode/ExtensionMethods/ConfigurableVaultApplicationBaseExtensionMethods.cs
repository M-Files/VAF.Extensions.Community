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
		/// <param name="maxConcurrentBatches">The maximum number of concurrent batches.</param>
		/// <param name="maxConcurrentJobs">The maximum number of concurrent jobs.</param>
		/// <param name="maxPollingInterval">The maximum polling interval.</param>
		/// <param name="cancellationTokenSource">The token source for cancellation.</param>
		/// <returns>The queue ID for rebroadcasting.</returns>
		public static string EnableConfigurationRebroadcasting<TSecureConfiguration>
		(
			this ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication,
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

			// Set up the broadcast task queue ID.  This is specific for this application.
			var broadcastTaskQueueId = $"{vaultApplication.GetType().FullName.Replace(".", "-")}-ConfigurationRebroadcastQueue";

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
				// This does not require any task handlers, but if other broadcast tasks are used then they could be added here.
				TaskHandlers = new Dictionary<string, TaskProcessorJobHandler>()
				{
					// Note that we have to provide at least one task handler or the underlying call excepts.
					{ Guid.NewGuid().ToString(), (j) => { } }
				},
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
			var broadcastProcessor = vaultApplication.CreateBroadcastTaskProcessor
			(
				processorSettings,
				cancellationTokenSource: cancellationTokenSource
			);

			// Return the queue ID.
			return broadcastTaskQueueId;
		}
	}
}
