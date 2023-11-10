using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.ApplicationOverviewDashboardContent
{
	public class DefaultApplicationOverviewDashboardContentRenderer<TSecureConfiguration>
		: DefaultApplicationOverviewDashboardContentRenderer<TSecureConfiguration, LicenseContentBase>
		where TSecureConfiguration : class, new()
	{

		public DefaultApplicationOverviewDashboardContentRenderer(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
			: base(vaultApplication)
		{
		}

	}
	public class DefaultApplicationOverviewDashboardContentRenderer<TSecureConfiguration, TLicenseContent>
		: IApplicationOverviewDashboardContentRenderer
		where TSecureConfiguration : class, new()
		where TLicenseContent : LicenseContentBase
	{
		/// <inheritdoc />
		public string ApplicationDetailsTitle { get; set; } = Resources.Licensing.ApplicationDetailsPanel_Title;

		/// <inheritdoc />
		public string LicensingStatusTitle { get; set; } = Resources.Licensing.LicensingStatusPanel_Title;

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
		/// If populated, will show available modules as well as licensed ones.
		/// </summary>
		/// <remarks>
		/// The key should be the specific text in the license, the value can be a display value.
		/// </remarks>
		public Dictionary<string, string> AllModules { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// The vault application that this is rendering for.
		/// </summary>
		public ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; set; }

		public DefaultApplicationOverviewDashboardContentRenderer(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
		{
			this.VaultApplication = vaultApplication 
				?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		/// <summary>
		/// Generates the content for the "application details" and, if appropriate, "application licensing" dashboard sections.
		/// </summary>
		/// <returns>The content, or null to render nothing.</returns>
		public virtual IDashboardContent GetDashboardContent()
		{
			var content = new DashboardContentCollection();
			IDashboardContent root = content;

			if(this.VaultApplication.DashboardLogo != null)
			{
				// Create a table so we can put the logo on as well.
				var table = new DashboardTable();
				table.TableStyles.AddOrUpdate("border", "0px");
				var row = table.AddRow();

				// Create a new cell for the content.
				var contentCell = row.AddCell();
				contentCell.Styles.AddOrUpdate("vertical-align", "top");
				contentCell.InnerContent = content;

				// Add the logo.
				row.AddCell(this.VaultApplication.DashboardLogo);

				// The root will now be the table, not the collection.
				root = table;
			}

			// Add the details panel, if one is returned.
			{
				var detailsPanel = this.GetApplicationDetailsDashboardContent();
				if (null != detailsPanel)
					content.Add(detailsPanel);
			}

			// Add a licensing panel, if one is returned.
			{
				var licensingPanel = this.GetLicensingDetailsDashboardContent
				(
					this.VaultApplication?.License?.Content<TLicenseContent>(),
					this.VaultApplication?.License?.ServerLicense
				);
				if (null != licensingPanel)
					content.Add(licensingPanel);
			}

			// Return all the content.
			return root;

		}

		/// <summary>
		/// Generates the "Licensing Status" dashboard content.
		/// </summary>
		/// <returns>The dashboard panel, or null if nothing should be shown.</returns>
		public virtual DashboardPanelEx GetLicensingDetailsDashboardContent(TLicenseContent licenseContent, LicenseStatus serverLicenseStatus)
		{
			// Sanity.
			if (null == licenseContent || false == this.ShowLicenseStatus)
				return null;

			// Let's generate the various statements!
			var content = new DashboardContentCollection();

			// General status.
			{
				var licenseStatus = this.GetLicenseStatusDashboardContent(licenseContent.LicenseStatus);
				if(null != licenseStatus)
					content.Add(licenseStatus);
			}

			// Who is it licensed to?
			{
				if (false == string.IsNullOrWhiteSpace(licenseContent?.LicensedTo))
					content.Add(this.GetStatementDashboardContent
					(
						string.Format(Resources.Licensing.LicensingStatusPanel_LicensedTo, licenseContent?.LicensedTo),
						StatementType.Okay
					));
			}

			// Is it okay for this serial number?
			{
				var serialNumber = serverLicenseStatus?.SerialNumber;
				if(false == string.IsNullOrWhiteSpace(licenseContent.MFilesSerialNumber))
					content.Add(this.GetTernaryStatementDashboardContent
					(
						serialNumber == licenseContent.MFilesSerialNumber,
						Resources.Licensing.LicensingStatusPanel_ValidForThisServer,
						Resources.Licensing.LicensingStatusPanel_NotValidForThisServer
					));
			}

			// Server version?
			{
				if (licenseContent.ServerVersions?.Any() ?? false)
				{
					MFilesVersion serverVersion = this.VaultApplication.PermanentVault.GetServerVersionOfVault();
					content.Add(this.GetTernaryStatementDashboardContent
					(
						licenseContent.ServerVersions.Any((string f) => VersionMatch(serverVersion, f)),
						Resources.Licensing.LicensingStatusPanel_ValidInThisServerVersion,
						string.Format(Resources.Licensing.LicensingStatusPanel_NotValidInThisServerVersion, serverVersion?.ToString())
					));
				}
			}

			// Is it okay in this vault?
			{
				if (licenseContent.Vaults?.Any() ?? false)
				{
					var vaultGuid = this.VaultApplication.PermanentVault.GetGUID();
					content.Add(this.GetTernaryStatementDashboardContent
					(
						licenseContent.Vaults == null || licenseContent.Vaults.Any(v => v.Equals(vaultGuid, StringComparison.OrdinalIgnoreCase)),
						Resources.Licensing.LicensingStatusPanel_ValidInThisVault,
						string.Format(Resources.Licensing.LicensingStatusPanel_NotValidInThisVault, vaultGuid)
					));
				}
			}

			// When does it expire?
			{
				if (DateTime.TryParseExact(licenseContent.LicenseExpireDate, "d.M.yyyy", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime expiry))
				{
					var days = expiry.Subtract(DateTime.Now).Days;
					content.Add(this.GetTernaryStatementDashboardContent
					(
						expiry > DateTime.Now,
						string.Format(Resources.Licensing.LicensingStatusPanel_LicenseExpiresInXXXXDays, days),
						string.Format(Resources.Licensing.LicensingStatusPanel_LicenseExpiredXXXXDaysAgo, DateTime.Now.Subtract(expiry).Days),
						successfulStatementType: days > 30
							? StatementType.Okay
							: StatementType.Warning
					));
				}
			}

			// Maintenance.
			{
				if (DateTime.TryParseExact(licenseContent.MaintenanceExpireDate, "d.M.yyyy", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime expiry))
				{
					var days = expiry.Subtract(DateTime.Now).Days;
					content.Add(this.GetTernaryStatementDashboardContent
					(
						expiry > DateTime.Now,
						string.Format(Resources.Licensing.LicensingStatusPanel_MaintenanceExpiresInXXXXDays, days),
						string.Format(Resources.Licensing.LicensingStatusPanel_MaintenanceExpiredXXXXDaysAgo, DateTime.Now.Subtract(expiry).Days),
						successfulStatementType: days > 30
							? StatementType.Okay
							: StatementType.Warning
					));
				}
			}

			// Users.
			{
				// Named users.
				if(licenseContent.NamedUsers >= 0)
				{
					{
						var licensed = licenseContent.NamedUsers;
						var used = serverLicenseStatus.NumOfNamedUserLicenses;
						content.Add(this.GetTernaryStatementDashboardContent
						(
							used == -1 || licensed >= used,
							string.Format(Resources.Licensing.LicensingStatusPanel_NamedUsers_Valid, licensed, used == -1 ? "unlimited" : used.ToString()),
							string.Format(Resources.Licensing.LicensingStatusPanel_NamedUsers_Exceeded, licensed, used == -1 ? "unlimited" : used.ToString())
						));
					}
				}

				// Concurrent users.
				if (licenseContent.ConcurrentUsers >= 0)
				{
					{
						var licensed = licenseContent.ConcurrentUsers;
						var used = serverLicenseStatus.NumOfConcurrentUserLicenses;
						content.Add(this.GetTernaryStatementDashboardContent
						(
							used == -1 || licensed >= used,
							string.Format(Resources.Licensing.LicensingStatusPanel_ConcurrentUsers_Valid, licensed, used == -1 ? "unlimited" : used.ToString()),
							string.Format(Resources.Licensing.LicensingStatusPanel_ConcurrentUsers_Exceeded, licensed, used == -1 ? "unlimited" : used.ToString())
						));
					}
				}

				// Read-only users.
				if (licenseContent.ReadOnlyUsers >= 0)
				{
					{
						var licensed = licenseContent.ReadOnlyUsers;
						var used = serverLicenseStatus.NumOfReadonlyLicenses;
						content.Add(this.GetTernaryStatementDashboardContent
						(
							used == -1 || licensed >= used,
							string.Format(Resources.Licensing.LicensingStatusPanel_ReadOnlyUsers_Valid, licensed, used == -1 ? "unlimited" : used.ToString()),
							string.Format(Resources.Licensing.LicensingStatusPanel_ReadOnly_Exceeded, licensed, used == -1 ? "unlimited" : used.ToString())
						));
					}
				}
			}

			// Groups.
			{
				if(licenseContent.Groups?.Any() ?? false)
				{
					foreach (var group in licenseContent.Groups)
					{
						if (null == group || string.IsNullOrWhiteSpace(group.Group))
							continue;
						int userGroupIDByAlias = this.VaultApplication.PermanentVault.UserGroupOperations.GetUserGroupIDByAlias(group.Group);
						if (userGroupIDByAlias != -1)
						{
							UserGroup userGroup = this.VaultApplication.PermanentVault.UserGroupOperations.GetUserGroupAdmin(userGroupIDByAlias).UserGroup;
							content.Add(this.GetTernaryStatementDashboardContent
							(
								userGroup.Members.Count > group.Len,
								string.Format(Resources.Licensing.LicensingStatusPanel_GroupMembership_Valid, group.Group, group.Len, userGroup.Members.Count),
								string.Format(Resources.Licensing.LicensingStatusPanel_GroupMembership_Exceeded, group.Group, group.Len, userGroup.Members.Count)
							));
						}
					}
				}
			}

			// Modules.
			{
				if (licenseContent.Modules?.Any() ?? false)
				{
					var modules = new DashboardContentCollection
					{
						new DashboardCustomContentEx($"<p>The following modules are licensed:</p>")
					};

					// If we know about all the modules then we can show the ones that are unlicensed too.
					if (this.AllModules?.Any() ?? false)
					{
						// We have more details about the modules
						foreach (var kvp in this.AllModules)
						{
							modules.Add
							(
								this.GetTernaryStatementDashboardContent
								(
									licenseContent.Modules.Contains(kvp.Key),
									kvp.Value, // Use the same value for success and failure.
									kvp.Value, // Use the same value for success and failure.
									failureStatementType: StatementType.Unknown // Unlicensed modules is not an error or warning.
								)
							);
						}
					}
					else
					{
						// We can just list out the data that was in the license.
						foreach (var module in licenseContent.Modules)
						{
							modules.Add(this.GetStatementDashboardContent(module, StatementType.Okay));
						}
					}

					// Add the panel for the modules.
					var modulesPanel = new DashboardPanelEx()
					{
						InnerContent = modules
					};
					modulesPanel.Styles.AddOrUpdate("margin", "8px 0px");
					content.Add(modulesPanel);
				}
			}

			// Create panel.
			var panel = new DashboardPanelEx()
			{
				Title = this.LicensingStatusTitle,
				InnerContent = content
			};
			panel.InnerContentStyles.AddOrUpdate("padding-left", "10px");
			panel.InnerContentStyles.AddOrUpdate("margin-left", "10px");
			panel.InnerContentStyles.AddOrUpdate("border-left", "1px solid #EEE");
			return panel;
		}

		/// <summary>
		/// Generates the "Application Details" dashboard content.
		/// </summary>
		/// <returns>The dashboard panel, or null if nothing should be shown.</returns>
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
				row.AddCell(ApplicationDefinition.Publisher.EscapeXmlForDashboard());
			}
			if (this.ShowCopyright && false == string.IsNullOrWhiteSpace(ApplicationDefinition.Copyright))
			{
				var row = table.AddRow();
				row.AddCell(Resources.Licensing.ApplicationDetailsPanel_RowHeaders_Copyright, DashboardTableCellType.Header)
					.HeaderStyles
					.Remove("border-bottom");
				row.AddCell($"&copy; {ApplicationDefinition.Copyright.EscapeXmlForDashboard()}");
			}

			innerContent.Add(table);

			// Add a marker to say whether this is MSM-compatible.
			if (this.ShowMultiServerModeStatus)
			{
				var element = 
					ApplicationDefinition.MultiServerCompatible
					? this.GetStatementDashboardContent(Resources.Licensing.ApplicationDetailsPanel_ApplicationIsMultiServerModeCompatible, StatementType.Okay)
					: this.GetStatementDashboardContent(Resources.Licensing.ApplicationDetailsPanel_ApplicationIsNotMultiServerModeCompatible, StatementType.Error);
				element.Styles.AddOrUpdate("padding-top", "4px");
				innerContent.Add(element);
			}

			// Create panel.
			var panel = new DashboardPanelEx()
			{
				Title = this.ApplicationDetailsTitle,
				InnerContent = innerContent
			};
			panel.InnerContentStyles.AddOrUpdate("padding-left", "10px");
			panel.InnerContentStyles.AddOrUpdate("margin-left", "10px");
			panel.InnerContentStyles.AddOrUpdate("border-left", "1px solid #EEE");
			return panel;

		}

		public enum StatementType
		{
			Unknown = 0,
			Okay = 1,
			Warning = 2,
			Error = 3
		}

		protected virtual DashboardContentBase GetTernaryStatementDashboardContent
		(
			bool test,
			string successfulMessage,
			string failureMessage,
			StatementType successfulStatementType = StatementType.Okay,
			StatementType failureStatementType = StatementType.Error
		)
		{
			return test
				? this.GetStatementDashboardContent(successfulMessage, successfulStatementType)
				: this.GetStatementDashboardContent(failureMessage, failureStatementType);
		}

		protected virtual DashboardContentBase GetLicenseStatusDashboardContent(MFilesAPI.MFApplicationLicenseStatus licenseStatus)
		{
			StatementType statementType = StatementType.Unknown;
			string status = null;
			switch (licenseStatus)
			{
				// OKAY STATES

				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusValid:
					statementType = StatementType.Okay;
					status = Resources.Licensing.MFApplicationLicenseStatusValid;
					break;
				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusNotNeeded:
					status = Resources.Licensing.MFApplicationLicenseStatusNotNeeded;
					statementType = StatementType.Okay;
					break;
				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusInstalled:
					status = Resources.Licensing.MFApplicationLicenseStatusInstalled;
					statementType = StatementType.Okay;
					break;

				// WARNING STATES

				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusTrial:
					status = Resources.Licensing.MFApplicationLicenseStatusTrial;
					statementType = StatementType.Warning;
					break;
				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusUnknown:
					status = Resources.Licensing.MFApplicationLicenseStatusUnknown;
					statementType = StatementType.Warning;
					break;
				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusGracePeriod:
					status = Resources.Licensing.MFApplicationLicenseStatusGracePeriod;
					statementType = StatementType.Warning;
					break;

				// ERROR STATES

				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusNotInstalled:
					status = Resources.Licensing.MFApplicationLicenseStatusNotInstalled;
					statementType = StatementType.Error;
					break;
				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusInvalid:
					status = Resources.Licensing.MFApplicationLicenseStatusInvalid;
					statementType = StatementType.Error;
					break;
				case MFilesAPI.MFApplicationLicenseStatus.MFApplicationLicenseStatusFormatError:
					status = Resources.Licensing.MFApplicationLicenseStatusFormatError;
					statementType = StatementType.Error;
					break;
			}

			// Add the status if we can.
			if (null != status)
				return this.GetStatementDashboardContent(status, statementType);
			return null;

		}

		protected virtual DashboardContentBase GetStatementDashboardContent(string message, StatementType statementType)
		{
			string colour;
			string icon;
			switch (statementType)
			{
				case StatementType.Okay:
					colour = "green";
					icon = "Resources/Images/Completed.png";
					break;
				case StatementType.Warning:
					colour = "darkorange";
					icon = "Resources/Images/Failed.png";
					break;
				case StatementType.Error:
					colour = "red";
					icon = "Resources/Images/Failed.png";
					break;
				default:
					colour = "#333";
					icon = "Resources/Images/notenabled.png";
					break;
			}
			return new DashboardCustomContentEx($"<p style='color: {colour}'>{message?.EscapeXmlForDashboard()}</p>")
			{
				Icon = icon
			};
		}

		IDashboardContent IApplicationOverviewDashboardContentRenderer.GetDashboardContent()
			=> this.GetDashboardContent();

		#region Implementation copied from LicenseContentBase

		//
		// Summary:
		//     Return if the filter matches the version. Filter contain any number of parts
		//     "12", "11.2", "11.1.4320", "10.3.3210.123"
		//
		// Parameters:
		//   ver:
		//     Version
		//
		//   filter:
		//     Filter
		//
		// Returns:
		//     True when the filter matches the version.
		private bool VersionMatch(MFilesVersion ver, string filter)
		{
			Match match = Regex.Match(filter, "^(?<major>[0-9]+)(.(?<minor>[0-9]+)(.(?<build>[0-9]+)(.(?<patch>[0-9]+))?)?)?$");
			if (!match.Success)
			{
				return false;
			}

			if (match.Groups["major"].Success && int.Parse(match.Groups["major"].Value) != ver.Major)
			{
				return false;
			}

			if (match.Groups["minor"].Success && int.Parse(match.Groups["minor"].Value) != ver.Minor)
			{
				return false;
			}

			if (match.Groups["build"].Success && int.Parse(match.Groups["build"].Value) != ver.Build)
			{
				return false;
			}

			if (match.Groups["patch"].Success && int.Parse(match.Groups["patch"].Value) != ver.Patch)
			{
				return false;
			}

			return true;
		}

		#endregion

	}
}
