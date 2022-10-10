using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent
{
	public class DashboardQueueAndTaskDetails
	{

		public string QueueId { get; set; }
		public string TaskType { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool ShowDegradedDashboard => TasksInQueue >= AsynchronousDashboardContentSettings.DegradedDashboardThreshold;
		public int TasksInQueue { get; set; }
		public List<DashboardCommand> Commands { get; set; } = new List<DashboardCommand>();
		public IRecurrenceConfiguration RecurrenceConfiguration { get; set; }
	}
}
