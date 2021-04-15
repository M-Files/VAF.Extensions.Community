using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Details about the current job status.
	/// </summary>
	public class TaskInformation
	{
		/// <summary>
		/// Details about the current status (e.g. "Processing object 5 of 100").
		/// </summary>
		public string StatusDetails { get; set; }

		/// <summary>
		/// Details on how far through the process the task is.
		/// </summary>
		public int? PercentageComplete { get; set; }

		/// <summary>
		/// Creates either a <see cref="DashboardProgressBar"/> or <see cref="DashboardCustomContent"/>
		/// to render the current task information on a dashboard.
		/// </summary>
		/// <returns>The dashboard content.</returns>
		public IDashboardContent AsDashboardContent()
		{
			
			// If we have a progress then do a pretty bar chart.
			if (null != this.PercentageComplete)
			{
				var progressBar = new DashboardProgressBar()
				{
					PercentageComplete = this.PercentageComplete.Value,
					Text = this.StatusDetails
				};
				return progressBar;
			}
			else if (false == string.IsNullOrWhiteSpace(this.StatusDetails))
			{
				// Otherwise just show the text.
				return new DashboardCustomContent(this.StatusDetails);
			}

			// Return nothing.
			return null;
		}
	}
}