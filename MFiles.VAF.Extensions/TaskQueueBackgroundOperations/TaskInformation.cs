using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Details about the current job status.
	/// </summary>
	public class TaskInformation
	{
		/// <summary>
		/// The datetime that this task was started.
		/// </summary>
		public DateTime? Started { get; set; }

		/// <summary>
		/// The datetime that this task was last updated.
		/// </summary
		public DateTime? LastActivity { get; set; }

		/// <summary>
		/// The datetime that this task was completed.
		/// </summary>
		public DateTime? Completed { get; set; }

		/// <summary>
		/// Details about the current status (e.g. "Processing object 5 of 100").
		/// </summary>
		public string StatusDetails { get; set; }

		/// <summary>
		/// Details on how far through the process the task is.
		/// </summary>
		public int? PercentageComplete { get; set; }

		/// <summary>
		/// The current task state.
		/// </summary>
		public MFTaskState CurrentTaskState { get; internal set; }

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
					Text = this.StatusDetails,
					TaskState = this.CurrentTaskState
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

		/// <summary>
		/// Gets the time elapsed between the latest activity and the activation timestamp.
		/// </summary>
		/// <param name="taskInfo">The task in question.</param>
		/// <returns>The time span, or <see cref="TimeSpan.Zero"/> if null.</returns>
		public TimeSpan GetElapsedTime()
		{
			// If we have no start or last activity date then return zero.
			if (false == this.Started.HasValue
				|| false == this.LastActivity.HasValue)
				return TimeSpan.Zero;

			// What's the difference?
			var delta = this.LastActivity.Value.Subtract(this.Started.Value);

			// If it's less than a second then zero.
			return delta < TimeSpan.FromSeconds(1)
				? TimeSpan.Zero
				: delta;
		}
	}
}