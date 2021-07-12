using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public static class TaskQueueResolverExtensionMethods
	{
		/// <summary>
		/// Returns some dashboard content that shows the background operations and their current status.
		/// </summary>
		/// <returns>The dashboard content.</returns>
		public static IEnumerable<DashboardListItem> GetDashboardContent(this TaskQueueResolver taskQueueResolver, TaskManager taskManager)
		{
			if (null == taskQueueResolver)
				yield break;
			if (null == taskManager)
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

					// TODO: Replicate IEnumerableTaskQueueBackgroundOperationExtensionMethods.AsDashboardListItems
					var htmlString = "<p>Runs on demand (does not repeat).<br /></p>";

					// Get known executions (prior, running and future).
					var executions = taskManager
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

					// Set the list item content.
					listItem.InnerContent = new DashboardCustomContent
					(
						htmlString
						+ executions?
							.AsDashboardContent(taskManager.ServerId)?
							.ToXmlString()
					);

					// Add the list item.
					yield return listItem;

				}
			}
			
		}
	}
}
