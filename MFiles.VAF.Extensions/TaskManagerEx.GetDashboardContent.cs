using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VAF.Extensions.ScheduledExecution;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class TaskManagerEx<TConfiguration>
	{
		/// <summary>
		/// The number of waiting tasks in a single queue at which point the dashboard is shown degraded.
		/// </summary>
		private const int DegradedDashboardThreshold = 3000;

		/// <summary>
		/// Returns some dashboard content that shows the background operations and their current status.
		/// </summary>
		/// <returns>The dashboard content.</returns>
		public IEnumerable<DashboardListItem> GetDashboardContent(TaskQueueResolver taskQueueResolver)
		{
			if (null == taskQueueResolver)
				yield break;

			foreach (var queue in taskQueueResolver.GetQueues())
			{
				// Sanity.
				if (string.IsNullOrWhiteSpace(queue))
					continue;

				// Get information about the queues.
				TaskQueueAttribute queueSettings = null;
				System.Reflection.FieldInfo fieldInfo = null;
				try
				{
					queueSettings = taskQueueResolver.GetQueueSettings(queue);
					fieldInfo = taskQueueResolver.GetQueueFieldInfo(queue);
				}
				catch
				{
					// Throws if the queue is incorrect.
					this.Logger?.Warn
					(
						$"Cannot load details for queue {queue}; is there a static field with the [TaskQueue] attribute?"
					);
					continue;
				}
				

				// Skip anything broken.
				if (null == queueSettings || null == fieldInfo)
					continue;

				// If it's marked as hidden then skip.
				{
					var attributes = fieldInfo.GetCustomAttributes(typeof(HideOnDashboardAttribute), true)
						?? new HideOnDashboardAttribute[0];
					if (attributes.Length != 0)
						continue;
				}

				// Get the number of items in the queue.
				var waitingTasks = this.GetTaskCountInQueue(queue, MFTaskState.MFTaskStateWaiting);
				var showDegraded = waitingTasks > DegradedDashboardThreshold;

				// Get each task processor.
				foreach (var processor in taskQueueResolver.GetTaskProcessors(queue))
				{
					// Sanity.
					if (null == processor)
						continue;

					// Get information about the processor..
					TaskProcessorAttribute taskProcessorSettings = null;
					System.Reflection.MethodInfo methodInfo = null;
					try
					{
						taskProcessorSettings = taskQueueResolver.GetTaskProcessorSettings(queue, processor.Type);
						methodInfo = taskQueueResolver.GetTaskProcessorMethodInfo(queue, processor.Type);
					}
					catch
					{
						// Throws if the task processor is not found.
						this.Logger?.Warn
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

					// Show the description?
					var htmlString = "";
					if (false == string.IsNullOrWhiteSpace(showOnDashboardAttribute?.Description))
					{
						htmlString += new DashboardCustomContent($"<p><em>{showOnDashboardAttribute?.Description.EscapeXmlForDashboard()}</em></p>").ToXmlString();
					}

					// If we are running degraded then highlight that.
					if (showDegraded)
					{
						htmlString += "<p style='background-color: red; font-weight: bold; color: white; padding: 5px 10px;'>";
						htmlString += String.Format
						(
							Resources.AsynchronousOperations.DegradedQueueDashboardNotice,
							waitingTasks,
							DegradedDashboardThreshold
						).EscapeXmlForDashboard();
						htmlString += "</p>";
					}

					// Does it have any configuration instructions?
						IRecurrenceConfiguration recurrenceConfiguration = null;
					if (this
						.VaultApplication?
						.RecurringOperationConfigurationManager?
						.TryGetValue(queue, processor.Type, out recurrenceConfiguration) ?? false)
					{
						htmlString += recurrenceConfiguration.ToDashboardDisplayString();
					}
					else
					{
						htmlString += $"<p>{Resources.AsynchronousOperations.RepeatType_RunsOnDemandOnly.EscapeXmlForDashboard()}<br /></p>";
					}

					// Get known executions (prior, running and future).
					var executions = showDegraded
						? this.GetExecutions<TaskDirective>(queue, processor.Type, MFTaskState.MFTaskStateInProgress)
						: this.GetAllExecutions<TaskDirective>(queue,processor.Type)
						.ToList();
					var isRunning = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateInProgress);
					var isScheduled = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateWaiting);

					// Create the (basic) list item.
					var listItem = new DashboardListItemWithNormalWhitespace()
					{
						Title = showOnDashboardAttribute?.Name ?? processor.Type,
						StatusSummary = new DomainStatusSummary()
						{
							Label = isRunning
							? Resources.AsynchronousOperations.Status_Running
							: isScheduled ? Resources.AsynchronousOperations.Status_Scheduled : Resources.AsynchronousOperations.Status_Stopped
						}
					};

					// Should we show the run command?
					{
						var key = $"{queue}-{processor.Type}";
						lock (this._lock)
						{
							if (this.TaskQueueRunCommands.ContainsKey(key))
							{
								var cmd = new DashboardDomainCommand
								{
									DomainCommandID = this.TaskQueueRunCommands[key].ID,
									Title = this.TaskQueueRunCommands[key].DisplayName,
									Style = DashboardCommandStyle.Link
								};
								listItem.Commands.Add(cmd);
							}
						}
					}

					// Set the list item content.
					listItem.InnerContent = new DashboardCustomContent
					(
						htmlString
						+ executions?
							.AsDashboardContent()?
							.ToXmlString()
					);

					// Add the list item.
					yield return listItem;

				}
			}

		}
	}
}
