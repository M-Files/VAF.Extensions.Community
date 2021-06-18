using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards
{
	/// <summary>
	/// Used to hide a queue or task processor on the dashboard.
	/// </summary>
	/// <remarks>
	/// If both <see cref="ShowOnDashboardAttribute"/> and <see cref="HideOnDashboardAttribute"/> attributes
	/// are present then the <see cref="HideOnDashboardAttribute"/> takes effect.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class HideOnDashboardAttribute
		: Attribute
	{
	}

	/// <summary>
	/// Used to describe how a task processor should be shown on the dashboard.
	/// Can be applied to any method marked with a <see cref="MFiles.VAF.AppTasks.TaskProcessorAttribute"/>.
	/// </summary>
	/// <remarks>
	/// If both <see cref="ShowOnDashboardAttribute"/> and <see cref="HideOnDashboardAttribute"/> attributes
	/// are present then the <see cref="HideOnDashboardAttribute"/> takes effect.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ShowOnDashboardAttribute
		: Attribute
	{
		public int DashboardSortOrder { get; set; } = 0;
		public string Name { get; set; } = null;
		public string Description { get; set; } = null;
		public bool ShowRunCommandInDashboard { get; set; } = false;
		public string DashboardRunCommandTitle { get; set; } = "Run";

		public ShowOnDashboardAttribute(string name)
		{
			this.Name = name;
		}
	}
}
