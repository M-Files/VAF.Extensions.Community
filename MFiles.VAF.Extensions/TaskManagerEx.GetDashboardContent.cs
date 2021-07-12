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
	public partial class TaskManagerEx
		: TaskManager
	{
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
				// Get information about the queues.
				var queueSettings = taskQueueResolver.GetQueueSettings(queue);
				var fieldInfo = taskQueueResolver.GetQueueFieldInfo(queue);

				// If it's marked as hidden then skip.
				{
					var attributes = fieldInfo.GetCustomAttributes(typeof(HideOnDashboardAttribute), true)
						?? new HideOnDashboardAttribute[0];
					if (attributes.Length != 0)
						continue;
				}

				// Get each task processor.
				foreach (var processor in taskQueueResolver.GetTaskProcessors(queue))
				{
					// Get information about the processor.
					var taskProcessorSettings = taskQueueResolver.GetTaskProcessorSettings(queue, processor.Type);
					var methodInfo = taskQueueResolver.GetTaskProcessorMethodInfo(queue, processor.Type);

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
						htmlString += new DashboardCustomContent($"<p><em>{System.Security.SecurityElement.Escape(showOnDashboardAttribute?.Description)}</em></p>").ToXmlString();
					}

					// TODO: Replicate IEnumerableTaskQueueBackgroundOperationExtensionMethods.AsDashboardListItems
					htmlString += "<p>Runs on demand (does not repeat).<br /></p>";

					// Get known executions (prior, running and future).
					var executions = this
						.GetAllExecutions<TaskDirective>(queue, processor.Type)
						.ToList();
					var isRunning = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateInProgress);
					var isScheduled = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateWaiting);

					// Create the (basic) list item.
					var listItem = new DashboardListItemWithNormalWhitespace()
					{
						Title = showOnDashboardAttribute?.Name ?? processor.Type,
						StatusSummary = new Configuration.Domain.DomainStatusSummary()
						{
							Label = isRunning
							? "Running"
							: false ? "Scheduled" : "Stopped"
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
							.AsDashboardContent(this.ServerId)?
							.ToXmlString()
					);

					// Add the list item.
					yield return listItem;

				}
			}

		}
	}
}
