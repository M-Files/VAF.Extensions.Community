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

		protected override TaskManager CreateTaskManager()
			=> new TaskManagerEx<TSecureConfiguration>
				(
					this,
					this.GetType().Namespace,
					this.PermanentVault,
					GetTransactionRunner()
				);

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

	}

}
