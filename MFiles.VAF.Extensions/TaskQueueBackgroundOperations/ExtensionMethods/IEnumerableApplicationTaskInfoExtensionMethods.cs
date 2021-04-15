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
		public static IDashboardContent AsDashboardContent(this IEnumerable<ApplicationTaskInfo> applicationTasks)
		{
			// Sanity.
			if (null == applicationTasks || false == applicationTasks.Any())
				return null;


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

			// Add a row for each execution.
			foreach (var execution in applicationTasks)
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

			// Return the table.
			return table;
		}
	}
}
