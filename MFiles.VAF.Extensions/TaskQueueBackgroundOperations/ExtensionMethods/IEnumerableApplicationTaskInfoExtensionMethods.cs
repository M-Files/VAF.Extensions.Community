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
					new DashboardCustomContent("Scheduled for"),
					new DashboardCustomContent("Status"),
					new DashboardCustomContent("Duration"),
					new DashboardCustomContent("Details")
				);
			}

			// Add a row for each execution.
			foreach (var execution in applicationTasks)
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
					taskInfo?.AsDashboardContent()
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
						break;
					case MFilesAPI.MFTaskState.MFTaskStateCompleted:
						rowTitle = $"Started at {activation.ToString("yyyy-MM-dd HH:mm:ss")}, server-time (took {execution.GetElapsedTime().ToDisplayString()}).";
						break;
					default:
						break;
				}
				row.Attributes.Add("title", rowTitle);
			}

			// Return the table.
			return table;
		}
	}
}
