using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent;
using MFiles.VaultApplications.Logging.Configuration;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.LoggingDashboardContent
{
	public interface ILoggingDashboardContentRenderer
	{
		/// <summary>
		/// Gets the logging dashboard content.
		/// </summary>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent(ILoggingConfiguration loggingConfiguration);
	}
}
