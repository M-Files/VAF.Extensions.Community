using MFiles.VAF.Configuration.Domain.Dashboards;

namespace MFiles.VAF.Extensions.Dashboard
{
	public static class DashboardHelper
	{
		public static IDashboardContent CreateSimpleButtonContent( string labelText, string buttonText, string commandId )
		{
			return new DashboardContentCollection
			{
				// Description
				new DashboardText( labelText ),

				// Toggle button.
				new DashboardDomainCommand
				{
					DomainCommandID = commandId,
					Title = buttonText,
					Style = DashboardCommandStyle.Button,
					Attributes = { { "style", "float:right" } }
				},

				// Float fix for the button.
				new DashboardCustomContent( "<div style='clear:both;'></div>" )
			};
		}
	}
}
