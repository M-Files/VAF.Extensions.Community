using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousContent
{
	/// <summary>
	/// Renders details about executions of an asynchronous operation into a dashboard.
	/// Note: rendering of overall process is done via <see cref="IAsynchronousDashboardContentRenderer"/>, 
	/// and the two work in tandem.
	/// </summary>
	public interface IAsynchronousExecutionsDashboardContentRenderer
	{
		IDashboardContent GetDashboardContent
		(
			DashboardQueueAndTaskDetails details,
			IEnumerable<TaskInfo<TaskDirective>> executions
		);
	}
}
