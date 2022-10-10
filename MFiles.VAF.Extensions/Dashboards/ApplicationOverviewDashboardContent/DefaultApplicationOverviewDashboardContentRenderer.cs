using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Domain.Dashboards;
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
		public virtual DashboardPanelEx GetDashboardContent()
		{
			// Create panel.
			return new DashboardPanelEx()
			{
				InnerContent = new DashboardCustomContentEx($@"
					<table style='margin:12px;color:#333;'>
						<tbody>
							<tr>
								<td>Application Name:</td>
								<td>{ApplicationDefinition.Name}</td>
							</tr>
							<tr>
								<td>Version:</td>
								<td>{ApplicationDefinition.Version}</td>
							</tr>
							<tr>
								<td>Publisher:</td>
								<td>{ApplicationDefinition.Publisher}</td>
							</tr>
							<tr>
								<td>Description:</td>
								<td>{ApplicationDefinition.Description}</td>
							</tr>
						</tbody>
					</table>")
			};

		}

		IDashboardContent IApplicationOverviewDashboardContentRenderer.GetDashboardContent()
			=> this.GetDashboardContent();
	}
}
