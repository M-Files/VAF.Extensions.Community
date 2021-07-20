using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF;
using MFilesAPI;
using MFiles.VAF.MultiserverMode;

// ReSharper disable once CheckNamespace
namespace MFiles.VAF.Extensions
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
		internal static SequentialTaskProcessor CreateSequentialTaskProcessor
		(
			this VaultApplicationBase vaultApplication,
			AppTaskProcessorSettings processorSettings,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));
			if (null == processorSettings)
				throw new ArgumentNullException(nameof(processorSettings));
			if (processorSettings.QueueDef.TaskType != TaskQueueManager.TaskType.ApplicationTasks)
				throw new ArgumentException("The processor settings queue definition task type must be ApplicationTasks.", nameof(processorSettings));
			if (processorSettings.QueueDef.ProcessingBehavior != MFTaskQueueProcessingBehavior.MFProcessingBehaviorSequential)
				throw new ArgumentException("The processor settings queue definition processing behaviour must be MFProcessingBehaviorSequential.", nameof(processorSettings));
			if (null == processorSettings.TaskHandlers)
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(processorSettings));
			if (processorSettings.TaskHandlers.Count == 0)
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(processorSettings));
			if (processorSettings.TaskHandlers.Any(kvp => kvp.Value == null))
				throw new ArgumentException("Task handlers cannot be null.", nameof(processorSettings));

			// Ensure the integer values are valid.
			if (processorSettings.MaxConcurrentJobs <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent jobs must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.MaxConcurrentJobs = 5;
			}
			if (processorSettings.QueueDef.MaximumPollingIntervalInSeconds <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum polling interval must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.QueueDef.MaximumPollingIntervalInSeconds = 10;
			}

			// Create the processor.
			var processor = new SequentialTaskProcessor
			(
				processorSettings,
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
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true,
			string vaultExtensionProxyMethodId = null,
			CancellationTokenSource cancellationTokenSource = default
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

			// Create the processor settings.
			var processorSettings = new AppTaskProcessorSettings
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
				TaskQueueManager = vaultApplication.TaskQueueManager,
				VaultExtensionMethodProxyId = vaultExtensionProxyMethodId
			};

			// Use the other overload.
			return vaultApplication.CreateSequentialTaskProcessor
			(
				processorSettings,
				automaticallyRegisterQueues,
				automaticallyStartPolling,
				cancellationTokenSource
			);
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
		internal static AppTaskBatchProcessor CreateConcurrentTaskProcessor
		(
			this VaultApplicationBase vaultApplication,
			AppTaskBatchProcessorSettings processorSettings,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));
			if (null == processorSettings)
				throw new ArgumentNullException(nameof(processorSettings));
			if (processorSettings.QueueDef.TaskType != TaskQueueManager.TaskType.ApplicationTasks)
				throw new ArgumentException("The processor settings queue definition task type must be ApplicationTasks.", nameof(processorSettings));
			if (processorSettings.QueueDef.ProcessingBehavior != MFTaskQueueProcessingBehavior.MFProcessingBehaviorConcurrent)
				throw new ArgumentException("The processor settings queue definition processing behaviour must be MFProcessingBehaviorConcurrent.", nameof(processorSettings));
			if (null == processorSettings.TaskHandlers)
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(processorSettings));
			if (processorSettings.TaskHandlers.Count == 0)
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(processorSettings));
			if (processorSettings.TaskHandlers.Any(kvp => kvp.Value == null))
				throw new ArgumentException("Task handlers cannot be null.", nameof(processorSettings));

			// Ensure the integer values are valid.
			if (processorSettings.MaxConcurrentBatches <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent batches must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.MaxConcurrentBatches = 5;
			}
			if (processorSettings.MaxConcurrentJobs <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent jobs must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.MaxConcurrentJobs = 5;
			}
			if (processorSettings.QueueDef.MaximumPollingIntervalInSeconds <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum polling interval must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.QueueDef.MaximumPollingIntervalInSeconds = 10;
			}

			// Create the processor.
			var processor = new AppTaskBatchProcessor
			(
				processorSettings,
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
			int maxConcurrentBatches = 5,
			int maxConcurrentJobs = 5,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true,
			bool enableAutomaticTaskUpdates = true,
			CancellationTokenSource cancellationTokenSource = default
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
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(taskHandlers));
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

			// Create the processor settings.
			var processorSettings = new AppTaskBatchProcessorSettings
			{
				DisableAutomaticProgressUpdates = false,
				PollTasksOnJobCompletion = true,
				MaxConcurrentBatches = maxConcurrentBatches,
				MaxConcurrentJobs = maxConcurrentJobs,
				PermanentVault = vaultApplication.PermanentVault,
				EnableAutomaticTaskUpdates = enableAutomaticTaskUpdates,
				QueueDef = new TaskQueueDef
				{
					TaskType = TaskQueueManager.TaskType.ApplicationTasks,
					Id = queueId,
					ProcessingBehavior = MFTaskQueueProcessingBehavior.MFProcessingBehaviorConcurrent,
					MaximumPollingIntervalInSeconds = maxPollingInterval,
					LastBroadcastId = ""
				},
				TaskHandlers = taskHandlers,
				TaskQueueManager = vaultApplication.TaskQueueManager,
				VaultExtensionMethodProxyId = vaultApplication.GetVaultExtensionMethodEventHandlerProxyName()
			};

			// Create the processor.
			var processor = vaultApplication.CreateConcurrentTaskProcessor
			(
				processorSettings,
				automaticallyRegisterQueues,
				automaticallyStartPolling,
				cancellationTokenSource
			);

			// Return the processor.
			return processor;
		}

		/// <summary>
		/// Creates an instance of <see cref="AppTaskBatchProcessor"/> for
		/// broadcast task processing, using common configuration settings.
		/// </summary>
		/// <param name="vaultApplication">The vault application that this task processor is associated with.</param>
		/// <param name="processorSettings">The settings for the task processor.</param>
		/// <param name="automaticallyRegisterQueues">If true, automatically calls <see cref="AppTaskBatchProcessor.RegisterTaskQueues"/>.</param>
		/// <param name="automaticallyStartPolling">If true, automatically calls <see cref="TaskQueueManager.EnableTaskPolling"/>.</param>
		/// <returns>The broadcast batch processor.</returns>
		public static AppTaskBatchProcessor CreateBroadcastTaskProcessor
		(
			this VaultApplicationBase vaultApplication,
			AppTaskBatchProcessorSettings processorSettings,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));
			if (null == processorSettings)
				throw new ArgumentNullException(nameof(processorSettings));
			if (processorSettings.QueueDef.TaskType != TaskQueueManager.TaskType.BroadcastMessages)
				throw new ArgumentException("The processor settings queue definition task type must be BroadcastMessages.", nameof(processorSettings));
			if (null == processorSettings.TaskHandlers)
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(processorSettings));
			if (processorSettings.TaskHandlers.Count == 0)
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(processorSettings));
			if (processorSettings.TaskHandlers.Any(kvp => kvp.Value == null))
				throw new ArgumentException("Task handlers cannot be null.", nameof(processorSettings));

			// Ensure the integer values are valid.
			if (processorSettings.MaxConcurrentBatches <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent batches must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.MaxConcurrentBatches = 5;
			}
			if (processorSettings.MaxConcurrentJobs <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum concurrent jobs must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.MaxConcurrentJobs = 5;
			}
			if (processorSettings.QueueDef.MaximumPollingIntervalInSeconds <= 0)
			{
				SysUtils.ReportToEventLog
				(
					"The maximum polling interval must be a positive integer; using default.",
					System.Diagnostics.EventLogEntryType.Warning
				);
				processorSettings.QueueDef.MaximumPollingIntervalInSeconds = 10;
			}

			// Create the processor.
			var processor = new AppTaskBatchProcessor
			(
				processorSettings,
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
		/// <param name="vaultExtensionProxyMethodId">The Id of the vault extension method proxy to use for re-broadcasts.</param>
		/// <returns>The broadcast batch processor.</returns>
		public static AppTaskBatchProcessor CreateBroadcastTaskProcessor
		(
			this VaultApplicationBase vaultApplication,
			string queueId,
			Dictionary<string, TaskProcessorJobHandler> taskHandlers,
			int maxConcurrentBatches = 5,
			int maxConcurrentJobs = 5,
			int maxPollingInterval = 10,
			bool automaticallyRegisterQueues = true,
			bool automaticallyStartPolling = true,
			CancellationTokenSource cancellationTokenSource = default
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
				throw new ArgumentException("The processor settings must have at least one task handler defined.", nameof(taskHandlers));
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

			// Create the processor settings.
			var processorSettings = new AppTaskBatchProcessorSettings
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
				VaultExtensionMethodProxyId = vaultApplication.GetVaultExtensionMethodEventHandlerProxyName()
			};

			// Create the processor.
			var processor = vaultApplication.CreateBroadcastTaskProcessor
			(
				processorSettings,
				automaticallyRegisterQueues,
				automaticallyStartPolling,
				cancellationTokenSource
			);

			// Return the processor.
			return processor;
		}
	}
}
