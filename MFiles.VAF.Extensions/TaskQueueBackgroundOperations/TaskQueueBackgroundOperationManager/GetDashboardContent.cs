using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
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

					// Define when it should run.
					var htmlString = "Runs ";
					switch (kvp.Value.RepeatType)
					{
						case TaskQueueBackgroundOperationRepeatType.NotRepeating:
							htmlString += "on demand (does not repeat).<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Interval:
							htmlString += $"{kvp.Value.Interval.ToIntervalDisplayString()}.<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Schedule:
							htmlString += $"{kvp.Value.Schedule.ToDisplayString()}";
							break;
						default:
							htmlString = "<em>Unhandled: " + kvp.Value.RepeatType + "</em><br />";
							break;
					}

					// Get known executions (prior, running and future).
					var executions = kvp.Value
						.GetAllExecutions()
						.OrderByDescending(e => e.ActivationTimestamp.ToDateTime(DateTimeKind.Utc))
						.Take(20)
						.ToList();
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

					// If we know about any executions then output them.
					DashboardTable table = null;
					if (executions.Count > 0)
					{

						// Create the table and header row.
						table = new DashboardTable();
						{
							var header = table.AddRow(DashboardTableRowType.Header);
							header.AddCells
							(
								new DashboardCustomContent("Scheduled for"),
								new DashboardCustomContent("Status"),
								new DashboardCustomContent("Duration"),
								new DashboardCustomContent("Details")
							);
						}

						// Add a row for each execution.
						foreach (var execution in executions)
						{
							var taskInfo = execution.RetrieveTaskInfo();
							var activation = execution.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);

							string state = "";
							switch (execution.State)
							{
								case MFilesAPI.MFTaskState.MFTaskStateWaiting:
									state = "Waiting";
									break;
								case MFilesAPI.MFTaskState.MFTaskStateInProgress:
									state = "Running";
									break;
								default:
									state = execution.State.ToString().Substring(11);
									break;
							}

							// Render out the status details.
							IDashboardContent statusDetails = new DashboardCustomContent("");
							if (null != taskInfo)
							{
								// If we have a progress then do a pretty bar chart.
								if (null != taskInfo.PercentageComplete)
								{
									var progressBar = new DashboardTable();
									progressBar.Attributes.Add("title", taskInfo.StatusDetails);
									var progressRow = progressBar.AddRow();
									var completeCell = progressRow.AddCell
									(
										$"{taskInfo.PercentageComplete.Value}%"
									);
									completeCell.Styles.Add("width", $"{taskInfo.PercentageComplete.Value}%");
									completeCell.Styles.Add("background-color", "green");
									completeCell.Styles.Add("color", "white");
									completeCell.Styles.Add("text-align", "right");
									var leftCell = progressRow.AddCell("&nbsp;");
									leftCell.Styles.Add("width", $"{(100 - taskInfo.PercentageComplete.Value)}%");
									statusDetails = progressBar;
								}
								else if (false == string.IsNullOrWhiteSpace(taskInfo.StatusDetails))
								{
									// Otherwise just show the text.
									statusDetails = new DashboardCustomContent(taskInfo?.StatusDetails);
								}
							}

							// Add a row for this execution.
							var row = table.AddRow();
							row.AddCells
							(
								new DashboardCustomContent
								(
									activation.ToTimeOffset
									(
										// If we are waiting for it to start then highlight that.
										execution.State == MFilesAPI.MFTaskState.MFTaskStateWaiting
											? FormattingExtensionMethods.DateTimeRepresentationOf.NextRun
											: FormattingExtensionMethods.DateTimeRepresentationOf.LastRun
									)
								),
								new DashboardCustomContent(state),
								new DashboardCustomContent(execution.GetElapsedTime().ToDisplayString()),
								statusDetails
							);

							// Set the cell sizing.
							for (var i = 0; i < 3; i++)
							{
								row.Cells[i].Styles.Add("white-space", "nowrap");
							}
							row.Cells[3].Styles.Add("min-width", "150px");
						}

					}

					// Set the list item content.
					listItem.InnerContent = new DashboardCustomContent
					(
						$"<p>{htmlString}</p>" +
							table?.ToXmlString()
					);

					//// If it is already running then just show the overview.
					//var runningTask = executions
					//	.Where(e => e.State == MFilesAPI.MFTaskState.MFTaskStateInProgress)
					//	.OrderByDescending(e => e.LatestActivityTimestamp.ToDateTime(DateTimeKind.Utc))
					//	.FirstOrDefault();
					//if (isRunning)
					//{
					//	var status = runningTask?.RetrieveTaskInfo();
					//	if (null != status && status.PercentageComplete.HasValue)
					//	{
					//		// If we have a percentage then render a progress bar.
					//		if (string.IsNullOrWhiteSpace(status.StatusDetails))
					//		{
					//			status.StatusDetails = $"Running; at {status.PercentageComplete}% complete";
					//		}
					//		listItem.InnerContent = new DashboardCustomContent($"<table title='{status.StatusDetails}' style='height: 16px; width: 100%;'><tr><td style='width: {status.PercentageComplete.Value}%; background-color: green; color: white; font-size: 12px; text-align: right; padding: 0px 3px;'>{status.PercentageComplete.Value}%</td><td style='width: {100 - status.PercentageComplete.Value}%;background-color: lightGray'>&nbsp;</td></tr></table><span style='font-size: 12px;'><em>{status.StatusDetails}</em></span>");
					//	}
					//	else if (null != status && false == string.IsNullOrWhiteSpace(status.StatusDetails))
					//	{
					//		// If we have a status then render the status.
					//		listItem.InnerContent = new DashboardCustomContent($"<p><em>The operation is currently running:</em> <span style='font-weight: bold'>{status.StatusDetails}</span></p>");
					//	}
					//	else
					//	{
					//		// Otherwise, just "running".
					//		listItem.InnerContent = new DashboardCustomContent($"<p><em>The operation is currently running.</em></p>");
					//	}

					//	yield return listItem;
					//	continue;
					//}

					//// If we have any scheduled then render them.
					//if (isScheduled)
					//{
					//	htmlString += "The task is scheduled to run:<ul>";
					//	foreach (var scheduledExecution in executions.Where(e => e.State == MFilesAPI.MFTaskState.MFTaskStateWaiting))
					//	{
					//		DateTime? activationTime = scheduledExecution.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);
					//		htmlString += $"<li>{activationTime.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.NextRun)}</li>";
					//	}
					//	htmlString += "</ul>";
					//}
					//else
					//{
					//	htmlString += "The background operation is not scheduled to run again.<br />";
					//}

					//// Output any previous executions
					//var previousExecutions = executions.Where
					//(
					//	e => e.State == MFilesAPI.MFTaskState.MFTaskStateCompleted
					//		|| e.State == MFilesAPI.MFTaskState.MFTaskStateCanceled
					//		|| e.State == MFilesAPI.MFTaskState.MFTaskStateFailed
					//)
					//.OrderByDescending(e => e.LatestActivityTimestamp.ToDateTime(DateTimeKind.Utc))
					//.Take(10)
					//.ToList();
					//if (previousExecutions.Any())
					//{
					//	htmlString += "The task has previously run:<ul>";
					//	foreach (var previousExecution in previousExecutions)
					//	{
					//		DateTime? activationTime = previousExecution.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);
					//		htmlString += $"<li>{activationTime.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.LastRun)}: {previousExecution.State}</li>";
					//	}
					//	htmlString += "</ul>";
					//}

					//// Set the list item content.
					//listItem.InnerContent = new DashboardCustomContent
					//(
					//	(listItem.InnerContent?.ToXmlString() ?? "") +
					//	$"<p>{htmlString}</p>"
					//);

					// Add the list item.
					yield return listItem;
				}

			}
		}
	}
}
