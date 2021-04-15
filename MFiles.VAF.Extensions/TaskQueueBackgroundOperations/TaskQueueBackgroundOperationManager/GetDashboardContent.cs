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

					// Show the description?
					var htmlString = "";
					if (false == string.IsNullOrWhiteSpace(kvp.Value.Description))
					{
						htmlString += new DashboardCustomContent($"<p><em>{kvp.Value.Description}</em></p>").ToXmlString();
					}

					// Show when it should run.
					htmlString += "<p>Runs ";
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
					htmlString += "</p>";

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
							var lastActivityTime = execution.LatestActivityTimestamp.ToDateTime(DateTimeKind.Utc);

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
									var progressBar = new DashboardProgressBar()
									{
										PercentageComplete = taskInfo.PercentageComplete.Value,
										Text = taskInfo.StatusDetails
									};
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

							// Set the row title.
							var rowTitle = "";
							switch (execution.State)
							{
								case MFilesAPI.MFTaskState.MFTaskStateWaiting:
									break;
								case MFilesAPI.MFTaskState.MFTaskStateInProgress:
									rowTitle = $"Started at {activation.ToString("yyyy-MM-dd HH:mm:ss")}, server-time (taken {execution.GetElapsedTime().ToDisplayString()} so far).";
									break;
								case MFilesAPI.MFTaskState.MFTaskStateFailed:
								case MFilesAPI.MFTaskState.MFTaskStateCanceled:
									rowTitle = $"Started at {activation.ToString("yyyy-MM-dd HH:mm:ss")}, server-time (took {execution.GetElapsedTime().ToDisplayString()}).";
									row.Styles.Add("color", "red");
									break;
								case MFilesAPI.MFTaskState.MFTaskStateCompleted:
									rowTitle = $"Started at {activation.ToString("yyyy-MM-dd HH:mm:ss")}, server-time (took {execution.GetElapsedTime().ToDisplayString()}).";
									break;
								default:
									break;
							}
							row.Attributes.Add("title", rowTitle);
						}

					}

					// Set the list item content.
					listItem.InnerContent = new DashboardCustomContent
					(
						htmlString + table?.ToXmlString()
					);

					// Add the list item.
					yield return listItem;
				}

			}
		}
	}
}
