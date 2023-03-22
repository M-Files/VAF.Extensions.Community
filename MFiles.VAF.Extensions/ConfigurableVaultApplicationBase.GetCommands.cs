// ReSharper disable once CheckNamespace
using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Interfaces.Domain;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		/// <inheritdoc />
		public override IEnumerable<CustomDomainCommand> GetCommands(IConfigurationRequestContext context)
		{
			// Return the base commands, if any.
			foreach (var c in base.GetCommands(context) ?? new CustomDomainCommand[0])
				yield return c;

			// Return the command related to the background operation approach.
			foreach (var c in this.GetTaskQueueBackgroundOperationRunCommands())
				yield return c;

			// Return the commands related to the VAF 2.3+ attribute-based approach.
			foreach (var c in this.TaskManager.GetTaskQueueRunCommands(this.TaskQueueResolver))
				yield return c;

			// Return the commands associated with downloading logs from the default file target.
			foreach (var c in this.GetDefaultLogTargetDownloadCommands())
				yield return c;
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

		/// <summary>
		/// Creates a command that, when executed, imports a replication package.
		/// </summary>
		/// <param name="commandId">The id of the command. Must be unique in the application.</param>
		/// <param name="displayName">The display text for the command.</param>
		/// <param name="replicationPackagePath">
		/// The path to the replication package.  
		/// Should be a .zip file included in the vault application's .mfappx.
		/// </param>
		/// <returns>The command.</returns>
		public virtual CustomDomainCommand CreateImportReplicationPackageDomainCommand
		(
			string commandId,
			string displayName,
			string replicationPackagePath
		)
		{
			// Synchronous import.
			return new Dashboards.Commands.ImportReplicationPackageDashboardCommand<TSecureConfiguration>
			(
				this,
				commandId,
				displayName,
				replicationPackagePath
			)
			{
				Blocking = true
			};
		}
	}
}
