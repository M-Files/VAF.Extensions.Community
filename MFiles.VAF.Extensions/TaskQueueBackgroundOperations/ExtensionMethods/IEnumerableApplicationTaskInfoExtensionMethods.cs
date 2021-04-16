using MFiles.VAF.Configuration;
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
			int maximumRowsToShow
		)
		{
			// Sanity.
			if (null == applicationTasks || false == applicationTasks.Any())
				return null;
			var list = applicationTasks.ToList();

			// Create the table and header row.
			DashboardTable table = new DashboardTable();
			{
				var header = table.AddRow(DashboardTableRowType.Header);
				header.AddCells
				(
					new DashboardCustomContent("Scheduled"),
					new DashboardCustomContent("Duration"),
					new DashboardCustomContent("Details")
				);
			}

			List<ApplicationTaskInfo> executionsToShow;
			if (list.Count <= maximumRowsToShow)
				executionsToShow = list;
			else
			{
				// TODO: Better logic for which records to show.
				executionsToShow = new List<ApplicationTaskInfo>(list.Take(maximumRowsToShow));
			}

			// Add a row for each execution to show.
			foreach (var execution in executionsToShow)
			{
				var taskInfo = execution.RetrieveTaskInfo();
				var activation = execution.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);

				// Create the content for the scheduled column (including icon).
				var scheduled = new DashboardCustomContentEx
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
						scheduled.Icon = "Resources/Waiting.png";
						rowTitle = $"Waiting.  Will start at approximately {activation.ToString("yyyy-MM-dd HH:mm:ss")}.";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateInProgress:
						rowTitle = $"Running. Started at approximately {activation.ToString("yyyy-MM-dd HH:mm:ss")} server-time (taken {execution.GetElapsedTime().ToDisplayString()} so far).";
						scheduled.Icon = "Resources/Running.png";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateFailed:
						rowTitle = $"Failed. Started at approximately {activation.ToString("yyyy-MM-dd HH:mm:ss")} server-time (took {execution.GetElapsedTime().ToDisplayString()}).";
						scheduled.Icon = "Resources/Failed.png";
						break;
					case MFilesAPI.MFTaskState.MFTaskStateCompleted:
						rowTitle = $"Completed. Started at approximately {activation.ToString("yyyy-MM-dd HH:mm:ss")} server-time (took {execution.GetElapsedTime().ToDisplayString()}).";
						scheduled.Icon = "Resources/Completed.png";
						break;
					default:
						break;
				}
				row.Attributes.Add("title", rowTitle);

				// Add the cells to the row.
				row.AddCells
				(
					scheduled,
					new DashboardCustomContent(execution.GetElapsedTime().ToDisplayString()),
					taskInfo?.AsDashboardContent()
				);

				// First two cells should be as small as possible.
				row.Cells[0].Styles.Add("width", "1%");
				row.Cells[0].Styles.Add("white-space", "nowrap");
				row.Cells[1].Styles.Add("width", "1%");
				row.Cells[1].Styles.Add("white-space", "nowrap");

				// Last cell should have as much space as possible.
				row.Cells[2].Styles.Add("width", "100%");
			}

			// Create an overview of the statuses.
			var data = list.GroupBy(e => e.State).ToDictionary(e => e.Key, e => e.Count());
			var overview = new DashboardCustomContentEx
			(
				"<span style='float: right; margin: 5px 0px'>"
					+ "<span>Totals: </span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateWaiting) ? data[MFTaskState.MFTaskStateWaiting] : 0)} awaiting processing' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Waiting.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateWaiting) ? data[MFTaskState.MFTaskStateWaiting] : 0)}</span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateInProgress) ? data[MFTaskState.MFTaskStateInProgress] : 0)} running' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Running.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateInProgress) ? data[MFTaskState.MFTaskStateInProgress] : 0)}</span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateCompleted) ? data[MFTaskState.MFTaskStateCompleted] : 0)} completed' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Completed.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateCompleted) ? data[MFTaskState.MFTaskStateCompleted] : 0)}</span>"
					+ $"<span title='{(data.ContainsKey(MFTaskState.MFTaskStateFailed) ? data[MFTaskState.MFTaskStateFailed] : 0)} failed' style=\"display: inline-block; margin: 0px 2px; background-image: url({DashboardHelpersEx.ImageFileToDataUri("Resources/Failed.png")}); background-repeat: no-repeat; background-position: 0 center; padding-left: 20px\">{(data.ContainsKey(MFTaskState.MFTaskStateFailed) ? data[MFTaskState.MFTaskStateFailed] : 0)}</span>"
				+ "</span>"
			);

			// Return the table.
			return new DashboardContentCollection()
			{
				table,
				overview
			};
		}
	}
}
