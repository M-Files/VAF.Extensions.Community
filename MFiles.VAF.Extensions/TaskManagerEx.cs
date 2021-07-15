using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class TaskManagerEx<TConfiguration>
		: TaskManager
		where TConfiguration : class, new()
	{
		/// <summary>
		/// The vault application used to create this task manager.
		/// </summary>
		protected ConfigurableVaultApplicationBase<TConfiguration> VaultApplication { get; private set; }

		public TaskManagerEx
		(
			ConfigurableVaultApplicationBase<TConfiguration> vaultApplication,
			string id, 
			Vault permanentVault, 
			IVaultTransactionRunner transactionRunner,
			TimeSpan? processingInterval = null,
			uint maxConcurrency = 16,
			TimeSpan? maxLockWaitTime = null,
			TaskExceptionSettings exceptionSettings = null
		)
			: base(id, permanentVault, transactionRunner, processingInterval, maxConcurrency, maxLockWaitTime, exceptionSettings)
		{
			this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
			this.TaskEvent += TaskManagerEx_TaskEvent;
		}

		private void TaskManagerEx_TaskEvent(object sender, TaskManagerEventArgs e)
		{
			if (null == e.Queues || e.Queues.Count == 0)
				return;

			// When the job is finished, re-schedule.
			switch (e.EventType)
			{
				case TaskManagerEventType.TaskJobStarted:
					break;
				case TaskManagerEventType.TaskJobFinished:
					switch (e.JobResult)
					{
						case TaskProcessingJobResult.Complete:
						case TaskProcessingJobResult.Fatal:
							// Re-schedule.
							foreach (var t in e.Tasks)
							{
								// Are there any future executions scheduled?
								if (this.GetPendingExecutions<TaskDirective>(t.QueueID, t.TaskType, includeCurrentlyExecuting: false).Any())
									continue; // We already have one scheduled; don't re-schedule.

								// Can we get a next execution date for this task?
								var nextExecutionDate = this
									.VaultApplication?
									.RecurringOperationConfigurationManager?
									.GetNextTaskProcessorExecution(t.QueueID, t.TaskType);
								if (false == nextExecutionDate.HasValue)
									continue;							

								// Schedule.
								this.AddTask(this.Vault, t.QueueID, t.TaskType, activationTime: nextExecutionDate);
							}
							break;
					}
					break;
			}
		}
	}
}
