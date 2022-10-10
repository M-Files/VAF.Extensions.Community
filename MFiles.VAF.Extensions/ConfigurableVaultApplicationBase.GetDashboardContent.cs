using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VaultApplications.Logging.NLog.ExtensionMethods;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFilesAPI;
using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Dashboards.AsynchronousContent;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		/// <summary>
		/// The default dashboard refresh interval (in seconds).
		/// </summary>
		public const int DefaultDashboardRefreshInterval = 30;

		/// <summary>
		/// The interval (in seconds) to refresh the dashboard.  If null or <=0, no refresh will be done.
		/// </summary>
		public int? DashboardRefreshInterval = DefaultDashboardRefreshInterval;

		/// <summary>
		/// Creates the status dashboard object that will be populated by <see cref="GetDashboardContent(IConfigurationRequestContext)"/>.
		/// </summary>
		/// <returns>The status dashboard into which the dashboard content will be populated.</returns>
		public virtual StatusDashboard CreateStatusDashboard()
		{
			return new StatusDashboard()
			{
				RefreshInterval = this.DashboardRefreshInterval ?? 0
			};
		}

		/// <summary>
		/// Gets the content showing the application name, version, etc.
		/// </summary>
		/// <param name="context">The request context for this dashboard generation.</param>
		/// <returns>The dashboard content, or null if none should be rendered.</returns>
		public virtual IDashboardContent GetApplicationOverviewDashboardContent(IConfigurationRequestContext context)
		{
			// If there's some base content then add that.
			var baseContent = base.GetDashboardContent(context);
			return false == string.IsNullOrWhiteSpace(baseContent)
				? new DashboardCustomContent(baseContent)
				: null; // No content so return null.
		}

		/// <summary>
		/// Gets the root items for the dashboard.
		/// </summary>
		/// <param name="context">The request context for this dashboard generation.</param>
		/// <returns>The dashboard content.</returns>
		public virtual IEnumerable<IDashboardContent> GetStatusDashboardRootItems(IConfigurationRequestContext context)
		{
			// Application overview?
			yield return this.GetApplicationOverviewDashboardContent(context);

			// Do we have any asynchronous operation content?
			yield return this.GetAsynchronousOperationDashboardContent(context);

			// Do we have any logging content?
			yield return this.GetLoggingDashboardContent(context);

#if DEBUG
			// Output any data that may be useful for development of the extensions library.
			yield return this.GetDevelopmentDashboardData(context);
#endif

		}

		/// <inheritdoc />
		/// <remarks>
		/// Calls <see cref="CreateStatusDashboard()"/> to create the dashboard,
		/// then <see cref="GetStatusDashboardRootItems(IConfigurationRequestContext)"/> to get the items to display,
		/// then returns <see cref="StatusDashboard.ToString()"/>.
		/// </remarks>
		public override string GetDashboardContent(IConfigurationRequestContext context)
		{
			// Create a new dashboard.
			var dashboard = this.CreateStatusDashboard();
			if (null == dashboard)
				return "";

			// Add all content in turn.
			foreach (var content in this.GetStatusDashboardRootItems(context) ?? Enumerable.Empty<IDashboardContent>())
				if(null != content)
					dashboard.AddContent(content);

			// Return the dashboard.
			return dashboard.ToString();
		}

#if DEBUG
		protected void PopulateReferencedAssemblies()
		{
			this.Logger?.Trace($"Starting to load referenced assemblies");
			using (var context = this.Logger?.BeginLoggingContext($"Loading referenced assemblies"))
			{
				try
				{
					var rootAssembly = this.Configuration?.GetType()?.Assembly
						?? this.GetType().Assembly;
					this.Logger?.Info($"Loading assemblies referenced by {rootAssembly.FullName}");
					this.referencedAssemblies =
						this.GetReferencedAssemblies(rootAssembly.GetName())?
						.Distinct()?
						.OrderBy(a => a.GetName().Name)?
						.ToList();
					this.Logger?.Info($"{(this.referencedAssemblies?.Count ?? 0)} assemblies loaded.");
					foreach (var a in this.referencedAssemblies ?? Enumerable.Empty<Assembly>())
					{
						this.Logger?.Trace($"Assembly {a.FullName} is referenced from {a.Location}");
					}
				}
				catch (Exception e)
				{
					this.Logger?.Error(e, $"Exception loading referenced assemblies");
				}
			}
		}
		private List<System.Reflection.Assembly> referencedAssemblies = null;
		protected IEnumerable<System.Reflection.Assembly> GetReferencedAssemblies(System.Reflection.AssemblyName assemblyName)
		{
			// Try and load the assembly.
			System.Reflection.Assembly assembly;
			try { assembly = System.Reflection.Assembly.Load(assemblyName); }
			catch { assembly = null;}
			if(null != assembly)
				yield return assembly;

			// If we loaded it from somewhere other than the GAC then return its referenced items.
			if (null == assembly)
				yield break;
			if (false != assembly.GlobalAssemblyCache)
				yield break;
			foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
			foreach (var x in this.GetReferencedAssemblies(referencedAssembly))
				yield return x;
		}
		public virtual IDashboardContent GetDevelopmentDashboardData(IConfigurationRequestContext context)
		{
			// Our data will go in a list.
			var list = new DashboardList();

			// Add the assemblies.
			if(null != this.referencedAssemblies)
			{

				// Create the table to populate with assembly data.
				var table = new DashboardTable();
				{
					var header = table.AddRow(DashboardTableRowType.Header);
					header.AddCells
					(
						new DashboardCustomContent("Company"),
						new DashboardCustomContent("Assembly"),
						new DashboardCustomContent("Version"),
						new DashboardCustomContent("Location")
					);
				}
				table.MaximumHeight = null; // Allow any height.

				Func<System.Reflection.Assembly, string> getCompanyName = (assembly) =>
				{
					try
					{
						var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
						if(false == string.IsNullOrWhiteSpace(info.CompanyName))
							return info.CompanyName;
						var attributes = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), false);
						if (attributes.Length > 0 && attributes[0] is System.Reflection.AssemblyCompanyAttribute companyAttribute)
							return companyAttribute.Company;
					}
					catch
					{
					}
					return "";
				};

				// Render the table.
				foreach (var assembly in referencedAssemblies)
				{
					var row = table.AddRow();
					row.AddCells
					(
						new DashboardCustomContent(getCompanyName(assembly)),
						new DashboardCustomContent(assembly.GetName().Name),
						new DashboardCustomContent(assembly.GetName().Version.ToString()),
						new DashboardCustomContent(assembly.Location)
					);
					// Don't wrap the company name or assembly location.
					row.Cells[0].Styles.AddOrUpdate("white-space", "nowrap");
					row.Cells[3].Styles.AddOrUpdate("white-space", "nowrap");
				}

				// Add it to the list.
				list.Items.Add(new DashboardListItemEx()
				{
					Title = "Referenced Assemblies",
					InnerContent = table
				});

			}

			// Return a panel with the table in it.
			return new DashboardPanelEx()
			{
				Title = "Development Data",
				InnerContent = new DashboardContentCollection
				{
					list
				}
			};
		}
