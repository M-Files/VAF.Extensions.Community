using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Resources;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Extensions.Directives;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A base class for vault applications that use the VAF Extensions library.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	/// <remarks>See https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes for further details.</remarks>
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// Whether the current configuration is valid.
		/// </summary>
		private bool? isCurrentConfigurationValid;

		/// <summary>
		/// Returns whether the configuration is valid.
		/// Note: uses a cached value that is updated during
		/// <see cref="StartOperations(Vault)"/> and <see cref="OnConfigurationUpdated(Configuration, bool, bool)"/>.
		/// Only re-validates if the status is unknown, or if <paramref name="force"/> is <see langword="true"/>.
		/// Validation is done by calling <see cref="Core.ConfigurableVaultApplicationBase{TSecureConfiguration}.IsValid"/>.
		/// </summary>
		/// <param name="vault">The vault reference to use to validate, if the status is unknown or <paramref name="force"/> is <see langword="true"/>.</param>
		/// <param name="force">If <see langword="true"/> then the cached value is updated.  This incurs a performance hit and should not be used often.</param>
		/// <returns><see langword="true"/> if the configuration is valid, <see langword="false"/> otherwise.</returns>
		protected bool IsCurrentConfigurationValid(Vault vault, bool force = false)
		{
			this.isCurrentConfigurationValid =
				this.isCurrentConfigurationValid.HasValue && !force
				? this.isCurrentConfigurationValid.Value // No-op.
				: this.IsValid(vault); // Use the IsValid method.
			return this.isCurrentConfigurationValid.Value;
		}

		/// <inheritdoc />
		protected override void OnConfigurationUpdated(TSecureConfiguration oldConfiguration, bool isValid, bool updateExternals)
		{
			// Base implementation is empty, but good practice to call it.
			base.OnConfigurationUpdated(oldConfiguration, isValid, updateExternals);

			// Populate the task processing schedule configuration.
			this.RecurringOperationConfigurationManager?.PopulateFromConfiguration(isVaultStartup: false);

			// Update the cached value with the validity status.
			this.isCurrentConfigurationValid = isValid;

			// If we have logging configuration then set it up.
			var loggingConfiguration = this.GetLoggingConfiguration();
			this.Logger?.Debug($"Logging configuration updating");

			// This can be called even when null, and we may need to clear the existing configuration
			LogManager.UpdateConfiguration(loggingConfiguration);
			this.Logger?.Debug($"Logging configuration updated");
		}

		internal new SecureConfigurationManager<TSecureConfiguration> ConfManager
		{
			get => base.ConfManager;
		}

		/// <inheritdoc />
		public override void StartOperations(Vault vaultPersistent)
		{
			// Do we have a valid configuration?
			this.isCurrentConfigurationValid = this.IsValid(vaultPersistent);

			// Initialize the application.
			base.StartOperations(vaultPersistent);

			// Ensure that our recurring configuration is updated.
			try
			{
				this.RecurringOperationConfigurationManager?.PopulateFromConfiguration(isVaultStartup: true);
			}
			catch(Exception e)
			{
				this.Logger?.Fatal(e, $"Exception mapping configuration to recurring operations.");
			}


		}

		/// <summary>
		/// Handles the task asking that the metadata structure cache is reinitialised.
		/// </summary>
		/// <param name="job">The directive for the task.</param>
		[BroadcastProcessor
		(
			ReinitializeMetadataCacheTaskDirective.TaskType,
			// ImportReplicationPackageDashboardCommand will update the server it runs on,
			// so we only need to process on other servers.
			FilterMode = BroadcastFilterMode.FromOtherServersOnly
		)]
		protected virtual void HandleReinitializeMetadataStructureCache(IBroadcastProcessingJob<ReinitializeMetadataCacheTaskDirective> job)
		{
			this.ReinitializeMetadataStructureCache(this.PermanentVault);
		}
	}
}
