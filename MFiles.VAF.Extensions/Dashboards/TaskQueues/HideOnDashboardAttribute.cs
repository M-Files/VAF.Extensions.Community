using System;

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
}
