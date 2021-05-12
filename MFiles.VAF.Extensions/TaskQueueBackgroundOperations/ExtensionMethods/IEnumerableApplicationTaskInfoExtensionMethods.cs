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
		public static IDashboardContent AsDashboardContent
		(
			this IEnumerable<ApplicationTaskInfo> applicationTasks,
			int maximumRowsToShow = 40
		)
		{
			// Sanity.
			if (null == applicationTasks || false == applicationTasks.Any())
				return null;
			var list = applicationTasks
				.OrderByDescending(e => e.LatestActivityTimestamp.ToDateTime(DateTimeKind.Utc))
				.ToList();

			// Create the table and header row.
			DashboardTable table = new DashboardTable();
			{
				var header = table.AddRow(DashboardTableRowType.Header);
				header.AddCells
				(
					new DashboardCustomContent("Task"),
					new DashboardCustomContent("Scheduled"),
					new DashboardCustomContent("Duration"),
					new DashboardCustomContent("Details")
				);
			}

			List<ApplicationTaskInfo> executionsToShow;
			bool isFiltered = false;
			if (list.Count <= maximumRowsToShow)
				executionsToShow = list;
			else
			{
				isFiltered = true;

				// Show the latest 20 errors, then the rest filled with non-errors.
				executionsToShow = new List<ApplicationTaskInfo>
				(
					list
						.Where(e => e.State == MFTaskState.MFTaskStateFailed)
						.Take(20)
						.Union(list.Where(e => e.State != MFTaskState.MFTaskStateFailed).Take(maximumRowsToShow - 20))
				);
			}

			// Add a row for each execution to show.
			foreach (var execution in executionsToShow)
			{
				var taskInfo = execution.RetrieveTaskInfo();
				var activation = execution.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);

				// Try and get the display name from the directive.
				var displayName = execution.TaskID; // Default to the task ID.
				if (null != execution.TaskData && execution.TaskData.Length > 0)
				{
					try
					{
						var directive = TaskQueueDirective.Parse<BackgroundOperationTaskQueueDirective>(execution.TaskData)
							?.GetParsedInternalDirective() as ITaskQueueDirectiveWithDisplayName;
						displayName = string.IsNullOrWhiteSpace(directive?.DisplayName)
							? displayName
							: directive.DisplayName;
					}
					catch { }
				}

				// Create the content for the scheduled column (including icon).
				var taskInfoCell = new DashboardCustomContentEx(System.Security.SecurityElement.Escape(displayName));
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

				// Add a row for this execution.
				var row = table.AddRow();

				// Set the row title.
				var rowTitle = "";
				switch (execution.State)
				{
					case MFilesAPI.MFTaskState.MFTaskStateWaiting:
						taskInfoCell.Icon = "Resources/Waiting.png";
						rowTitle = $"Waiting.  Will start at approximately {activation.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}.";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateInProgress:
						rowTitle = "Running.";
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle += $" Started at approximately {taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} server-time (taken {taskInfo.GetElapsedTime().ToDisplayString()} so far).";
						taskInfoCell.Icon = "Resources/Running.png";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateFailed:
						rowTitle = "Failed.";
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle += $" Started at approximately {taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} server-time (took {taskInfo.GetElapsedTime().ToDisplayString()}).";
						taskInfoCell.Icon = "Resources/Failed.png";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateCompleted:
						rowTitle = "Completed.";
						if ((taskInfo?.Started.HasValue) ?? false)
							rowTitle += $" Started at approximately {taskInfo.Started.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} server-time (took {taskInfo.GetElapsedTime().ToDisplayString()}).";
						taskInfoCell.Icon = "Resources/Completed.png";
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
					new DashboardCustomContent(taskInfo?.GetElapsedTime().ToDisplayString()),
					taskInfo?.AsDashboardContent()
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
					cell1.InnerContent = new DashboardCustomContentEx($"<p style='font-size: 12px'><em>This table shows only {maximumRowsToShow} of {list.Count} tasks.</em></p>");

				// The second cell contains the totals.
				var cell2 = row.AddCell(new DashboardCustomContentEx
				(
					"<span>Totals: </span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateWaiting) ? data[MFTaskState.MFTaskStateWaiting] : 0)} awaiting processing' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Waiting.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateWaiting) ? data[MFTaskState.MFTaskStateWaiting] : 0)}</span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateInProgress) ? data[MFTaskState.MFTaskStateInProgress] : 0)} running' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Running.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateInProgress) ? data[MFTaskState.MFTaskStateInProgress] : 0)}</span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateCompleted) ? data[MFTaskState.MFTaskStateCompleted] : 0)} completed' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Completed.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateCompleted) ? data[MFTaskState.MFTaskStateCompleted] : 0)}</span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateFailed) ? data[MFTaskState.MFTaskStateFailed] : 0)} failed' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Failed.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateFailed) ? data[MFTaskState.MFTaskStateFailed] : 0)}</span>"
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
	}
}
