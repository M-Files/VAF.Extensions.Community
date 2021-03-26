// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// A base class that automatically implements the pattern required for broadcasting
	/// configuration changes to other servers.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	/// <remarks>See https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes for further details.</remarks>
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>, IUsesTaskQueue
	where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// The rebroadcast queue Id.
		/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
		/// </summary>
		protected string ConfigurationRebroadcastQueueId { get; private set; }

		/// <summary>
		/// The rebroadcast queue processor.
		/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
		/// </summary>
		protected AppTaskBatchProcessor ConfigurationRebroadcastTaskProcessor { get; private set; }

		/// <summary>
		/// The task queue background operation manager for this application.
		/// </summary>
		protected TaskQueueBackgroundOperationManager TaskQueueBackgroundOperationManager { get; private set; }

		/// <inheritdoc />
		public override string GetRebroadcastQueueId()
		{
			// If we do not have a rebroadcast queue for the configuration data
			// then create one.
			if (null == this.ConfigurationRebroadcastTaskProcessor)
			{
				// Enable the configuration rebroadcasting.
				this.EnableConfigurationRebroadcasting
					(
					out AppTaskBatchProcessor processor,
					out string queueId
					);

				// Populate references to the task processor and queue Id.
				this.ConfigurationRebroadcastQueueId = queueId;
				this.ConfigurationRebroadcastTaskProcessor = processor;
			}

			// Return the broadcast queue Id.
			return this.ConfigurationRebroadcastQueueId;
		}

		#region Implementation of IUsesTaskQueue

		/// <inheritdoc />
		public virtual void RegisterTaskQueues()
		{
		}

		#endregion

		/// <inheritdoc />
		protected override void StartApplication()
		{
			base.StartApplication();

			// Instantiate the background operation manager.
			this.TaskQueueBackgroundOperationManager = new TaskQueueBackgroundOperationManager
			(
				this
			);
		}

		/// <inheritdoc />
		protected override void UninitializeApplication(Vault vault)
		{
			// When the application is uninstalled we should cancel any future executions, to stop any oddities
			// where a future application with the same name (or a re-install) picks up the old task queue.
			// This is especially important where the task IDs may change but the queue stays the same,
			// as this would throw exceptions because the new background operation manager cannot find
			// a processor for the old task ID.
			try
			{
				this.TaskQueueBackgroundOperationManager?.CancelFutureExecutions(remarks: "Application being uninstalled.");
			}
			catch
			{
			}

			base.UninitializeApplication(vault);
		}
	}
}
