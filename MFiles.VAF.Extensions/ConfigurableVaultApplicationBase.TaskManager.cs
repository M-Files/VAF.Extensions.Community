// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.MultiserverMode;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VaultApplications.Logging;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// Task manager.
		/// </summary>
		public new TaskManagerEx<TSecureConfiguration> TaskManager
		{
			get => base.TaskManager as TaskManagerEx<TSecureConfiguration>;
			protected set => base.TaskManager = value;
		}

		/// <summary>
		/// The queue ID to use for the recurring task scheduling.
		/// </summary>
		/// <returns></returns>
		public virtual string GetSchedulerQueueID()
			=> $"{this.TaskManager.Id}.extensions.scheduler";

		/// <summary>
		/// The task type to use for rescheduling tasks.
		/// </summary>
		/// <returns></returns>
		public virtual string GetRescheduleTaskType()
			=> $"reschedule";

		/// <summary>
		/// Gets the task manager to use.
		/// </summary>
		/// <returns></returns>
		protected virtual TaskManagerEx<TSecureConfiguration> GetTaskManager()
			=> new TaskManagerEx<TSecureConfiguration>
				(
					this,
					this.GetType().Namespace,
					this.PermanentVault,
					GetTransactionRunner()
				);

		/// <inheritdoc />
		protected override void InitializeTaskManager()
		{
			// Include our configuration updater if it is supported.
			if (CanBroadcastConfigurationChanges())
				this.TaskQueueResolver?.Include(new ConfigurationUpdater(this));

			// Report an error to the event log if queues were declared, but tasks aren't supported.
			if (AppTasks.TaskManager.IsSupported(PermanentVault))
			{
				// Initialize the new task manager, and register the queues from the task manager.
				this.TaskManager = this.GetTaskManager();
				this.TaskQueueResolver?.RegisterAll(this.TaskManager);
				TaskStatusHelper.Attach(this.TaskManager);

				// Register the scheduling queue.
				this.TaskManager?.RegisterSchedulingQueue();
			}
			else if (this.TaskQueueResolver != null && this.TaskQueueResolver.GetQueues().Length > 0)
			{
				// Report an error if task manager is not supported, but queues have been resolved.
				// If someone tries to declare queues programmatically, the TaskManager should be null,
				// and therefore throw its own errors.
				this.Logger?.Fatal("The application requires support for tasks. Please upgrade to M-Files 20.4 or later.");
			}
		}

		/// <inheritdoc />
		public override void Uninstall(Vault vaultSrc)
		{
			// For all queues/task-types that are running on a schedule/interval, cancel them now.
			if (null != this.RecurringOperationConfigurationManager)
			{
				foreach (var key in this.RecurringOperationConfigurationManager.Keys)
				{
					try
					{
						this.Logger?.Trace($"Cancelling future tasks on queue {key.QueueID} with type {key.TaskType}.");
						this.TaskManager?.CancelAllFutureExecutions(key.QueueID, key.TaskType, vault: vaultSrc);
					}
					catch (Exception e)
					{
						this.Logger?.Fatal
						(
							e,
							$"Could not cancel future executions of task type {key.TaskType} on queue {key.QueueID}."
						);
					}
				}
				this.RecurringOperationConfigurationManager.Clear();
			}

			// Call the base implementation.
			base.Uninstall(vaultSrc);
		}

		#region Update Configuration Broadcasts

		/// <summary>
		/// Simple class that includes the configuration changed broadcast handler.
		/// </summary>
		private class ConfigurationUpdater
		{
			/// <summary>
			/// The name of the broadcast type.
			/// </summary>
			public const string BroadcastType = "core.ConfigurationChanged";

			/// <summary>
			/// The application instance we are listening for configuration changes for.
			/// </summary>
			private readonly ConfigurableVaultApplicationBase<TSecureConfiguration> instance;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="instance">The application instance we are listening for configuration changes for.</param>
			public ConfigurationUpdater(
				ConfigurableVaultApplicationBase<TSecureConfiguration> instance
			)
			{
				// Set member.
				this.instance = instance;
			}

			/// <summary>
			/// Configuration updated broadcast message handler.
			/// </summary>
			/// <param name="job">Job containing broadcast messages to handle.</param>
			[BroadcastProcessor(BroadcastType, FilterMode = BroadcastFilterMode.FromOtherServersOnly)]
			public void OnConfigurationChangedBroadcast(IBroadcastProcessingJob<AppTasks.BroadcastDirective> job)
			{
				// Delegate in a separate thread.
				DateTime lastUpdated = job.Messages.Max(m => m.Directive.SentAt);
				Task.Run(() => this.instance.UpdateConfigurationFromVault(lastUpdated));
			}
		}

		/// <summary>
		/// Indicates whether it is possible for mulitle instance of the vault to run,
		/// signaling whether we should bother listening for configuration change broadcasts.
		/// </summary>
		/// <returns>True if configuration broadcasts should be listened for, false otherwise.</returns>
		private bool CanBroadcastConfigurationChanges()
		{
			return ApplicationDefinition.MultiServerCompatible;
		}

		/// <summary>
		/// Keeps track of when the configuration was last loaded,
		/// so we don't bother loading updates if they are stale.
		/// </summary>
		private DateTime configurationLastLoaded;

		/// <summary>
		/// Lock object to synchronize configuration loading/saving.
		/// </summary>
		private readonly object confLoadLock = new object();

		/// <summary>
		/// Updates the application's running configuration with the one stored in the vault,
		/// but only if the application hasn't already loaded the configuration since the
		/// last suspected change.
		/// Triggers the <see cref="OnConfigurationUpdated"/> virtual method if the configuration changed.
		/// </summary>
		/// <param name="lastUpdated">The time the configuration last changed in the vault.</param>
		private void UpdateConfigurationFromVault(
			DateTime lastUpdated
		)
		{
			// Any exception here is critical and must be logged.
			try
			{
				// Synchronize loading.
				lock (this.confLoadLock)
				{
					// Check if we should bother re-loading the configuration.
					if (lastUpdated > this.configurationLastLoaded)
					{
						// Keep track of the previous configuration.
						TSecureConfiguration oldConf = this.Configuration;

						// Load the new configuration.
						this.Configuration = LoadConfiguration(this.PermanentVault, false);
						this.configurationLastLoaded = DateTime.UtcNow;

						// Inform the app that its configuration just changed.
						OnConfigurationUpdated(oldConf, false);
					}
				}
			}
			catch (Exception e)
			{
				this.Logger?.Fatal
				(
					e,
					$"Failed to update configuration from vault."
				);
			}
		}

		#endregion

	}

}
