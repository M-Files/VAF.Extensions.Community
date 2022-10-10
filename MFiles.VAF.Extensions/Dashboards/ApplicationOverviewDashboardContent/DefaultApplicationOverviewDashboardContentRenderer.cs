using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.ApplicationOverviewDashboardContent
{
	public class DefaultApplicationOverviewDashboardContentRenderer
		: IApplicationOverviewDashboardContentRenderer
	{
		/// <summary>
		/// Generates the content for the "application overview" dashboard section.
		/// </summary>
		/// <returns>The content, or null to render nothing.</returns>
		public virtual DashboardPanelEx GetDashboardContent()
		{
			var innerContent = new DashboardContentCollection();

			// If we have a description then add that,
			if (false == string.IsNullOrWhiteSpace(ApplicationDefinition.Description))
				innerContent.Add(new DashboardCustomContentEx($"<p><em>{ApplicationDefinition.Description}</em></p>"));

			// Add the version.
			innerContent.Add(new DashboardCustomContentEx($"<p><em><strong>Version:</strong> {ApplicationDefinition.Version}</em></p>"));

			// Add the publisher and copyright if we have them.
			if (false == string.IsNullOrWhiteSpace(ApplicationDefinition.Publisher))
				innerContent.Add(new DashboardCustomContentEx($"<p><em><strong>Publisher:</strong> {ApplicationDefinition.Publisher}</em></p>"));
			if (false == string.IsNullOrWhiteSpace(ApplicationDefinition.Copyright))
				innerContent.Add(new DashboardCustomContentEx($"<p><em><strong>Copyright:</strong> &copy; {ApplicationDefinition.Copyright}</em></p>"));

			// Add a marker to say whether this is MSM-compatible.
			innerContent.Add
			(
				ApplicationDefinition.MultiServerCompatible
				? new DashboardCustomContentEx($"<p style='color: green'>This application is marked as compatible with M-Files Multi-Server Mode.</p>")
				{
					Icon = "Resources/Images/Completed.png"
				}
				: new DashboardCustomContentEx($"<p style='color: red'>This application is <strong>NOT</strong> marked as compatible with M-Files Multi-Server Mode.</p>")
				{
					Icon = "Resources/Images/canceled.png"
				}
			);

			// Create panel.
			var panel = new DashboardPanelEx()
			{
				Title = $"{ApplicationDefinition.Name}",
				InnerContent = innerContent
			};
			panel.InnerContentStyles.AddOrUpdate("padding-left", "10px");
			panel.InnerContentStyles.AddOrUpdate("margin-left", "10px");
			panel.InnerContentStyles.AddOrUpdate("border-left", "1px solid #EEE");
			return panel;

		}

		IDashboardContent IApplicationOverviewDashboardContentRenderer.GetDashboardContent()
			=> this.GetDashboardContent();
	}
}
