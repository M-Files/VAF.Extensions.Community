using MFiles.VAF.AppTasks;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using static MFiles.VAF.Common.ApplicationTaskQueue.TaskQueueManager;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousContent
{
	/// <summary>
	/// Renders details about asynchronous operations into a dashboard.
	/// Note: rendering of the actual executions is done via <see cref="IAsynchronousExecutionsDashboardContentRenderer"/>, 
	/// and the two work in tandem.
	/// </summary>
	public interface IAsynchronousDashboardContentRenderer
	{
		/// <summary>
		/// Gets the dashboard content for the provided <paramref name="providers"/>.
		/// </summary>
		/// <param name="providers">The providers to render content from.</param>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent(IEnumerable<IAsynchronousDashboardContentProvider> providers);

		/// <summary>
		/// Gets the dashboard content for the provided <paramref name="data"/>.  This data may come from multiple providers.
		/// </summary>
		/// <param name="data">The data to render.</param>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent(IEnumerable<KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>> data);

		/// <summary>
		/// Gets the content for a single task queue / task type.
		/// </summary>
		/// <param name="item">The item to render.</param>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent(KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>> item);
	}
}
