using System;

namespace MFiles.VAF.Extensions.Dashboard
{
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class ShowRunCommandOnDashboardAttribute : Attribute
	{
		public string Name { get; set; }
		public string LabelText { get; set; }
		public string ButtonText { get; set; }
	}
}
