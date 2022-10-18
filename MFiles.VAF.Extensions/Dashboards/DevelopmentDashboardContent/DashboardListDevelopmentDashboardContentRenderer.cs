using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFiles.VaultApplications.Logging.Configuration;
using System.Collections.Generic;
using System.Linq;
using MFiles.VaultApplications.Logging.NLog.ExtensionMethods;
using System.Reflection;
using System;
using MFiles.VaultApplications.Logging;

namespace MFiles.VAF.Extensions.Dashboards.DevelopmentDashboardContent
{
#if DEBUG
	public class DashboardListDevelopmentDashboardContentRenderer
		: IDevelopmentDashboardContentRenderer
	{
		private ILogger Logger { get; } = LogManager.GetLogger<DashboardListDevelopmentDashboardContentRenderer>();

		public void PopulateReferencedAssemblies<TConfigurationType>()
		{
			this.Logger?.Trace($"Starting to load referenced assemblies");
			using (var context = this.Logger?.BeginLoggingContext($"Loading referenced assemblies"))
			{
				try
				{
					var rootAssembly = typeof(TConfigurationType)?.Assembly
						?? this.GetType().Assembly;
					this.Logger?.Info($"Loading assemblies referenced by {rootAssembly.FullName}");
					this.ReferencedAssemblies.Clear();
					this.ReferencedAssemblies.AddRange
					(
						this.GetReferencedAssemblies(rootAssembly.GetName())?
						.Distinct()?
						.OrderBy(a => a.GetName().Name)
					);
					this.Logger?.Info($"{(this.ReferencedAssemblies?.Count ?? 0)} assemblies loaded.");
					foreach (var a in this.ReferencedAssemblies ?? Enumerable.Empty<Assembly>())
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
		protected IEnumerable<System.Reflection.Assembly> GetReferencedAssemblies(System.Reflection.AssemblyName assemblyName)
		{
			// Try and load the assembly.
			System.Reflection.Assembly assembly;
			try { assembly = System.Reflection.Assembly.Load(assemblyName); }
			catch { assembly = null; }
			if (null != assembly)
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
		protected List<System.Reflection.Assembly> ReferencedAssemblies { get; } = new List<Assembly>();
		public virtual DashboardPanelEx GetDashboardContent()
		{
			// Our data will go in a list.
			var list = new DashboardList();

			// Add the assemblies.
			if (null != this.ReferencedAssemblies)
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
						if (false == string.IsNullOrWhiteSpace(info.CompanyName))
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
				foreach (var assembly in ReferencedAssemblies)
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

		IDashboardContent IDevelopmentDashboardContentRenderer.GetDashboardContent()
			=> this.GetDashboardContent();
	}
#endif
}
