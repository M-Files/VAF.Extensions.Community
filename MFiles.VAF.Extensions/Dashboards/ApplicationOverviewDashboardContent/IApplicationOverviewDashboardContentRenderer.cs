using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent;
using MFiles.VaultApplications.Logging.Configuration;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.ApplicationOverviewDashboardContent
{
	public interface IApplicationOverviewDashboardContentRenderer
	{
		/// <summary>
		/// Gets the application overview dashboard content.
		/// </summary>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent();
	}
}
