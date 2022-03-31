using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public static class IEnumerableApplicationTaskInfoExtensionMethods
	{
		/// <summary>
		/// Creates a <see cref="DashboardTable"/> containing information about the executions
		/// detailed in <paramref name="applicationTasks"/>.
		/// </summary>
		/// <param name="applicationTasks">The previous executions.</param>
		/// <returns>The table.</returns>
		public static IDashboardContent AsDashboardContent<TDirective>
		(
			this IEnumerable<TaskInfo<TDirective>> applicationTasks,
			int maximumRowsToShow = 100
		)
			where TDirective : TaskDirective
		{
			// Sanity.
			if (null == applicationTasks || false == applicationTasks.Any())
				return null;

			// We can only show a certain number.
			var totalTaskCount = applicationTasks.Count();
			bool isFiltered = false;
			if (totalTaskCount > maximumRowsToShow)
				isFiltered = true;

			var list = applicationTasks
				.OrderByDescending(e => e.LatestActivity)
				.Take(maximumRowsToShow);

			// Create the table and header row.
			DashboardTable table = new DashboardTable();
			{
				var header = table.AddRow(DashboardTableRowType.Header);
				header.AddCells
				(
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_TaskHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_ScheduledHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_DurationHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_DetailsHeader.EscapeXmlForDashboard())
				);
			}

			// Add a row for each execution to show.
			foreach (var execution in list)
			{
				TaskDirective internalDirective = execution.Directive;
				{
					if (internalDirective is BackgroundOperationTaskDirective bgtd)
					{
						internalDirective = bgtd.GetParsedInternalDirective();
					}
				}
				var directive = internalDirective as ITaskDirectiveWithDisplayName;
				var displayName = string.IsNullOrWhiteSpace(directive?.DisplayName)
					? execution.TaskId
					: directive.DisplayName;
				var activation = execution.ActivationTime;
				TaskInformation taskInfo = null == execution.Status?.Data
					? new TaskInformation()
					: new TaskInformation(execution.Status.Data);

				// Create the content for the scheduled column (including icon).
				var taskInfoCell = new DashboardCustomContentEx(displayName.EscapeXmlForDashboard());
				var scheduledCell = new DashboardCustomContentEx
					(
						activation.ToTimeOffset
						(
							// If we are waiting for it to start then highlight that.
							execution.State == MFilesAPI.MFTaskState.MFTaskStateWaiting
								? FormattingExtensionMethods.DateTimeRepresentationOf.NextRun
								: FormattingExtensionMethods.DateTimeRepresentationOf.LastRun
						)
					);

				// Copy data from the execution if needed.
				taskInfo.Started = taskInfo.Started ?? execution.Status?.ReservedAt ?? execution.Status?.LastRetryStarted ?? taskInfo.LastActivity;
				taskInfo.LastActivity = taskInfo.LastActivity ?? execution.Status?.EndedAt ?? execution.Status?.LastUpdatedAt ?? DateTime.UtcNow;
				taskInfo.StatusDetails = taskInfo.StatusDetails ?? execution.Status?.Details;
				taskInfo.PercentageComplete = taskInfo.PercentageComplete ?? execution.Status?.PercentComplete;
				if (taskInfo.CurrentTaskState != execution.State)
					taskInfo.CurrentTaskState = execution.State;
				var removeLineBreaks = false; // By default show the full text as sent.
				if (taskInfo.CurrentTaskState == MFTaskState.MFTaskStateFailed)
				{
					taskInfo.StatusDetails = execution.Status?.ErrorMessage ?? taskInfo.StatusDetails;
					removeLineBreaks = true; // Exceptions are LONG, so format them.
				}

				// Add a row for this execution.
				var row = table.AddRow();

				// Set the row title.
				var rowTitle = "";
				switch (execution.State)
				{
					case MFilesAPI.MFTaskState.MFTaskStateWaiting:
						taskInfoCell.Icon = "Resources/Images/Waiting.png";
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_WaitingRowTitle.EscapeXmlForDashboard(activation.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
						break;
					case MFilesAPI.MFTaskState.MFTaskStateInProgress:
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_RunningRowTitle.EscapeXmlForDashboard();
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle = Resources.Dashboard.AsynchronousOperations_Table_RunningRowTitle_WithTimes.EscapeXmlForDashboard
								(
									taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
									taskInfo.GetElapsedTime().ToDisplayString()
								);
						taskInfoCell.Icon = "Resources/Images/Running.png";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateFailed:
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_FailedRowTitle.EscapeXmlForDashboard();
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle = Resources.Dashboard.AsynchronousOperations_Table_FailedRowTitle_WithTimes.EscapeXmlForDashboard
								(
									taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
									taskInfo.GetElapsedTime().ToDisplayString()
								);
						taskInfoCell.Icon = "Resources/Images/Failed.png";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateCompleted:
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_CompletedRowTitle.EscapeXmlForDashboard();
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle = Resources.Dashboard.AsynchronousOperations_Table_CompletedRowTitle_WithTimes.EscapeXmlForDashboard
								(
									taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
									taskInfo.GetElapsedTime().ToDisplayString()
								);
						taskInfoCell.Icon = "Resources/Images/Completed.png";
						break;
					default:
						break;
				}
				row.Attributes.Add("title", rowTitle);

				// Add the cells to the row.
				row.AddCells
				(
					taskInfoCell,
					scheduledCell,
					new DashboardCustomContent(execution.State == MFilesAPI.MFTaskState.MFTaskStateWaiting ? "" : taskInfo?.GetElapsedTime().ToDisplayString()),
					taskInfo?.AsDashboardContent(removeLineBreaks)
				);

				// First three cells should be as small as possible.
				row.Cells[0].Styles.AddOrUpdate("width", "1%");
				row.Cells[0].Styles.AddOrUpdate("white-space", "nowrap");
				row.Cells[1].Styles.AddOrUpdate("width", "1%");
				row.Cells[1].Styles.AddOrUpdate("white-space", "nowrap");
				row.Cells[2].Styles.AddOrUpdate("width", "1%");
				row.Cells[2].Styles.AddOrUpdate("white-space", "nowrap");

				// Last cell should have as much space as possible.
				row.Cells[3].Styles.AddOrUpdate("width", "100%");
			}

			// Create an overview of the statuses.
			var data = list.GroupBy(e => e.State).ToDictionary(e => e.Key, e => e.Count());
			
			var overview = new DashboardTable();
			{
				// Remove all styles - we only are using this for layout.
				overview.Styles.Clear();
				overview.Styles.AddOrUpdate("width", "100%");

				// Add a single row.
				var row = overview.AddRow();

				// The first cell contains text if this table is filtered, or is empty otherwise.
				var cell1 = row.AddCell();
				if (isFiltered)
					cell1.InnerContent = new DashboardCustomContentEx
					(
						$"<p style='font-size: 12px'><em>{Resources.Dashboard.AsynchronousOperations_Table_FilteredListComment.EscapeXmlForDashboard(maximumRowsToShow, totalTaskCount)}</em></p>"
					);

				// The second cell contains the totals.
				var cell2 = row.AddCell(new DashboardCustomContentEx
				(
					"<span>Totals: </span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateWaiting, Resources.Dashboard.AsynchronousOperations_Table_Footer_AwaitingProcessing)}' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Waiting.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateWaiting) ? data[MFTaskState.MFTaskStateWaiting] : 0)}</span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateInProgress, Resources.Dashboard.AsynchronousOperations_Table_Footer_Running)}' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Running.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateInProgress) ? data[MFTaskState.MFTaskStateInProgress] : 0)}</span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateCompleted, Resources.Dashboard.AsynchronousOperations_Table_Footer_Completed)}' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Completed.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateCompleted) ? data[MFTaskState.MFTaskStateCompleted] : 0)}</span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateFailed, Resources.Dashboard.AsynchronousOperations_Table_Footer_Failed)}' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Failed.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateFailed) ? data[MFTaskState.MFTaskStateFailed] : 0)}</span>"
				));
				cell2.Styles.AddOrUpdate("text-align", "right");
			}

			// Return the content.
			return new DashboardContentCollection()
			{
				table,
				overview
			};
		}
		private static string GetTotalTasksInStateForDisplay(Dictionary<MFTaskState, int> data, MFTaskState state, string resourceString, int defaultValue = default)
		{
			return resourceString?
				.EscapeXmlForDashboard(data.ContainsKey(state) ? data[state] : defaultValue)?
				.Replace("'", "&#39;");
		}
	}
}
