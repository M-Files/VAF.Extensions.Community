using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VaultApplications.Logging;
using MFilesAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static MFiles.VAF.Common.ApplicationTaskQueue.TaskQueueManager;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent
{
	/// <summary>
	/// Retrieves data from the <see cref="TaskManagerEx{TConfiguration}"/> about asynchronous operations that should
	/// be rendered onto a dashboard.
	/// </summary>
	/// <typeparam name="TConfiguration">The type of configuration used.</typeparam>
	public class TaskManagerExAsynchronousDashboardContentProvider<TConfiguration>
		: IAsynchronousDashboardContentProvider
		where TConfiguration : class, new()
	{
		private ILogger Logger { get; } = LogManager.GetLogger<TaskManagerExAsynchronousDashboardContentProvider<TConfiguration>>();
		protected Vault Vault { get; set; }
		public TaskManagerEx<TConfiguration> TaskManager { get; protected set; }
		public RecurringOperationConfigurationManager<TConfiguration> RecurringOperationConfigurationManager { get; protected set; }
		public TaskQueueResolver TaskQueueResolver { get; protected set; }
		public TaskManagerExAsynchronousDashboardContentProvider
		(
			Vault vault,
			TaskManagerEx<TConfiguration> taskManager,
			TaskQueueResolver taskQueueResolver,
			RecurringOperationConfigurationManager<TConfiguration> recurringOperationConfigurationManager
		)
		{
			Vault = vault ?? throw new ArgumentNullException(nameof(vault));
			TaskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
			TaskQueueResolver = taskQueueResolver ?? throw new ArgumentNullException(nameof(taskQueueResolver));
			RecurringOperationConfigurationManager = recurringOperationConfigurationManager ?? throw new ArgumentNullException(nameof(recurringOperationConfigurationManager));
		}

		/// <inheritdoc />
		public IEnumerable<KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>> GetAsynchronousDashboardContent()
		{
			if (null == TaskQueueResolver)
				yield break;

			foreach (var queue in TaskQueueResolver.GetQueues())
			{
				// Sanity.
				if (string.IsNullOrWhiteSpace(queue))
					continue;

				// Get information about the queues.
				System.Reflection.FieldInfo fieldInfo = null;
				try
				{
					fieldInfo = TaskQueueResolver.GetQueueFieldInfo(queue);
				}
				catch (Exception e)
				{
					// Throws if the queue is incorrect.
					Logger?.Warn
					(
						e,
						$"Cannot load details for queue {queue}; is there a static field with the [TaskQueue] attribute?"
					);
					continue;
				}


				// Skip anything broken.
				if (null == fieldInfo)
					continue;

				// If it's marked as hidden then skip.
				{
					var attributes = fieldInfo.GetCustomAttributes(typeof(HideOnDashboardAttribute), true)
						?? new HideOnDashboardAttribute[0];
					if (attributes.Length != 0)
						continue;
				}

				// Get the number of items in the queue.
				var waitingTasks = TaskHelper.GetTaskIDs(Vault, queue, MFTaskState.MFTaskStateWaiting).Count;
				var showDegraded = waitingTasks > AsynchronousDashboardContentSettings.DegradedDashboardThreshold;

				// Get each task processor.
				foreach (var processor in TaskQueueResolver.GetTaskProcessors(queue))
				{
					// Sanity.
					if (null == processor)
						continue;

					// Get information about the processor..
					TaskProcessorAttribute taskProcessorSettings = null;
					System.Reflection.MethodInfo methodInfo = null;
					try
					{
						taskProcessorSettings = TaskQueueResolver.GetTaskProcessorSettings(queue, processor.Type);
						methodInfo = TaskQueueResolver.GetTaskProcessorMethodInfo(queue, processor.Type);
					}
					catch
					{
						// Throws if the task processor is not found.
						Logger?.Warn
						(
							$"Cannot load processor details for task type {processor.Type} on queue {queue}."
						);
						continue;
					}


					// Skip anything broken.
					if (null == taskProcessorSettings || null == methodInfo)
						continue;

					// If it's marked as hidden then skip.
					{
						var attributes = methodInfo.GetCustomAttributes(typeof(HideOnDashboardAttribute), true)
							?? new HideOnDashboardAttribute[0];
						if (attributes.Length != 0)
							continue;
					}

					// This should be shown.  Do we have any extended details?
					var showOnDashboardAttribute = methodInfo.GetCustomAttributes(typeof(ShowOnDashboardAttribute), true)?
						.FirstOrDefault() as ShowOnDashboardAttribute;

					// Add the run command, if we should/can.
					var commands = new List<DashboardCommand>();
					if (showOnDashboardAttribute?.ShowRunCommand ?? false)
					{
						var key = $"{queue}-{processor.Type}";
						if (TaskManager.TaskQueueRunCommands.TryGetValue(key, out CustomDomainCommand cmd))
						{
							commands.Add
							(
								new DashboardDomainCommand
								{
									DomainCommandID = cmd.ID,
									Title = cmd.DisplayName,
									Style = DashboardCommandStyle.Link
								}
							);
						}
					}

					// Does it recur?
					IRecurrenceConfiguration recurrenceConfiguration = null;
					this.RecurringOperationConfigurationManager?
						.TryGetValue(queue, processor.Type, out recurrenceConfiguration);

					// Return the data.
					yield return new KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>
					(
						new DashboardQueueAndTaskDetails()
						{
							QueueId = queue,
							TaskType = processor.Type,
							Name = showOnDashboardAttribute?.Name,
							Description = showOnDashboardAttribute?.Description,
							Commands = commands,
							TasksInQueue = waitingTasks,
							RecurrenceConfiguration = recurrenceConfiguration
						},
						// Get known executions (prior, running and future).
						showDegraded
							? TaskManager.GetExecutions<TaskDirective>(queue, processor.Type, MFTaskState.MFTaskStateInProgress)
							: TaskManager.GetAllExecutions<TaskDirective>(queue, processor.Type)
					);

				}
			}
		}
	}
}
