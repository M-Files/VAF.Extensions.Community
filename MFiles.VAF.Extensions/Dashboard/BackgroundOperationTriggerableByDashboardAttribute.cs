using System;

namespace MFiles.VAF.Extensions.Dashboard
{
	// TODO: PLEASE come up with a better name for this
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class BackgroundOperationTriggerableByDashboardAttribute : Attribute
	{
		public string Name { get; set; }
		public string LabelText { get; set; }
		public string ButtonText { get; set; }
	}
}
