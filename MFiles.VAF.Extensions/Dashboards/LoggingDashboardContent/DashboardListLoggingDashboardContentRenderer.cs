using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.Targets;

namespace MFiles.VAF.Extensions.Dashboards.LoggingDashboardContent
{
	public class DashboardListLoggingDashboardContentRenderer
		: ILoggingDashboardContentRenderer
	{
		/// <summary>
		/// Allows the user to choose which log files to select.  If false, all log files are downloaded.
		/// </summary>
		protected bool AllowUserToSelectLogFiles { get; set; } = false;

		/// <summary>
		/// Allows log file download via the dashboard.
		/// </summary>
		protected bool AllowDashboardLogFileDownload { get; set; } = true;

		/// <summary>
		/// Allows admins to view recent log entries via the dashboard.
		/// </summary>
		protected bool AllowDashboardLogEntryViewing { get; set; } = true;

		public virtual DashboardPanelEx GetDashboardContent(ILoggingConfiguration loggingConfiguration)
		{
			// If we don't have any logging configuration then return null.
			if (loggingConfiguration == null)
				return null;

			// If logging is not enabled then return a simple panel.
			if (!loggingConfiguration.Enabled)
			{
				return new DashboardPanelEx()
				{
					Title = Resources.Dashboard.Logging_DashboardTitle,
					InnerContent = new DashboardContentCollection
					{
						new DashboardCustomContent($"<em>{Resources.Dashboard.Logging_LoggingNotEnabled}</em>")
					}
				};
			}

			// Create the table
			var table = new DashboardTable();
			{
				var header = table.AddRow(DashboardTableRowType.Header);
				header.AddCells
				(
					Resources.Dashboard.Logging_Table_NameHeader,
					Resources.Dashboard.Logging_Table_LogLevelsHeader,
					""
				);
			}
			table.Styles.Add("background-color", "#f2f2f2");
			table.Styles.Add("padding", "10px");
			table.Styles.Add("margin", "16px 0px");

			// Retrieve all loggers.
			var logTargetConfiguration = loggingConfiguration.GetAllLogTargetConfigurations();

			// Add each in turn to the list.
			foreach (var config in logTargetConfiguration)
			{
				// Build up the row.
				var row = table.AddRow();
				if (false == config.Enabled)
				{
					// Not enabled.
					//row.Styles.Add("text-decoration", "line-through");
					row.Attributes.Add("title", Resources.Dashboard.Logging_TargetNotEnabled);
					row.Styles.AddOrUpdate("color", Resources.Dashboard.Logging_ColorNotEnabled);
				}

				// Sort out the name.
				{
					var name = new DashboardCustomContentEx($"{(string.IsNullOrWhiteSpace(config.Name) ? Resources.Dashboard.Logging_Table_UnnamedTarget : config.Name)} ({config.TypeName})");

					if (config.Enabled == false)
					{
						name.Icon = "Resources/Images/notenabled.png";
					}
					// Validation can take some time to run; let's not incur that cost.
					else if (config.GetValidationFindings().Any(f => f.Type == ValidationFindingType.Error || f.Type == ValidationFindingType.Exception))
					{
						name.Icon = "Resources/Images/error.png";
						row.Attributes.Add("title", Resources.Dashboard.Logging_TargetValidationErrors);
						row.Styles.AddOrUpdate("color", Resources.Dashboard.Logging_ColorValidationErrors);
					}
					else
					{
						name.Icon = "Resources/Images/enabled.png";
					}

					row.AddCell(name);
				}
				row.AddCell($"{config.MinimumLogLevel} and higher");

				// If it's the default one then allow downloads.
				if (config is DefaultTargetConfiguration)
				{
					// Add whatever buttons, according to app configuration.
					var buttons = new DashboardContentCollection();
					if (this.AllowDashboardLogFileDownload)
					{
						buttons.Add(new DashboardDomainCommandEx
						{
							DomainCommandID = this.AllowUserToSelectLogFiles
									? Dashboards.Commands.ShowSelectLogDownloadDashboardCommand.CommandId
									: Dashboards.Commands.DownloadSelectedLogsDashboardCommand.CommandId,
							Title = Resources.Dashboard.Logging_Table_DownloadLogs,
							Icon = "Resources/Images/Download.png"
						});
					}
					if (this.AllowDashboardLogEntryViewing)
					{
						buttons.Add(new DashboardDomainCommandEx
						{
							DomainCommandID = Dashboards.Commands.ShowLatestLogEntriesDashboardCommand.CommandId,
							Title = Resources.Dashboard.Logging_Table_ShowLatestLogEntries,
							Icon = "Resources/Images/viewlogs.png"
						});
					}

					// Add the buttons to the cell
					var cell = row.AddCell(buttons);
					cell.Styles.AddOrUpdate("text-align", "right");
				}
				else
				{
					row.AddCell("");
				}
			}

			// Return the panel.
			return new DashboardPanelEx()
			{
				Title = Resources.Dashboard.Logging_DashboardTitle,
				InnerContent = new DashboardContentCollection
				{
					0 == table.Rows.Count(r => r.DashboardTableRowType == DashboardTableRowType.Body)
						? (IDashboardContent)new DashboardList()
						{
							Items = new List<DashboardListItem>()
							{
								new DashboardListItem()
								{
									Title = Resources.Dashboard.Logging_NoLogTargetsAreConfigured,
									StatusSummary = new DomainStatusSummary()
									{
										Status = DomainStatus.Undefined
									}
								}
							}
						}
						: (IDashboardContent)table
				}
			};
		}

		IDashboardContent ILoggingDashboardContentRenderer.GetDashboardContent(ILoggingConfiguration loggingConfiguration)
			=> this.GetDashboardContent(loggingConfiguration);
	}
}
