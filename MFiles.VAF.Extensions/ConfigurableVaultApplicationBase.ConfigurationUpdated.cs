using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
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
		protected override void OnConfigurationUpdated(TSecureConfiguration oldConfiguration, bool isValid, bool updateExternals)
		{
			// Base implementation is empty, but good practice to call it.
			base.OnConfigurationUpdated(oldConfiguration, isValid, updateExternals);

			// Populate the task processing schedule configuration.
			this.RecurringOperationConfigurationManager?.PopulateFromConfiguration(isVaultStartup: false);

			// If we have logging configuration then set it up.
			if (this.Configuration is Configuration.IConfigurationWithLoggingConfiguration configurationWithLogging)
			{
				this.Logger?.Debug("Logging configuration updating");
				LogManager.UpdateConfiguration(configurationWithLogging?.GetLoggingConfiguration());
				this.Logger?.Debug("Logging configuration updated");
			}
		}

		/// <summary>
		/// Gets any rules that should be applied to the configuration.
		/// These rules may define a change in configuration location (e.g. to migrate older configuration to a newer location),
		/// or define how to map older configuration structures across to new.
		/// Rules will be executed in the order that they appear in the list.
		/// Rules are run when the configuration is loaded.
		/// </summary>
		/// <returns>
		/// The rules to run.
		/// </returns>
		protected virtual IEnumerable<Configuration.Upgrading.Rules.IUpgradeRule> GetConfigurationUpgradeRules()
		{
			yield break;
		}

		/// <inheritdoc />
		/// <remarks>Will run any <see cref="GetConfigurationUpgradeRules"/> at this point.</remarks>
		protected override void PopulateConfigurationObjects(Vault vault)
		{
			// Run any configuration upgrade rules.
			var rules = this.GetConfigurationUpgradeRules();
			if (null != rules)
				foreach (var rule in rules)
					rule?.Execute(vault);

			// Use the base implementation.
			base.PopulateConfigurationObjects(vault);
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
				this.Logger?.Fatal(e, "Exception mapping configuration to recurring operations.");
			}


		}
	}
}
