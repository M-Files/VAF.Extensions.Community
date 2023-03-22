using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Configuration.Logging;

namespace MFiles.VAF.Extensions.Dashboards.ReplicationPackageDashboardContent
{
	public interface IReplicationPackageDashboardContentRenderer<TSecureConfiguration>
		where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// Gets the dashboard content.
		/// </summary>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent
		(
			IConfigurationRequestContext context
		);
	}
}
