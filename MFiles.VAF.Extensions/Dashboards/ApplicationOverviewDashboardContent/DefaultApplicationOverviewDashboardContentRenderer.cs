using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.ApplicationOverviewDashboardContent
{
	public class DefaultApplicationOverviewDashboardContentRenderer<TSecureConfiguration>
		: IApplicationOverviewDashboardContentRenderer
		where TSecureConfiguration : class, new()
	{
		/// <inheritdoc />
		public bool ShowDescription { get; set; } = true;

		/// <inheritdoc />
		public bool ShowVersion { get; set; } = true;

		/// <inheritdoc />
		public bool ShowPublisher { get; set; } = true;

		/// <inheritdoc />
		public bool ShowCopyright { get; set; } = true;

		/// <inheritdoc />
		public bool ShowMultiServerModeStatus { get; set; } = true;

		/// <inheritdoc />
		public bool ShowLicenseStatus { get; set; } = true;

		/// <summary>
		/// The vault application that this is rendering for.
		/// </summary>
		public ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; set; }

		public DefaultApplicationOverviewDashboardContentRenderer(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
		{
			VaultApplication = vaultApplication 
				?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		/// <summary>
		/// Generates the content for the "application overview" dashboard section.
		/// </summary>
		/// <returns>The content, or null to render nothing.</returns>
		public virtual DashboardPanelEx GetDashboardContent()
		{
			var innerContent = new DashboardContentCollection();

			// If we have a description then add that,
			if (this.ShowDescription && false == string.IsNullOrWhiteSpace(ApplicationDefinition.Description))
				innerContent.Add(new DashboardCustomContentEx($"<p><em>{ApplicationDefinition.Description}</em></p>"));

			var table = new DashboardTable();
			table.TableStyles.Remove("border");

			// Add the version.
			if (this.ShowVersion)
			{
				var row = table.AddRow();
				row.AddCell("Version:", DashboardTableCellType.Header).HeaderStyles.Remove("border-bottom");
				row.AddCell(ApplicationDefinition.Version.ToString()).Styles.Add("width", "100%");
			}

			// Add the publisher and copyright if we have them.
			if (this.ShowPublisher && false == string.IsNullOrWhiteSpace(ApplicationDefinition.Publisher))
			{
				var row = table.AddRow();
				row.AddCell("Publisher:", DashboardTableCellType.Header).HeaderStyles.Remove("border-bottom");
				row.AddCell(ApplicationDefinition.Publisher.ToString());
			}
			if (this.ShowCopyright && false == string.IsNullOrWhiteSpace(ApplicationDefinition.Copyright))
			{
				var row = table.AddRow();
				row.AddCell("Copyright:", DashboardTableCellType.Header).HeaderStyles.Remove("border-bottom");
				row.AddCell($"&copy; {ApplicationDefinition.Copyright}");
			}

			innerContent.Add(table);

			// Add a marker to say whether this is MSM-compatible.
			if (this.ShowMultiServerModeStatus)
			{
				var element =
					ApplicationDefinition.MultiServerCompatible
					? new DashboardCustomContentEx($"<p style='color: green'>This application is marked as compatible with M-Files Multi-Server Mode.</p>")
					{
						Icon = "Resources/Images/Completed.png"
					}
					: new DashboardCustomContentEx($"<p style='color: red'>This application is <strong>NOT</strong> marked as compatible with M-Files Multi-Server Mode.</p>")
					{
						Icon = "Resources/Images/canceled.png"
					};
				element.Styles.AddOrUpdate("padding-top", "4px");
				innerContent.Add(element);
			}

			// Show the licensing status.
			if(this.ShowLicenseStatus)
			{
				if (null != this.VaultApplication?.License)
				{
					string colour = null, icon = null;
					switch(this.VaultApplication.License.LicenseStatus)
					{
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusValid:
							colour = "green";
							icon = "Resources/Images/Completed.png";
							break;
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusInvalid:
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusFormatError:
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusNotInstalled:
							colour = "red";
							icon = "Resources/Images/Failed.png";
							break;
					}
					innerContent.Add(new DashboardCustomContentEx($"<p style='color: {colour}'><strong>License:</strong> {this.VaultApplication.GetApplicationLicenseDetails(false, null)}</p>")
					{
						Icon = icon
					});
				}
			}

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
