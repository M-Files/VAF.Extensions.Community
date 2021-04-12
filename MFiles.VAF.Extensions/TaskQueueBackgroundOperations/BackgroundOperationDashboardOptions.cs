using MFiles.VAF.Configuration.AdminConfigurations;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Options for how to display the background operation in the dashboard.
	/// </summary>
	public class BackgroundOperationDashboardOptions
	{
		public const string DefaultRunCommandText = "Run now";
		public const string DefaultRunCommandMessageText = "The background operation has been scheduled to run.";

		/// <summary>
		/// The text shown on the run command in the dashboard.
		/// Only used if <see cref="ShowRunCommandInDashboard"/> is true.
		/// </summary>
		public string RunCommandText
		{
			get => this.RunCommand.DisplayName;
			set => this.RunCommand.DisplayName = value;
		}

		/// <summary>
		/// The text shown to the user when they click the run command in the dashboard.
		/// Only used if <see cref="ShowRunCommandInDashboard"/> is true.
		/// </summary>
		public string RunCommandMessageText
		{
			get => this.RunCommand.ConfirmMessage;
			set => this.RunCommand.ConfirmMessage = value;
		}

		/// <summary>
		/// Whether to show the run command in the dashboard.
		/// If true, the dashboard will render a "Run now" button that will allow the user
		/// to force a run of the background operation, even if the schedule does not
		/// require it to run immediately.
		/// </summary>
		public bool ShowRunCommandInDashboard { get; set; } = false;

		/// <summary>
		/// Whether to show the background operation in the dashboard.
		/// </summary>
		public bool ShowBackgroundOperationInDashboard { get; set; } = true;

		/// <summary>
		/// The run command to be shown in the dashboard.
		/// </summary>
		public CustomDomainCommand RunCommand { get; private set; }
			= new CustomDomainCommand()
			{
				ConfirmMessage = DefaultRunCommandMessageText,
				DisplayName = DefaultRunCommandText,
				Blocking = true
			};
	}
}