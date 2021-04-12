using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager
	{
		/// <summary>
		/// Returns some dashboard content that shows the background operations and their current status.
		/// </summary>
		/// <returns>The dashboard content.</returns>
		public virtual IEnumerable<DashboardListItem> GetDashboardContent()
		{
			if (this.BackgroundOperations.Count > 0)
			{
				// Output each background operation as a list item.
				foreach (var kvp in this.BackgroundOperations)
				{
					// If we should not show it then skip.
					if (false == (kvp.Value.BackgroundOperation?.ShowBackgroundOperationInDashboard ?? false))
						continue;

					// Create the (basic) list item.
					var listItem = new DashboardListItem()
					{
						Title = kvp.Key,
						StatusSummary = new Configuration.Domain.DomainStatusSummary()
						{
							Label = kvp.Value.Status.ToString()
						}
					};

					// If this background operation has a run command then render it.
					if (kvp.Value.BackgroundOperation.ShowRunCommandInDashboard)
					{
						var cmd = new DashboardDomainCommand
						{
							DomainCommandID = kvp.Value.BackgroundOperation.DashboardRunCommand.ID,
							Title = kvp.Value.BackgroundOperation.DashboardRunCommand.DisplayName,
							Style = DashboardCommandStyle.Link
						};
						listItem.Commands.Add(cmd);
					}

					var htmlString = "Runs ";
					switch (kvp.Value.BackgroundOperation.RepeatType)
					{
						case TaskQueueBackgroundOperationRepeatType.NotRepeating:
							htmlString += "on demand (does not repeat).<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Interval:
							htmlString += $"{kvp.Value.BackgroundOperation.Interval.ToDisplayString()}.<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Schedule:
							htmlString += $"{kvp.Value.BackgroundOperation.Schedule.ToDisplayString()}";
							break;
						default:
							htmlString = "<em>Unhandled: " + kvp.Value.BackgroundOperation.RepeatType + "</em><br />";
							break;
					}

					// Customise the description based on the last/next run values.
					if (kvp.Value.Status == TaskQueueBackgroundOperationStatus.Running)
					{
						htmlString += "<em>Currently running.</em>";
					}
					else
					{
						htmlString += !kvp.Value.NextRun.HasValue
							? "The background operation is not scheduled to run again.<br />"
							: $"Next run is {kvp.Value.NextRun.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.NextRun)}.<br />";
						if (kvp.Value.LastRun.HasValue)
						{
							htmlString += $"<em>Last run {kvp.Value.LastRun.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.LastRun)}.</em>";
						}
					}
					listItem.InnerContent = new DashboardCustomContent(htmlString);

					// Add the list item.
					yield return listItem;
				}

			}
		}
	}
}
