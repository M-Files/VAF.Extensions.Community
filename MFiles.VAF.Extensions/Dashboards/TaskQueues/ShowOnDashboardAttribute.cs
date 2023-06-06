using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards
{
	/// <summary>
	/// The type of approach used for recalculating interval-based task processors
	/// when the task is run manually.
	/// </summary>
	public enum RunNowRecalculationType
	{
		/// <summary>
		/// Does not cancel/recalculate any existing future executions.
		/// So if the task is scheduled to run every 30 minutes, and is run manually
		/// with 15 minutes left, the task still runs again in 15 minutes.
		/// </summary>
		LeaveFutureExecutions = 0,

		/// <summary>
		/// Recalculates any future executions once manual processing is complete.
		/// So if the task is scheudled to run every 30 minutes, and is run manually
		/// with 15 minutes left, the task is re-scheduled to run 30 minutes from completion.
		/// </summary>
		RecalculateFutureExecutions = 1
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
		/// <summary>
		/// The name to show for the background process.
		/// </summary>
		public string Name { get; set; } = null;

		/// <summary>
		/// The description to show for the background process.
		/// </summary>
		public string Description { get; set; } = null;

		/// <summary>
		/// If true, the "Run now" command will be shown for this background process.
		/// </summary>
		public bool ShowRunCommand { get; set; } = false;

		/// <summary>
		/// If specified, this will be used for the "Run now" command. Requires <see cref="ShowRunCommand"/> == true.
		/// This must separately be registered as a Domain Command, usually through overriding the <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.GetCommands(VAF.Configuration.AdminConfigurations.IConfigurationRequestContext)"/>.
		/// </summary>
		public string RunCommandId { get; set; }

		/// <summary>
		/// How to re-calculate any interval-based scheduled task processing
		/// in the situation where a user manually runs the task.
		/// </summary>
		public RunNowRecalculationType RunNowRecalculationType { get; set; }
			= RunNowRecalculationType.LeaveFutureExecutions;

		/// <summary>
		/// The default text shown on the "run now" button.
		/// </summary>
		public const string DefaultRunCommandDisplayText = "Run now";

		/// <summary>
		/// The default confirmation text used for the "run now" button, asking the user to confirm they want to do this.
		/// </summary>
		public const string DefaultRunCommandConfirmationText = null;

		/// <summary>
		/// The default text shown after the "run now" button has been clicked.
		/// </summary>
		public const string DefaultRunCommandSuccessText = "The background operation has been scheduled to run.";

		/// <summary>
		/// The text shown to the user as a popup when the background operation has been scheduled.
		/// </summary>
		public string RunCommandSuccessText { get; set; } = ShowOnDashboardAttribute.DefaultRunCommandSuccessText;

		/// <summary>
		/// The text shown to the user to confirm the command button click.
		/// </summary>
		public string RunCommandConfirmationText { get; set; } = ShowOnDashboardAttribute.DefaultRunCommandConfirmationText;

		/// <summary>
		/// The text shown on the command button.
		/// </summary>
		public string RunCommandDisplayText { get; set; } = ShowOnDashboardAttribute.DefaultRunCommandDisplayText;

		/// <summary>
		/// The default value for the <see cref="Order"/> property.
		/// </summary>
		protected internal const int DefaultOrder = 100;

		/// <summary>
		/// Used to determine the order in which the task processors are shown on the dashboard.
		/// Defaults to <see cref="DefaultOrder"/>.
		/// </summary>
		public int Order { get; set; } = ShowOnDashboardAttribute.DefaultOrder;

		public ShowOnDashboardAttribute(string name)
		{
			this.Name = name;
		}
	}
}
