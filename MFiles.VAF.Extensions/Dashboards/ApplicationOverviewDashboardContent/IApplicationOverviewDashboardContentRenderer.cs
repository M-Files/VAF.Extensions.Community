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
		/// The title for the application details panel.
		/// </summary>
		string ApplicationDetailsTitle { get; set; }

		/// <summary>
		/// The title for the licensing status panel.
		/// </summary>
		string LicensingStatusTitle { get; set; }

		/// <summary>
		/// Whether to show the version section.
		/// </summary>
		bool ShowVersion { get; set; }

		/// <summary>
		/// Whether to show the publisher section.
		/// </summary>
		bool ShowPublisher { get; set; }

		/// <summary>
		/// Whether to show the copyright section.
		/// </summary>
		bool ShowCopyright { get; set; }

		/// <summary>
		/// Whether to show the MSM status.
		/// </summary>
		bool ShowMultiServerModeStatus { get; set; }

		/// <summary>
		/// Whether to show the licensing status.
		/// </summary>
		bool ShowLicenseStatus { get; set; }

		/// <summary>
		/// Gets the application overview dashboard content.
		/// </summary>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent();
	}
}
