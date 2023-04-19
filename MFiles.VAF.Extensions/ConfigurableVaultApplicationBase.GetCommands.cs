using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		/// <inheritdoc />
		public override IEnumerable<CustomDomainCommand> GetCommands(IConfigurationRequestContext context)
		{
			// Return the base commands, if any.
			foreach (var c in base.GetCommands(context)?.AsNotNull())
				yield return c;

			// Return the command related to the background operation approach.
			foreach (var c in this.GetTaskQueueBackgroundOperationRunCommands()?.AsNotNull())
				yield return c;

			// Return the commands related to the VAF 2.3+ attribute-based approach.
			foreach (var c in this.TaskManager?.GetTaskQueueRunCommands(this.TaskQueueResolver)?.AsNotNull())
				yield return c;

			// Return the commands associated with downloading logs from the default file target.
			foreach (var c in this.GetDefaultLogTargetDownloadCommands()?.AsNotNull())
				yield return c;

			// Return the commands declared via attributes.
			foreach (var c in this.GetCustomDomainCommandResolver()?.GetCustomDomainCommands()?.AsNotNull())
				yield return c;
		}

		/// <summary>
		/// Returns an object - or <see langword="null"/> - that searches known object types
		/// to find methods decorated with <see cref="CustomCommandAttribute"/>.
		/// </summary>
		/// <returns>The resolver, or <see langword="null"/> if none is configured.</returns>
		/// <remarks>Returns <see cref="DefaultCustomDomainCommandResolver"/> by default.</remarks>
		public virtual ICustomDomainCommandResolver GetCustomDomainCommandResolver()
		{
			var resolver = new DefaultCustomDomainCommandResolver();
			resolver.Include(this);
			return resolver;
		}
		
		/// <summary>
		/// Returns the commands associated with downloading logs from the default file target.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<CustomDomainCommand> GetDefaultLogTargetDownloadCommands()
		{
			// One to allow them to select which logs...
			yield return Dashboards.Commands.ShowSelectLogDownloadDashboardCommand.Create();

			// ...and one that actually does the collation/download.
			yield return Dashboards.Commands.DownloadSelectedLogsDashboardCommand.Create();
			
			// Allow the user to see the latest log entries.
			yield return Dashboards.Commands.ShowLatestLogEntriesDashboardCommand.Create();
			yield return Dashboards.Commands.RetrieveLatestLogEntriesCommand.Create();
		}

		/// <summary>
		/// Returns the commands associated with manually running task queue background operations.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<CustomDomainCommand> GetTaskQueueBackgroundOperationRunCommands()
		{
			// Get the background operations that have a run command.
			// Note: this should be all of them.
			return this
				.GetType()
				.GetPropertiesAndFieldsOfType<TaskQueueBackgroundOperationManager<TSecureConfiguration>>(this)
				.SelectMany(tqbom => tqbom.BackgroundOperations)
				.AsEnumerable()
				.Select(bo => bo.Value?.DashboardRunCommand)
				.Where(c => null != c);
		}
	}
}
