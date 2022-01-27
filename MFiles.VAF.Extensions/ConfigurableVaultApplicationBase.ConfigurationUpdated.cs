using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VaultApplications.Logging;
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
		protected override void OnConfigurationUpdated(TSecureConfiguration oldConfiguration, bool updateExternals)
		{
			// Base implementation is empty, but good practice to call it.
			base.OnConfigurationUpdated(oldConfiguration, updateExternals);

			// Populate the task processing schedule configuration.
			this.RecurringOperationConfigurationManager?.PopulateFromConfiguration(isVaultStartup: false);

			// If we have logging configuration then set it up.
			if (this.Configuration is Configuration.IConfigurationWithLoggingConfiguration configurationWithLogging)
			{
				this.Logger?.Debug("Logging configuration updating");
				base.OnConfigurationUpdated(oldConfiguration, updateExternals);
				LogManager.UpdateConfiguration(configurationWithLogging?.GetLoggingConfiguration());
				this.Logger?.Debug("Logging configuration updated");
			}
		}

		/// <inheritdoc />
		protected override SecureConfigurationManager<TSecureConfiguration> GetConfigurationManager()
		{
			var configurationManager = base.GetConfigurationManager();

			// Set the resource manager for the configuration manager.
			var combinedResourceManager = new CombinedResourceManager(configurationManager.ResourceManager);

			// Set the resource manager for the configuration.
			configurationManager.ResourceManager = combinedResourceManager;
			return configurationManager;
		}

		protected void AddResourceManagerToConfiguration(ResourceManager resourceManager)
		{
			if (null == resourceManager)
				throw new ArgumentNullException(nameof(resourceManager));

			// Try and get the combined resource manager.
			var combinedResourceManager = this.ConfManager?.ResourceManager as CombinedResourceManager;
			if (null == combinedResourceManager)
				throw new InvalidOperationException(Resources.Exceptions.InternalOperations.ConfigurationManagerDoesNotSupportCombinedResources);

			combinedResourceManager.ResourceManagers.Add(resourceManager);
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
				SysUtils.ReportErrorMessageToEventLog("Exception mapping configuration to recurring operations.", e);
			}


		}
	}
}
