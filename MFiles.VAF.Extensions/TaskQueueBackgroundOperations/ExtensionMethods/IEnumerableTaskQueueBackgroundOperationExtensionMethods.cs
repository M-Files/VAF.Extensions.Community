using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain;
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
	public static class IEnumerableTaskQueueBackgroundOperationExtensionMethods
	{
		/// <summary>
		/// Creates <see cref="DashboardListItem"/> collection containing information about the background operations
		/// detailed in <paramref name="backgroundOperations"/>.
		/// </summary>
		/// <param name="backgroundOperations">The background operations.</param>
		/// <returns>The list items.</returns>
		public static IEnumerable<DashboardListItem> AsDashboardListItems<TSecureConfiguration>
		(
			this IEnumerable<TaskQueueBackgroundOperation<TSecureConfiguration>> backgroundOperations
		)
		where TSecureConfiguration : class, new()
		{
			// Sanity.
			if (null == backgroundOperations || false == backgroundOperations.Any())
				yield break;

			// Output each background operation as a list item.
			foreach (var bgo in backgroundOperations)
			{
				// Sanity.
				if (null == bgo)
					continue;

				// If we should not show it then skip.
				if (false == bgo.ShowBackgroundOperationInDashboard)
					continue;

				// Show the description?
				var htmlString = "";
				if (false == string.IsNullOrWhiteSpace(bgo.Description))
				{
					htmlString += new DashboardCustomContent($"<p><em>{System.Security.SecurityElement.Escape(bgo.Description)}</em></p>").ToXmlString();
				}

				// Show when it should run.
				switch (bgo.RepeatType)
				{
					case TaskQueueBackgroundOperationRepeatType.NotRepeating:
						htmlString += "<p>Runs on demand (does not repeat).<br /></p>";
						break;
					case TaskQueueBackgroundOperationRepeatType.Interval:
						htmlString += bgo.Interval?.ToDashboardDisplayString();
						break;
					case TaskQueueBackgroundOperationRepeatType.Schedule:
						htmlString += bgo.Schedule?.ToDashboardDisplayString();
						break;
					default:
						htmlString = "<p><em>Unhandled repeat type: " + bgo.RepeatType + "</em><br /></p>";
						break;
				}

				// Get known executions (prior, running and future).
				var executions = bgo
					.GetAllExecutions()
					.ToList();
				var isRunning = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateInProgress);
				var isScheduled = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateWaiting);

				// Create the (basic) list item.
				var listItem = new DashboardListItemWithNormalWhitespace()
				{
					Title = bgo.Name,
					StatusSummary = new DomainStatusSummary()
					{
						Label = isRunning
						? "Running"
						: isScheduled ? "Scheduled" : "Stopped"
					}
				};

				// If this background operation has a run command then render it.
				if (bgo.ShowRunCommandInDashboard)
				{
					var cmd = new DashboardDomainCommand
					{
						DomainCommandID = bgo.DashboardRunCommand.ID,
						Title = bgo.DashboardRunCommand.DisplayName,
						Style = DashboardCommandStyle.Link
					};
					listItem.Commands.Add(cmd);
				}

				// Set the list item content.
				listItem.InnerContent = new DashboardCustomContent
				(
					htmlString
					+ executions?
						.AsDashboardContent()?
						.ToXmlString()
				);

				// Add the list item.
				yield return listItem;
			}
		}
	}
}
