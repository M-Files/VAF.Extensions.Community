using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Extensions.Dashboards.Commands;
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

			// Return any commands that the resolver provides.
			{
				var resolver = this.GetCustomDomainCommandResolver();
				if (resolver != null)
				{
					foreach (var c in resolver.GetCustomDomainCommands()?.AsNotNull())
						yield return c;
				}
			}
		}

		/// <summary>
		/// Returns an object - or <see langword="null"/> - that searches known object types
		/// to find methods decorated with <see cref="CustomCommandAttribute"/>.
		/// </summary>
		/// <returns>The resolver, or <see langword="null"/> if none is configured.</returns>
		/// <remarks>Returns <see cref="DefaultCustomDomainCommandResolver"/> by default.</remarks>
		public virtual ICustomDomainCommandResolver GetCustomDomainCommandResolver()
		{
			return new DefaultCustomDomainCommandResolver<TSecureConfiguration>(this);
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
		public virtual ImportReplicationPackageDashboardCommand<TSecureConfiguration> CreateImportReplicationPackageDomainCommand
		(
			string commandId,
			string displayName,
			string replicationPackagePath
		)
		{
			return new ImportReplicationPackageDashboardCommand<TSecureConfiguration>
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

		/// <summary>
		/// Creates a command that, when executed, previews a replication package import.
		/// </summary>
		/// <param name="commandId">The id of the command. Must be unique in the application.</param>
		/// <param name="displayName">The display text for the command.</param>
		/// <param name="replicationPackagePath">
		/// The path to the replication package.  
		/// Should be a .zip file included in the vault application's .mfappx.
		/// </param>
		/// <returns>The command.</returns>
		public virtual PreviewReplicationPackageDashboardCommand<TSecureConfiguration> CreatePreviewReplicationPackageDomainCommand
		(
			string commandId,
			string displayName,
			string replicationPackagePath,
			ImportReplicationPackageDashboardCommand<TSecureConfiguration> importCommand = null
		)
		{
			return new PreviewReplicationPackageDashboardCommand<TSecureConfiguration>
			(
				this,
				commandId,
				displayName,
				replicationPackagePath,
				importCommand
			)
			{
				Blocking = true
			};
		}
	}
}
