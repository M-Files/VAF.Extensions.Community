using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent
{
	public class DashboardQueueAndTaskDetails
	{
		/// <summary>
		/// The queue ID being represented.
		/// </summary>
		public string QueueId { get; set; }

		/// <summary>
		/// The task type being represented.
		/// </summary>
		public string TaskType { get; set; }

		/// <summary>
		/// The (display) name for this queue/task-type to show on the dashboard.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The description to show on the dashboard.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Whether this section should be shown degraded or not.
		/// </summary>
		public bool ShowDegradedDashboard => TasksInQueue >= AsynchronousDashboardContentSettings.DegradedDashboardThreshold;

		/// <summary>
		/// The number of tasks of this type in the queue.
		/// </summary>
		public int TasksInQueue { get; set; }

		/// <summary>
		/// Any commands to render related to this queue/task-type.
		/// </summary>
		public List<DashboardCommand> Commands { get; set; } = new List<DashboardCommand>();

		/// <summary>
		/// If this is a recurring process then details on the recurring frequency.
		/// </summary>
		public IRecurrenceConfiguration RecurrenceConfiguration { get; set; }

		/// <summary>
		/// Determines the order the operations are shown on the dashboard.
		/// </summary>
		public int Order { get; set; }
	}
}
