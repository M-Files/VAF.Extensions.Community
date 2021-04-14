using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
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
					if (false == kvp.Value.ShowBackgroundOperationInDashboard)
						continue;

					// Get the current/future executions.
					var executions = kvp.Value.GetAllExecutions().ToList();
					var isRunning = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateInProgress);
					var isScheduled = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateWaiting);

					// Create the (basic) list item.
					var listItem = new DashboardListItemWithNormalWhitespace()
					{
						Title = kvp.Key,
						StatusSummary = new Configuration.Domain.DomainStatusSummary()
						{
							Label = isRunning
							? "Running"
							: isScheduled ? "Scheduled" : "Stopped"
						}
					};

					// If it is already running then just show the overview.
					var runningTask = executions
						.Where(e => e.State == MFilesAPI.MFTaskState.MFTaskStateInProgress)
						.OrderByDescending(e => e.LatestActivityTimestamp.ToDateTime(DateTimeKind.Utc))
						.FirstOrDefault();
					if (isRunning)
					{
						var status = runningTask?.RetrieveTaskInfo();
						if (null != status && status.PercentageComplete.HasValue)
						{
							// If we have a percentage then render a progress bar.
							if (string.IsNullOrWhiteSpace(status.StatusDetails))
							{
								status.StatusDetails = $"Running; at {status.PercentageComplete}% complete";
							}
							listItem.InnerContent = new DashboardCustomContent($"<table title='{status.StatusDetails}' style='height: 16px; width: 100%;'><tr><td style='width: {status.PercentageComplete.Value}%; background-color: green; color: white; font-size: 12px; text-align: right; padding: 0px 3px;'>{status.PercentageComplete.Value}%</td><td style='width: {100 - status.PercentageComplete.Value}%;background-color: lightGray'>&nbsp;</td></tr></table><span style='font-size: 12px;'><em>{status.StatusDetails}</em></span>");
						}
						else if (null != status && false == string.IsNullOrWhiteSpace(status.StatusDetails))
						{
							// If we have a status then render the status.
							listItem.InnerContent = new DashboardCustomContent($"<p><em>The operation is currently running:</em> <span style='font-weight: bold'>{status.StatusDetails}</span></p>");
						}
						else
						{
							// Otherwise, just "running".
							listItem.InnerContent = new DashboardCustomContent($"<p><em>The operation is currently running.</em></p>");
						}

						yield return listItem;
						continue;
					}

					// If this background operation has a run command then render it.
					if (kvp.Value.ShowRunCommandInDashboard)
					{
						var cmd = new DashboardDomainCommand
						{
							DomainCommandID = kvp.Value.DashboardRunCommand.ID,
							Title = kvp.Value.DashboardRunCommand.DisplayName,
							Style = DashboardCommandStyle.Link
						};
						listItem.Commands.Add(cmd);
					}

					var htmlString = "Runs ";
					switch (kvp.Value.RepeatType)
					{
						case TaskQueueBackgroundOperationRepeatType.NotRepeating:
							htmlString += "on demand (does not repeat).<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Interval:
							htmlString += $"{kvp.Value.Interval.ToDisplayString()}.<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Schedule:
							htmlString += $"{kvp.Value.Schedule.ToDisplayString()}";
							break;
						default:
							htmlString = "<em>Unhandled: " + kvp.Value.RepeatType + "</em><br />";
							break;
					}

					// If we have any scheduled then render them.
					if (isScheduled)
					{
						htmlString += "The task is scheduled to run:<ul>";
						foreach (var scheduledExecution in executions.Where(e => e.State == MFilesAPI.MFTaskState.MFTaskStateWaiting))
						{
							DateTime? activationTime = scheduledExecution.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);
							htmlString += $"<li>{activationTime.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.NextRun)}</li>";
						}
						htmlString += "</ul>";
					}
					else
					{
						htmlString += "The background operation is not scheduled to run again.<br />";
					}

					// Output any previous executions
					var previousExecutions = executions.Where
					(
						e => e.State == MFilesAPI.MFTaskState.MFTaskStateCompleted
							|| e.State == MFilesAPI.MFTaskState.MFTaskStateCanceled
							|| e.State == MFilesAPI.MFTaskState.MFTaskStateFailed
					)
					.OrderByDescending(e => e.LatestActivityTimestamp.ToDateTime(DateTimeKind.Utc))
					.Take(10)
					.ToList();
					if (previousExecutions.Any())
					{
						htmlString += "The task has previously run:<ul>";
						foreach (var previousExecution in previousExecutions)
						{
							DateTime? activationTime = previousExecution.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);
							htmlString += $"<li>{activationTime.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.LastRun)}: {previousExecution.State}</li>";
						}
						htmlString += "</ul>";
					}

					// Set the list item content.
					listItem.InnerContent = new DashboardCustomContent
					(
						(listItem.InnerContent?.ToXmlString() ?? "") +
						$"<p>{htmlString}</p>"
					);

					// Add the list item.
					yield return listItem;
				}

			}
		}
	}
}