#endif

		/// <summary>
		/// Returns the implementation for rendering asynchronous dashboard content.
		/// By default this is an instance of <see cref="DashboardListAsynchronousDashboardContentRenderer"/>.
		/// </summary>
		/// <returns>The renderer, or null if nothing should be rendered.</returns>
		protected virtual IAsynchronousDashboardContentRenderer GetAsynchronousDashboardContentRenderer()
		{
			// Get the default renderer, which renders as a list.
			return new DashboardListAsynchronousDashboardContentRenderer();
		}

		/// <summary>
		/// Returns all providers of asynchronous content.
		/// By default this returns an instance of <see cref="TaskQueueBackgroundOperationManagerAsynchronousDashboardContentProvider{TConfiguration}"/>
		/// and an instance of <see cref="TaskManagerExAsynchronousDashboardContentProvider{TConfiguration}"/>.
		/// </summary>
		/// <returns>The providers, or an empty collection if nothing should be rendered.</returns>
		protected virtual IEnumerable<IAsynchronousDashboardContentProvider> GetAsynchronousDashboardContentProviders()
		{
			// Return the data from task queue background operation manager.
			yield return new TaskQueueBackgroundOperationManagerAsynchronousDashboardContentProvider<TSecureConfiguration>
			(
				this
			);

			// Return the data from the task manager.
			yield return new TaskManagerExAsynchronousDashboardContentProvider<TSecureConfiguration>
			(
				this.PermanentVault,
				this.TaskManager,
				this.TaskQueueResolver,
				this.RecurringOperationConfigurationManager
			);
		}

		/// <summary>
		/// Returns the dashboard content showing asynchronous operation status.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no background operation managers, background operations or task processors.</returns>
		public virtual IDashboardContent GetAsynchronousOperationDashboardContent(IConfigurationRequestContext context)
		{
			// Get our renderer.
			var renderer = this.GetAsynchronousDashboardContentRenderer();

			// Render the data from all our providers.
			return renderer?.GetDashboardContent(this.GetAsynchronousDashboardContentProviders());
		}

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

		/// <summary>
		/// Returns the dashboard content showing logging status.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no logging data is available or configured.</returns>
		public virtual IDashboardContent GetLoggingDashboardContent(IConfigurationRequestContext context)
		{
			// If we don't have any logging configuration then return null.
			var loggingConfiguration = this.GetLoggingConfiguration();
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
				if(false == config.Enabled)
				{
					// Not enabled.
					//row.Styles.Add("text-decoration", "line-through");
					row.Attributes.Add("title", Resources.Dashboard.Logging_TargetNotEnabled);
					row.Styles.AddOrUpdate("color", Resources.Dashboard.Logging_ColorNotEnabled);
				}

				// Sort out the name.
				{
					var name = new DashboardCustomContentEx($"{(string.IsNullOrWhiteSpace(config.Name) ? Resources.Dashboard.Logging_Table_UnnamedTarget : config.Name)} ({config.TypeName})");

					if(config.Enabled == false)
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
				row.AddCell($"{config.MinimumLogLevel.ToDisplayString()} to {config.MaximumLogLevel.ToDisplayString()}");

				// If it's the default one then allow downloads.
				if(config is VaultApplications.Logging.NLog.Targets.DefaultTargetConfiguration)
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
	}
}
