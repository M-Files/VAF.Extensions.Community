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
		public string Name { get; set; } = null;
		public string Description { get; set; } = null;
		public bool ShowRunCommand { get; set; } = false;

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

		public ShowOnDashboardAttribute(string name)
		{
			this.Name = name;
		}
	}
}
