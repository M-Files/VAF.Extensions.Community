namespace MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent
{
	public static class AsynchronousDashboardContentSettings
	{
		/// <summary>
		/// The number of waiting tasks in a single queue at which point the dashboard is shown degraded.
		/// </summary>
		public const int DegradedDashboardThreshold = 3000;
	}
}
