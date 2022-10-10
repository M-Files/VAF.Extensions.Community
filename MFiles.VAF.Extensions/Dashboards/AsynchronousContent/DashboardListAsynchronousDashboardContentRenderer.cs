using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousContent
{
	/// <summary>
	/// An implementation of <see cref="IAsynchronousDashboardContentRenderer"/> that returns data as a dashboard list.
	/// </summary>
	public class DashboardListAsynchronousDashboardContentRenderer
		: IAsynchronousDashboardContentRenderer
	{
		protected IAsynchronousExecutionsDashboardContentRenderer ExecutionsDashboardContentRenderer { get; private set; }

		public DashboardListAsynchronousDashboardContentRenderer()
		{
			// Default to using the table renderer.
			ExecutionsDashboardContentRenderer = new DashboardTableAsynchronousExecutionsDashboardContentRenderer();
		}
		public DashboardListAsynchronousDashboardContentRenderer(IAsynchronousExecutionsDashboardContentRenderer executionsDashboardContentRenderer)
		{
			ExecutionsDashboardContentRenderer = executionsDashboardContentRenderer
				?? throw new ArgumentNullException(nameof(executionsDashboardContentRenderer));
		}
		/// <summary>
		/// Renders <paramref name="data"/> as a <see cref="DashboardList"/> within a <see cref="DashboardPanelEx"/>.
		/// </summary>
		/// <param name="data">The items to render.</param>
		/// <returns>The rendered panel.</returns>
		public virtual DashboardPanelEx GetDashboardContent(IEnumerable<KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>> data)
		{
			// Add all items to the list.
			var list = new DashboardList();
			if (null != data)
				list.Items.AddRange(data.Where(d => null != d.Key && null != d.Value).Select(d => GetDashboardContent(d)));

			// Did we get anything?
			if (0 == list.Items.Count)
				list.Items.Add(new DashboardListItem()
				{
					Title = Resources.Dashboard.AsynchronousOperations_ThereAreNoCurrentAsynchronousOperations,
					StatusSummary = new DomainStatusSummary()
					{
						Status = DomainStatus.Undefined
					}
				});

			// Return the panel.
			return new DashboardPanelEx()
			{
				Title = Resources.Dashboard.AsynchronousOperations_DashboardTitle,
				InnerContent = new DashboardContentCollection
				{
					new DashboardCustomContent($"<em>{Resources.Dashboard.TimeOnServer.EscapeXmlForDashboard(DateTime.Now.ToLocalTime().ToString("HH:mm:ss"))} ({(TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName)})</em>"),
					list
				}
			};
		}

		/// <summary>
		/// Returns a single <paramref name="item"/> rendered as a <see cref="DashboardListItem"/>.
		/// </summary>
		/// <param name="item">The item to render.</param>
		/// <returns>The list item, or null if the item is invalid.</returns>
		public virtual DashboardListItem GetDashboardContent(KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>> item)
		{
			// Sanity.
			if (null == item.Key)
				return null;
			if (null == item.Value)
				return null;
			var details = item.Key;
			var executions = item.Value.ToList();

			// Show the description?
			var htmlString = "";
			if (false == string.IsNullOrWhiteSpace(details.Description))
			{
				htmlString += new DashboardCustomContent($"<p><em>{details.Description.EscapeXmlForDashboard()}</em></p>").ToXmlString();
			}

			// If we are running degraded then highlight that.
			if (details.ShowDegradedDashboard)
			{
				htmlString += "<p style='background-color: red; font-weight: bold; color: white; padding: 5px 10px;'>";
				htmlString += string.Format
				(
					Resources.AsynchronousOperations.DegradedQueueDashboardNotice,
					details.TasksInQueue,
					DashboardQueueAndTaskDetails.DegradedDashboardThreshold
				).EscapeXmlForDashboard();
				htmlString += "</p>";
			}

			// Does it have any configuration instructions?
			if (null != details.RecurrenceConfiguration)
			{
				htmlString += details.RecurrenceConfiguration.ToDashboardDisplayString();
			}
			else
			{
				htmlString += $"<p>{Resources.AsynchronousOperations.RepeatType_RunsOnDemandOnly.EscapeXmlForDashboard()}<br /></p>";
			}

			// Get known executions (prior, running and future).
			var isRunning = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateInProgress);
			var isScheduled = executions.Any(e => e.State == MFilesAPI.MFTaskState.MFTaskStateWaiting);

			// Return the populated list item.
			return new DashboardListItemEx()
			{
				ID = $"{details.QueueId}-{details.TaskType}",
				Title = string.IsNullOrWhiteSpace(details.Name) ? details.TaskType : details.Name,
				StatusSummary = new DomainStatusSummary()
				{
					Label = isRunning || details.ShowDegradedDashboard
					? Resources.AsynchronousOperations.Status_Running
					: isScheduled ? Resources.AsynchronousOperations.Status_Scheduled : Resources.AsynchronousOperations.Status_Stopped
				},
				Commands = details.Commands ?? new List<DashboardCommand>(),

				// Use the executions renderer to get the inner content.
				InnerContent = new DashboardContentCollection()
				{
					new DashboardCustomContentEx(htmlString),
					ExecutionsDashboardContentRenderer?.GetDashboardContent(details, executions)
				}
			};
		}

		/// <inheritdoc />
		IDashboardContent IAsynchronousDashboardContentRenderer.GetDashboardContent(IEnumerable<IAsynchronousDashboardContentProvider> providers)
		{
			// Get the data from the providers and render it.
			return GetDashboardContent
			(
				providers?.SelectMany(provider => provider.GetAsynchronousDashboardContent())
			);
		}

		/// <inheritdoc />
		IDashboardContent IAsynchronousDashboardContentRenderer.GetDashboardContent(IEnumerable<KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>> data)
			=> GetDashboardContent(data);

		/// <inheritdoc />
		IDashboardContent IAsynchronousDashboardContentRenderer.GetDashboardContent(KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>> item)
			=> GetDashboardContent(item);
	}
}
