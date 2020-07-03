using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;

// ReSharper disable once CheckNamespace
namespace MFiles.VAF.Extensions.MultiServerMode
{
	public static class VaultApplicationBaseExtensionMethods
	{
		/// <summary>
		/// Creates an instance of <see cref="SequentialTaskProcessor"/> for
		/// sequential task processing, using common configuration settings.
		/// </summary>
		/// <param name="vaultApplication">The vault application this task processor is associated with.</param>
		/// <param name="queueId">The queue Id (must be unique in the vault) that this application is processing.</param>
		/// <param name="taskHandlers">The task Ids and handlers that this processor can handle.</param>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		/// <param name="maxPollingInterval">The maximum interval (in seconds) between polling.</param>
		/// <param name="automaticallyRegisterQueues">If true, automatically calls <see cref="AppTaskBatchProcessor.RegisterTaskQueues"/>.</param>
		/// <param name="automaticallyStartPolling">If true, automatically calls <see cref="TaskQueueManager.EnableTaskPolling"/>.</param>
		/// <returns>The sequential batch processor.</returns>
		public static SequentialTaskProcessor CreateSequentialTaskProcessor
		(
			this VaultApplicationBase vaultApplication,
			string queueId,
			Dictionary<string, TaskProcessorJobHandler> taskHandlers,
			CancellationTokenSource cancellationTokenSource = default,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true
		)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));
			if (string.IsNullOrWhiteSpace(queueId))
				throw new ArgumentException("A queue Id must be provided.", nameof(queueId));
			if (null == taskHandlers)
				throw new ArgumentNullException(nameof(taskHandlers));
			if (taskHandlers.Count == 0)
				throw new ArgumentException("No task handlers were registered.", nameof(taskHandlers));
			if (taskHandlers.Count == 0)
				throw new ArgumentException("No task handlers were registered.", nameof(taskHandlers));
			if (taskHandlers.Any(kvp => kvp.Value == null))
				throw new ArgumentException("Task handlers cannot be null.", nameof(taskHandlers));

			// Ensure the integer values are valid.
			if (maxPollingInterval <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum polling interval must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				maxPollingInterval = 10;
			}

			// Create the processor.
			var processor = new SequentialTaskProcessor(
				new AppTaskProcessorSettings
				{
					PollTasksOnJobCompletion = true,
					MaxConcurrentJobs = 1, // Always 1 for a sequential task processor.
					PermanentVault = vaultApplication.PermanentVault,
					EnableAutomaticTaskUpdates = true,
					QueueDef = new TaskQueueDef
					{
						TaskType = TaskQueueManager.TaskType.ApplicationTasks,
						Id = queueId,
						ProcessingBehavior = MFTaskQueueProcessingBehavior.MFProcessingBehaviorSequential,
						MaximumPollingIntervalInSeconds = maxPollingInterval,
						LastBroadcastId = ""
					},
					TaskHandlers = taskHandlers,
					TaskQueueManager = vaultApplication.TaskQueueManager
				},
				cancellationTokenSource?.Token ?? default
			);

			// Should we automatically register the task queues?
			if (automaticallyRegisterQueues)
				processor.RegisterTaskQueues();

			// Enable polling/processing of the queue.
			if(automaticallyStartPolling)
				vaultApplication.TaskQueueManager.EnableTaskPolling(true);

			// Return the processor.
			return processor;
		}

		/// <summary>
		/// Creates an instance of <see cref="AppTaskBatchProcessor"/> for
		/// concurrent task processing, using common configuration settings.
		/// </summary>
		/// <param name="vaultApplication">The vault application this task processor is associated with.</param>
		/// <param name="queueId">The queue Id (must be unique in the vault) that this application is processing.</param>
		/// <param name="taskHandlers">The task Ids and handlers that this processor can handle.</param>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		/// <param name="maxConcurrentBatches">The maximum concurrent batches (defaults to 5).</param>
		/// <param name="maxConcurrentJobs">The maximum number of concurrent jobs per batch (defaults to 5).</param>
		/// <param name="maxPollingInterval">The maximum interval (in seconds) between polling.</param>
		/// <param name="automaticallyRegisterQueues">If true, automatically calls <see cref="AppTaskBatchProcessor.RegisterTaskQueues"/>.</param>
		/// <param name="automaticallyStartPolling">If true, automatically calls <see cref="TaskQueueManager.EnableTaskPolling"/>.</param>
		/// <returns>The concurrent batch processor.</returns>
		public static AppTaskBatchProcessor CreateConcurrentTaskProcessor
		(
			this VaultApplicationBase vaultApplication,
			string queueId,
			Dictionary<string, TaskProcessorJobHandler> taskHandlers,
			CancellationTokenSource cancellationTokenSource = default,
			int maxConcurrentBatches = 5,
			int maxConcurrentJobs = 5,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true
		)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));
			if (string.IsNullOrWhiteSpace(queueId))
				throw new ArgumentException("A queue Id must be provided.", nameof(queueId));
			if (null == taskHandlers)
				throw new ArgumentNullException(nameof(taskHandlers));
			if (taskHandlers.Count == 0)
				throw new ArgumentException("No task handlers were registered.", nameof(taskHandlers));
			if (taskHandlers.Count == 0)
				throw new ArgumentException("No task handlers were registered.", nameof(taskHandlers));
			if (taskHandlers.Any(kvp => kvp.Value == null))
				throw new ArgumentException("Task handlers cannot be null.", nameof(taskHandlers));

			// Ensure the integer values are valid.
			if (maxConcurrentBatches <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent batches must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				maxConcurrentBatches = 5;
			}
			if (maxConcurrentJobs <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent jobs must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				maxConcurrentJobs = 5;
			}
			if (maxPollingInterval <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum polling interval must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				maxPollingInterval = 10;
			}

			// Create the processor.
			var processor = new AppTaskBatchProcessor
			(
				new AppTaskBatchProcessorSettings
				{
					DisableAutomaticProgressUpdates = false,
					PollTasksOnJobCompletion = true,
					MaxConcurrentBatches = maxConcurrentBatches,
					MaxConcurrentJobs = maxConcurrentJobs,
					PermanentVault = vaultApplication.PermanentVault,
					EnableAutomaticTaskUpdates = true,
					QueueDef = new TaskQueueDef
					{
						TaskType = TaskQueueManager.TaskType.ApplicationTasks,
						Id = queueId,
						ProcessingBehavior = MFTaskQueueProcessingBehavior.MFProcessingBehaviorConcurrent,
						MaximumPollingIntervalInSeconds = maxPollingInterval,
						LastBroadcastId = ""
					},
					TaskHandlers = taskHandlers,
					TaskQueueManager = vaultApplication.TaskQueueManager
				},
				cancellationTokenSource?.Token ?? default
			);

			// Should we automatically register the task queues?
			if (automaticallyRegisterQueues)
				processor.RegisterTaskQueues();

			// Enable polling/processing of the queue.
			if(automaticallyStartPolling)
				vaultApplication.TaskQueueManager.EnableTaskPolling(true);

			// Return the processor.
			return processor;
		}

		/// <summary>
		/// Creates an instance of <see cref="AppTaskBatchProcessor"/> for
		/// broadcast task processing, using common configuration settings.
		/// </summary>
		/// <param name="vaultApplication">The vault application that this task processor is associated with.</param>
		/// <param name="queueId">The queue Id (must be unique in the vault) that this application is processing.</param>
		/// <param name="taskHandlers">The task Ids and handlers that this processor can handle.</param>
		/// <param name="cancellationToken">The cancellation token used for cancelling ongoing operations.</param>
		/// <param name="maxConcurrentBatches">The maximum concurrent batches (defaults to 5).</param>
		/// <param name="maxConcurrentJobs">The maximum number of concurrent jobs per batch (defaults to 5).</param>
		/// <param name="maxPollingInterval">The maximum interval (in seconds) between polling.</param>
		/// <param name="automaticallyRegisterQueues">If true, automatically calls <see cref="AppTaskBatchProcessor.RegisterTaskQueues"/>.</param>
		/// <param name="automaticallyStartPolling">If true, automatically calls <see cref="TaskQueueManager.EnableTaskPolling"/>.</param>
		/// <returns>The broadcast batch processor.</returns>
		public static AppTaskBatchProcessor CreateBroadcastTaskProcessor
		(
			this VaultApplicationBase vaultApplication,
			string queueId,
			Dictionary<string, TaskProcessorJobHandler> taskHandlers,
			CancellationToken cancellationToken = default(CancellationToken),
			int maxConcurrentBatches = 5,
			int maxConcurrentJobs = 5,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true,
			string vaultExtensionProxyMethodId = null
		)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));
			if (string.IsNullOrWhiteSpace(queueId))
				throw new ArgumentException("A queue Id must be provided.", nameof(queueId));
			if (null == taskHandlers)
				throw new ArgumentNullException(nameof(taskHandlers));
			if (taskHandlers.Count == 0)
				throw new ArgumentException("No task handlers were registered.", nameof(taskHandlers));
			if (taskHandlers.Count == 0)
				throw new ArgumentException("No task handlers were registered.", nameof(taskHandlers));
			if (taskHandlers.Any(kvp => kvp.Value == null))
				throw new ArgumentException("Task handlers cannot be null.", nameof(taskHandlers));

			// Ensure the integer values are valid.
			if (maxConcurrentBatches <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent batches must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				maxConcurrentBatches = 5;
			}
			if (maxConcurrentJobs <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent jobs must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				maxConcurrentJobs = 5;
			}
			if (maxPollingInterval <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum polling interval must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				maxPollingInterval = 10;
			}

			// Create the processor.
			var processor = new AppTaskBatchProcessor(
				new AppTaskBatchProcessorSettings
				{
					QueueDef = new TaskQueueDef
					{
						TaskType = TaskQueueManager.TaskType.BroadcastMessages,
						Id = queueId,
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
					VaultExtensionMethodProxyId = vaultExtensionProxyMethodId
				},
				cancellationToken
			);

			// Should we automatically register the task queues?
			if (automaticallyRegisterQueues)
				processor.RegisterTaskQueues();

			// Enable polling/processing of the queue.
			if(automaticallyStartPolling)
				vaultApplication.TaskQueueManager.EnableTaskPolling(true);

			// Return the processor.
			return processor;
		}
	}
}
