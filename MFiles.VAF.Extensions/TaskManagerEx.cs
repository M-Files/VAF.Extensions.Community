using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VaultApplications.Logging;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class TaskManagerEx<TConfiguration>
		: TaskManager
		where TConfiguration : class, new()
	{
		private ILogger Logger { get; }

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
			this.Logger = LogManager.GetLogger(this.GetType());
		}

		/// <inheritdoc />
		public new string AddTask(Vault vault, string queueId, string taskType, TaskDirective directive = null, DateTime? activationTime = null)
		{
			this.Logger?.Debug($"Adding task to queue {queueId} of type {taskType}.");
			return base.AddTask(vault, queueId, taskType, directive, activationTime);
		}

		/// <summary>
		/// Cancels any future executions of tasks of type <paramref name="taskType"/> on queue <paramref name="queueId"/>.
		/// If <paramref name="scheduleFor"/> has a value then a new execution of the task is scheduled for this date/time.
		/// </summary>
		/// <param name="queueId">The queue that the task should be rescheduled on.</param>
		/// <param name="taskType">The task type to be rescheduled.</param>
		/// <param name="innerDirective">The inner directive to be passed to the rescheduled task.</param>
		/// <param name="vault">The vault reference to use for the operation.</param>
		/// <param name="scheduleFor">The date/time to schedule a new execution for.  If <see langword="null"/>, does not schedule a future execution.</param>
		/// <remarks>Adds an item to the scheduling queue, so that only one server performs this operation.</remarks>
		public virtual void RescheduleTask(string queueId, string taskType, TaskDirective innerDirective = null, Vault vault = null, DateTime? scheduleFor = null)
			=> this.AddTask
			(
				vault ?? this.VaultApplication.PermanentVault,
				this.VaultApplication.GetSchedulerQueueID(),
				this.VaultApplication.GetRescheduleTaskType(),
				new RescheduleProcessorTaskDirective()
				{
					QueueID = queueId,
					TaskType = taskType,
					NextExecution = scheduleFor,
					InnerDirective = innerDirective
				}
			);

		/// <summary>
		/// Registers/opens a queue with ID provided by <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.GetSchedulerQueueID"/>
		/// and registers a process to handle tasks of type <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.GetRescheduleTaskType"/>.
		/// </summary>
		/// <remarks>
		/// This is a sequential queue, and the <see cref="HandleReschedule(ITaskProcessingJob{RescheduleProcessorTaskDirective})"/>
		/// method processes the rescheduling tasks.
		/// </remarks>
		public virtual void RegisterSchedulingQueue()
		{
			// Register the scheduler queue.
			this.Logger?.Trace($"Registering scheduler queue {this.VaultApplication.GetSchedulerQueueID()}");
			this.RegisterQueue
			(
				this.VaultApplication.GetSchedulerQueueID(),
				new[]
				{
						new TaskProcessor<RescheduleProcessorTaskDirective>
						(
							this.VaultApplication.GetRescheduleTaskType(),
							this.HandleReschedule,
							TransactionMode.Full
						)
				},
				MFTaskQueueProcessingBehavior.MFProcessingBehaviorSequential
			);
		}

		/// <summary>
		/// Cancels future executions of a task with a given queue ID and task type (read from the <paramref name="job"/>'s directive).
		/// If the directive also contains a next-execution date then reschedules an execution of the task at that time.
		/// </summary>
		/// <param name="job"></param>
		protected virtual void HandleReschedule(ITaskProcessingJob<RescheduleProcessorTaskDirective> job)
		{
			// Cancel any future executions.
			this.CancelAllFutureExecutions
			(
				job.Directive.QueueID,
				job.Directive.TaskType,
				includeCurrentlyExecuting: false,
				vault: job.Vault
			);

			// Re-schedule?
			if (job.Directive.NextExecution.HasValue)
				// Schedule the next run.
				this.AddTask
				(
					job.Vault,
					job.Directive.QueueID,
					job.Directive.TaskType,
					directive: job.Directive.InnerDirective,
					activationTime: job.Directive.NextExecution.Value
				);
		}

		private void TaskManagerEx_TaskEvent(object sender, TaskManagerEventArgs e)
		{
			if (null == e.Queues || e.Queues.Count == 0)
				return;

			// When the job is finished, re-schedule.
			switch (e.EventType)
			{
				case TaskManagerEventType.TaskJobStarted:
					// Log out that we started.
					this.Logger?.Trace($"Starting job(s) {string.Join(", ", e.Tasks?.Select(t => t.TaskID))}");
					break;
				case TaskManagerEventType.TaskJobFinished:
					//Log out that we're done.
					if(e.JobResult == TaskProcessingJobResult.Fatal)
					{
						// Something went badly wrong.
						this.Logger?.Error
						(
							e.Exception,
							$"Job(s) {string.Join(", ", e.Tasks?.Select(t => t.TaskID))} finished with a fatal result: {e.JobStatus.ErrorMessage}"
						);
					}
					else
						this.Logger?.Trace($"Job(s) {string.Join(", ", e.Tasks?.Select(t => t.TaskID))} finished ({e.JobResult})");

					// Re-schedule as appropriate.
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
								this.Logger?.Debug($"Re-scheduling {t.TaskType} on {t.QueueID} for {nextExecutionDate.Value}");
								this.RescheduleTask(t.QueueID, t.TaskType, vault: this.Vault, scheduleFor: nextExecutionDate);
							}
							break;
					}
					break;
			}
		}
	}

	[DataContract]
	public class RescheduleProcessorTaskDirective
		: TaskDirective
	{
		[DataMember]
		public string QueueID { get; set; }
		[DataMember]
		public string TaskType { get; set; }
		[DataMember]
		public DateTime? NextExecution { get; set; }
		[DataMember]
		public TaskDirective InnerDirective { get; set; }
	}
}
