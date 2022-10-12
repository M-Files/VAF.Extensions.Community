using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
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
		public string Title { get; set; } = Resources.Licensing.ApplicationDetailsPanel_Title;

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
		/// Generates the content for the "application details" and, if appropriate, "application licensing" dashboard sections.
		/// </summary>
		/// <returns>The content, or null to render nothing.</returns>
		public virtual IDashboardContent GetDashboardContent()
		{
			var content = new DashboardContentCollection();

			// Add the details panel, if one is returned.
			{
				var detailsPanel = this.GetApplicationDetailsDashboardContent();
				if (null != detailsPanel)
					content.Add(detailsPanel);
			}

			// TODO: Add a licensing panel, if one is returned.
			{
				if (this.ShowLicenseStatus)
				{

				}
			}

			// Return all the content.
			return content;

		}

		/// <summary>
		/// Generates the "Application Details" dashboard content.
		/// </summary>
		/// <returns></returns>
		public virtual DashboardPanelEx GetApplicationDetailsDashboardContent()
		{ 
			var innerContent = new DashboardContentCollection();

			var table = new DashboardTable();
			table.TableStyles.Remove("border");

			// Add the version.
			if (this.ShowVersion)
			{
				var row = table.AddRow();
				row.AddCell(Resources.Licensing.ApplicationDetailsPanel_RowHeaders_Version, DashboardTableCellType.Header)
					.HeaderStyles
					.Remove("border-bottom");
				row.AddCell(ApplicationDefinition.Version.ToString())
					.Styles.Add("width", "100%");
			}

			// Add the publisher and copyright if we have them.
			if (this.ShowPublisher && false == string.IsNullOrWhiteSpace(ApplicationDefinition.Publisher))
			{
				var row = table.AddRow();
				row.AddCell(Resources.Licensing.ApplicationDetailsPanel_RowHeaders_Publisher, DashboardTableCellType.Header)
					.HeaderStyles
					.Remove("border-bottom");
				row.AddCell(ApplicationDefinition.Publisher.ToString());
			}
			if (this.ShowCopyright && false == string.IsNullOrWhiteSpace(ApplicationDefinition.Copyright))
			{
				var row = table.AddRow();
				row.AddCell(Resources.Licensing.ApplicationDetailsPanel_RowHeaders_Copyright, DashboardTableCellType.Header)
					.HeaderStyles
					.Remove("border-bottom");
				row.AddCell($"&copy; {ApplicationDefinition.Copyright}");
			}

			innerContent.Add(table);

			// Add a marker to say whether this is MSM-compatible.
			if (this.ShowMultiServerModeStatus)
			{
				var element =
					ApplicationDefinition.MultiServerCompatible
					? new DashboardCustomContentEx($"<p style='color: green'>{Resources.Licensing.ApplicationDetailsPanel_ApplicationIsMultiServerModeCompatible.EscapeXmlForDashboard()}</p>")
					{
						Icon = "Resources/Images/Completed.png"
					}
					: new DashboardCustomContentEx($"<p style='color: red'>{Resources.Licensing.ApplicationDetailsPanel_ApplicationIsNotMultiServerModeCompatible.EscapeXmlForDashboard()}</p>")
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
					string colour = null, icon = null, status = null;
					switch (this.VaultApplication.License.LicenseStatus)
					{
						// OKAY STATES

						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusValid:
							colour = "green";
							icon = "Resources/Images/Completed.png";
							status = Resources.Licensing.MFApplicationLicenseStatusValid;
							break;
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusNotNeeded:
							status = Resources.Licensing.MFApplicationLicenseStatusNotNeeded;
							colour = "green";
							icon = "Resources/Images/Completed.png";
							break;
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusTrial:
							status = Resources.Licensing.MFApplicationLicenseStatusTrial;
							colour = "green";
							icon = "Resources/Images/Completed.png";
							break;
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusInstalled:
							status = Resources.Licensing.MFApplicationLicenseStatusInstalled;
							colour = "green";
							icon = "Resources/Images/Completed.png";
							break;

						// WARNING STATES

						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusUnknown:
							status = Resources.Licensing.MFApplicationLicenseStatusUnknown;
							colour = "darkorange";
							icon = "Resources/Images/Failed.png";
							break;
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusGracePeriod:
							status = Resources.Licensing.MFApplicationLicenseStatusGracePeriod;
							colour = "darkorange";
							icon = "Resources/Images/Failed.png";
							break;

						// ERROR STATES

						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusNotInstalled:
							status = Resources.Licensing.MFApplicationLicenseStatusNotInstalled;
							colour = "red";
							icon = "Resources/Images/Failed.png";
							break;
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusInvalid:
							status = Resources.Licensing.MFApplicationLicenseStatusInvalid;
							colour = "red";
							icon = "Resources/Images/Failed.png";
							break;
						case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusFormatError:
							status = Resources.Licensing.MFApplicationLicenseStatusFormatError;
							colour = "red";
							icon = "Resources/Images/Failed.png";
							break;
					}

					// Add the status if we can.
					if (null != status)
						innerContent.Add(new DashboardCustomContentEx($"<p style='color: {colour}'>{status}</p>")
						{
							Icon = icon
						});
				}
			}

			// Create panel.
			var panel = new DashboardPanelEx()
			{
				Title = this.Title,
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
