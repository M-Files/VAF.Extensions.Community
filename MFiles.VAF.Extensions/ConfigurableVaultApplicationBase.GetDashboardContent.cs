// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF;
using System;
using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VaultApplications.Logging.NLog.ExtensionMethods;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{

		/// <inheritdoc />
		public override string GetDashboardContent(IConfigurationRequestContext context)
		{
			// Create a new dashboard that refreshes every 30 seconds.
			var dashboard = new StatusDashboard()
			{
				RefreshInterval = 30
			};

			// If there's some base content then add that.
			var baseContent = base.GetDashboardContent(context);
			if (false == string.IsNullOrWhiteSpace(baseContent))
				dashboard.AddContent(new DashboardCustomContent(baseContent));

			// Do we have any asynchronous operation content?
			var asynchronousOperationContent = this.GetAsynchronousOperationDashboardContent();
			if(null != asynchronousOperationContent)
				dashboard.AddContent(asynchronousOperationContent);

			// Do we have any logging content?
			var loggingContent = this.GetLoggingDashboardContent();
			if (null != loggingContent)
				dashboard.AddContent(loggingContent);

			// Return the dashboard.
			return dashboard.ToString();
		}

		/// <summary>
		/// Returns the dashboard content showing asynchronous operation status.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no background operation managers, background operations or task processors.</returns>
		public virtual IDashboardContent GetAsynchronousOperationDashboardContent()
		{
			// Declare our list which will go into the panel.
			var list = new DashboardList();

			// Iterate over all the background operation managers we can find
			// and add each of their background operations to the list.
			var taskQueueBackgroundOperationManagers = this
				.GetType()
				.GetPropertiesAndFieldsOfType<TaskQueueBackgroundOperationManager<TSecureConfiguration>>(this);
			foreach (var manager in taskQueueBackgroundOperationManagers.OrderBy(m => m.DashboardSortOrder))
			{
				var listItems = manager.GetDashboardContent();
				if (null == listItems)
					continue;
				list.Items.AddRange(listItems);
			}

			// Get the content from the task queue resolver.
			{
				var listItems = this.TaskManager.GetDashboardContent(this.TaskQueueResolver);
				if (null != listItems)
					list.Items.AddRange(listItems);
			}

			// Did we get anything?
			if (0 == list.Items.Count)
				list.Items.Add(new DashboardListItem()
				{
					Title = Resources.Dashboard.AsynchronousOperations_ThereAreNoCurrentAsynchronousOperations,
					StatusSummary = new DomainStatusSummary()
					{
						Status = VAF.Configuration.Domain.DomainStatus.Undefined
					}
				});

			// Return the panel.
			return new DashboardPanelEx()
			{
				Title = Resources.Dashboard.AsynchronousOperations_DashboardTitle,
				InnerContent = new DashboardContentCollection
				{
					new DashboardCustomContent($"<em>{Resources.Dashboard.TimeOnServer.EscapeXmlForDashboard(DateTime.Now.ToLocalTime().ToString("HH:mm:ss"))}</em>"),
					list
				}
			};
		}

		/// <summary>
		/// Returns the dashboard content showing logging status.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no logging data is available or configured.</returns>
		public virtual IDashboardContent GetLoggingDashboardContent()
		{
			// If we don't have any logging configuration then return null.
			if (!(this.Configuration is Configuration.IConfigurationWithLoggingConfiguration configurationWithLogging))
				return null;

			// Get the logging configuration.
			var loggingConfiguration = configurationWithLogging?.GetLoggingConfiguration();
			if (null == loggingConfiguration)
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
					Resources.Dashboard.Logging_Table_LogLevelsHeader
				);
			}
			table.Styles.Add("background-color", "#f2f2f2");
			table.Styles.Add("padding", "10px");

			// Retrieve all loggers.
			var logTargetConfiguration = loggingConfiguration.GetAllLogTargetConfigurations();

			// Add each in turn to the list.
			foreach (var config in logTargetConfiguration.OrderByDescending(t => t.Enabled).ThenBy(t => t.Name))
			{
				// Build up the row.
				var row = table.AddRow();
				if(false == config.Enabled)
				{
					// Not enabled.
					row.Styles.Add("text-decoration", "line-through");
				}

				// Sort out the name.
				{
					var name = new DashboardCustomContentEx($"{config.Name} ({config.TypeName})");

					if(config.Enabled == false)
					{
						name.Icon = "Resources/Images/notenabled.png";
						row.Attributes.Add("title", Resources.Dashboard.Logging_TargetNotEnabled);
					}
					// Validation can take some time to run; let's not incur that cost.
					//else if(config.GetValidationFindings().Any(f => f.Type == ValidationFindingType.Error || f.Type == ValidationFindingType.Exception))
					//{
					//	name.Icon = "Resources/Images/error.png";
					//	row.Attributes.Add("title", Resources.Dashboard.Logging_TargetValidationErrors);
					//}
					else
					{
						name.Icon = "Resources/Images/enabled.png";
					}

					row.AddCell(name);
				}
				row.AddCell($"{config.MinimumLogLevel.ToDisplayString()} to {config.MaximumLogLevel.ToDisplayString()}");
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
	}
}
