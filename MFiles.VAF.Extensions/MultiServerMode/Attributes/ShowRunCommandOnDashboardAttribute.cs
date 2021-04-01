using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// Adds a run command to dashboard in admin, button will trigger a RunOnce on the corresponding background operation
	/// Currently this is only supported on TaskQueueBackgroundOperation properties or fields on the VaultApplication class
	/// The current implementation does not handle generics, i.e. TaskQueueBackgroundOperation with TDirective
	/// Your operation is responsible for handling multiple concurrent runs
	/// NOTE: the default task queue manager runs items concurrently
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ShowRunCommandOnDashboardAttribute : Attribute
	{
		public string ButtonText { get; set; }
		public string Message { get; set; } = BackgroundOperationDashboardDisplayOptions.DefaultRunCommandMessageText;

		public ShowRunCommandOnDashboardAttribute(string buttonText = BackgroundOperationDashboardDisplayOptions.DefaultRunCommandText)
		{
			ButtonText = buttonText;
		}
	}
}
