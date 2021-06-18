using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
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
		public static IEnumerable<DashboardListItem> GetDashboardContent(this TaskQueueResolver taskQueueResolver)
		{
			if (null == taskQueueResolver)
				yield break;
			//return this.BackgroundOperations.Values.OrderBy(o => o.DashboardSortOrder).AsDashboardListItems();
			foreach (var queue in taskQueueResolver.GetQueues())
			{
				// Get information about the queues.
				var queueSettings = taskQueueResolver.GetQueueSettings(queue);
				var fieldInfo = taskQueueResolver.GetQueueFieldInfo(queue);

				// If it's marked as hidden then skip.
				{
					var attributes = fieldInfo.GetCustomAttributes(typeof(HideOnDashboardAttribute), true)
						?? new HideOnDashboardAttribute[0];
					if (attributes.Length == 0)
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
						if (attributes.Length == 0)
							continue;
					}

					// This should be shown.  Do we have any extended details?
					var showOnDashboardAttribute = methodInfo.GetCustomAttributes(typeof(ShowOnDashboardAttribute), true)?
						.FirstOrDefault() as ShowOnDashboardAttribute;

					// TODO: Replicate IEnumerableTaskQueueBackgroundOperationExtensionMethods.AsDashboardListItems

					// Create the (basic) list item.
					var listItem = new DashboardListItemWithNormalWhitespace()
					{
						Title = showOnDashboardAttribute?.Name ?? processor.Type,
						StatusSummary = new Configuration.Domain.DomainStatusSummary()
						{
							Label = false
							? "Running"
							: false ? "Scheduled" : "Stopped"
						}
					};

					// Add the list item.
					yield return listItem;

				}
			}
			
		}
	}
}
