using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFiles.VAF.Extensions.ScheduledExecution;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent
{
	public class DashboardTableAsynchronousExecutionsDashboardContentRenderer
		: IAsynchronousExecutionsDashboardContentRenderer
	{
		public int MaximumRowsToShow { get; set; } = 100;
		public virtual DashboardContentCollection GetDashboardContent
		(
			DashboardQueueAndTaskDetails details,
			IEnumerable<TaskInfo<TaskDirective>> executions
		)
		{
			// Sanity.
			if (null == executions || false == executions.Any())
				return null;

			// What is the timezone to display data in?
			TimeZoneInfo timeZone = TimeZoneInfo.Local;
			{
				if (details.RecurrenceConfiguration is Schedule schedule)
				{
					if (schedule.TriggerTimeType == TriggerTimeType.Utc)
						timeZone = TimeZoneInfo.Utc;
					if (schedule.TriggerTimeType == TriggerTimeType.Custom)
					{
						try { timeZone = TimeZoneInfo.FindSystemTimeZoneById(schedule.TriggerTimeCustomTimeZone); }
						catch { }
					}
				}
				if (details.RecurrenceConfiguration is Frequency frequency)
				{
					if (frequency.RecurrenceType == RecurrenceType.Schedule)
					{
						if (frequency.Schedule?.TriggerTimeType == TriggerTimeType.Utc)
							timeZone = TimeZoneInfo.Utc;
						if (frequency.Schedule?.TriggerTimeType == TriggerTimeType.Custom)
						{
							try { timeZone = TimeZoneInfo.FindSystemTimeZoneById(frequency.Schedule?.TriggerTimeCustomTimeZone); }
							catch { }
						}
					}
				}
			}

			// We can only show a certain number.
			var allTasks = executions.ToList();
			var totalTaskCount = allTasks.Count;
			bool isFiltered = false;
			if (totalTaskCount > MaximumRowsToShow)
				isFiltered = true;

			var list = executions
				.OrderByDescending(e => e.LatestActivity)
				.Take(MaximumRowsToShow);

			// Create the table and header row.
			DashboardTable table = new DashboardTable();
			{
				var header = table.AddRow(DashboardTableRowType.Header);
				header.AddCells
				(
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_TaskHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_ScheduledHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_StatusHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_StartedHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_DurationHeader.EscapeXmlForDashboard()),
					new DashboardCustomContent(Resources.Dashboard.AsynchronousOperations_Table_DetailsHeader.EscapeXmlForDashboard())
				);
			}

			// Remove the bottom border so we don't get it doubled on smaller tables.
			table.TableStyles.AddOrUpdate("border-bottom", "0px");

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

				// Copy data from the execution if needed.
				taskInfo.Started = taskInfo.Started ?? execution.Status?.ReservedAt;
				taskInfo.LastActivity = taskInfo.LastActivity ?? execution.Status?.EndedAt ?? execution.Status?.LastUpdatedAt;
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

				// Create the content for the scheduled column (including icon).
				var taskInfoCell = new DashboardCustomContentEx(displayName.EscapeXmlForDashboard());
				var scheduledCell = new DashboardCustomContentEx
					(
						activation.ToTimeOffset
						(
							// If we are waiting for it to start then highlight that.
							execution.State == MFTaskState.MFTaskStateWaiting
								? FormattingExtensionMethods.DateTimeRepresentationOf.NextRun
								: FormattingExtensionMethods.DateTimeRepresentationOf.LastRun,
							timeZone
						)
					);
				var startedCell = new DashboardCustomContentEx
				(
					taskInfo.Started.HasValue
					? taskInfo.Started.Value.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.LastRun, timeZone)
					: ""
				);

				// Add a row for this execution.
				var row = table.AddRow();

				// Set the row title.
				var rowTitle = "";
				switch (execution.State)
				{
					case MFTaskState.MFTaskStateWaiting:
						taskInfoCell.Icon = "Resources/Images/Waiting.png";
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_WaitingRowTitle.EscapeXmlForDashboard(activation.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
						row.Styles.AddOrUpdate("color", Resources.Dashboard.AsynchronousOperations_Table_ColorWaiting);
						break;
					case MFTaskState.MFTaskStateInProgress:
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_RunningRowTitle.EscapeXmlForDashboard();
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle = Resources.Dashboard.AsynchronousOperations_Table_RunningRowTitle_WithTimes.EscapeXmlForDashboard
								(
									taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
									taskInfo.GetElapsedTime().ToDisplayString()
								);
						taskInfoCell.Icon = "Resources/Images/Running.png";
						row.Styles.AddOrUpdate("color", Resources.Dashboard.AsynchronousOperations_Table_ColorRunning);
						break;
					case MFTaskState.MFTaskStateFailed:
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_FailedRowTitle.EscapeXmlForDashboard();
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle = Resources.Dashboard.AsynchronousOperations_Table_FailedRowTitle_WithTimes.EscapeXmlForDashboard
								(
									taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
									taskInfo.GetElapsedTime().ToDisplayString()
								);
						taskInfoCell.Icon = "Resources/Images/Failed.png";
						row.Styles.AddOrUpdate("color", Resources.Dashboard.AsynchronousOperations_Table_ColorFailed);
						break;
					case MFTaskState.MFTaskStateCompleted:
						rowTitle = Resources.Dashboard.AsynchronousOperations_Table_CompletedRowTitle.EscapeXmlForDashboard();
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle = Resources.Dashboard.AsynchronousOperations_Table_CompletedRowTitle_WithTimes.EscapeXmlForDashboard
								(
									taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
									taskInfo.GetElapsedTime().ToDisplayString()
								);
						taskInfoCell.Icon = "Resources/Images/Completed.png";
						row.Styles.AddOrUpdate("color", Resources.Dashboard.AsynchronousOperations_Table_ColorCompleted);
						break;
					case MFTaskState.MFTaskStateCanceled:
						taskInfoCell.Icon = "Resources/Images/canceled.png";
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
					new DashboardCustomContent(execution.State.ForDisplay()),
					startedCell,
					new DashboardCustomContent(execution.State == MFTaskState.MFTaskStateWaiting ? "" : taskInfo?.GetElapsedTime().ToDisplayString()),
					taskInfo?.AsDashboardContent(removeLineBreaks)
				);

				// First cells should be as small as possible.
				for (var i = 0; i < row.Cells.Count - 1; i++)
				{
					row.Cells[i].Styles.AddOrUpdate("width", "1%");
					row.Cells[i].Styles.AddOrUpdate("white-space", "nowrap");
				}

				// Last cell should have as much space as possible.
				row.Cells[row.Cells.Count - 1].Styles.AddOrUpdate("width", "100%");
			}

			// Create an overview of the statuses.
			var data = allTasks.GroupBy(e => e.State).ToDictionary(e => e.Key, e => e.Count());

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
						$"<p style='font-size: 12px'><em>{Resources.Dashboard.AsynchronousOperations_Table_FilteredListComment.EscapeXmlForDashboard(MaximumRowsToShow, totalTaskCount)}</em></p>"
					);

				// The second cell contains the totals.
				var cell2 = row.AddCell(new DashboardCustomContentEx
				(
					"<span style=\"display: inline-block; margin: 0px 7px;\">Totals:</span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateWaiting, Resources.Dashboard.AsynchronousOperations_Table_Footer_AwaitingProcessing)}' style=\"border-bottom: 1px dashed #CCC; padding: 3px; height: 1.2em; display: inline-block; margin: 0px 4px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Waiting.png")}); background-repeat: no-repeat; background-position: 1px 4px; background-size: 14px; padding-left: 21px\">{(data.ContainsKey(MFTaskState.MFTaskStateWaiting) ? data[MFTaskState.MFTaskStateWaiting] : 0)}</span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateInProgress, Resources.Dashboard.AsynchronousOperations_Table_Footer_Running)}' style=\"border-bottom: 1px dashed #CCC; padding: 3px; height: 1.2em; display: inline-block; margin: 0px 4px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Running.png")}); background-repeat: no-repeat; background-position: 3px 4px; background-size: 14px; padding-left: 23px\">{(data.ContainsKey(MFTaskState.MFTaskStateInProgress) ? data[MFTaskState.MFTaskStateInProgress] : 0)}</span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateCompleted, Resources.Dashboard.AsynchronousOperations_Table_Footer_Completed)}' style=\"border-bottom: 1px dashed #CCC; padding: 3px; height: 1.2em; display: inline-block; margin: 0px 4px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Completed.png")}); background-repeat: no-repeat; background-position: 1px 4px; background-size: 14px; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateCompleted) ? data[MFTaskState.MFTaskStateCompleted] : 0)}</span>"
					+ $"<span title='{GetTotalTasksInStateForDisplay(data, MFTaskState.MFTaskStateFailed, Resources.Dashboard.AsynchronousOperations_Table_Footer_Failed)}' style=\"border-bottom: 1px dashed #CCC; padding: 3px; height: 1.2em; display: inline-block; margin: 0px 4px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Images/Failed.png")}); background-repeat: no-repeat; background-position: 0px 3px; background-size: 14px; padding-left: 15px; color: {Resources.Dashboard.AsynchronousOperations_Table_ColorFailed}\">{(data.ContainsKey(MFTaskState.MFTaskStateFailed) ? data[MFTaskState.MFTaskStateFailed] : 0)}</span>"
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

		IDashboardContent IAsynchronousExecutionsDashboardContentRenderer.GetDashboardContent
		(
			DashboardQueueAndTaskDetails details,
			IEnumerable<TaskInfo<TaskDirective>> executions
		)
			=> GetDashboardContent(details, executions);

	}
}
