using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.AdminConfigurations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using static MFiles.VAF.Common.ApplicationTaskQueue.TaskQueueManager;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousContent
{
	/// <summary>
	/// Provides content around asynchronous dashboards, to be subsequently rendered on a dashboard.
	/// </summary>
	public interface IAsynchronousDashboardContentProvider
	{
		/// <summary>
		/// Returns data from this provider about asynchronous operations that should be rendered onto a dashboard.
		/// </summary>
		/// <returns>The data, or null if none should be rendered.</returns>
		IEnumerable<KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>> GetAsynchronousDashboardContent();
	}
}
