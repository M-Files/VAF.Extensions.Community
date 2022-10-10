using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Dashboards
{
	public class DashboardQueueAndTaskDetails
	{
		/// <summary>
		/// The number of waiting tasks in a single queue at which point the dashboard is shown degraded.
		/// </summary>
		public const int DegradedDashboardThreshold = 3000;

		public string QueueId { get; set; }
		public string TaskType { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool ShowDegradedDashboard => (this.TasksInQueue >= DegradedDashboardThreshold);
		public int TasksInQueue { get; set; }
		public List<DashboardCommand> Commands { get; set; } = new List<DashboardCommand>();
		public IRecurrenceConfiguration RecurrenceConfiguration { get; set; }
	}
}
