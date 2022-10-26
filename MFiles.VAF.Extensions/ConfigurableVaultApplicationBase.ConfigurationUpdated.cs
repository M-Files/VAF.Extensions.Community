using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Resources;

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
		/// <inheritdoc />
		protected override void OnConfigurationUpdated(TSecureConfiguration oldConfiguration, bool isValid, bool updateExternals)
		{
			// Base implementation is empty, but good practice to call it.
			base.OnConfigurationUpdated(oldConfiguration, isValid, updateExternals);

			// Populate the task processing schedule configuration.
			this.RecurringOperationConfigurationManager?.PopulateFromConfiguration(isVaultStartup: false);

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
	}
}
