using MFiles.VAF.Configuration;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public abstract class UpgradeConfigurationVersionUpgradeRule
		: UpgradeRuleBase<UpgradeConfigurationVersionUpgradeRule.UpgradeConfigurationVersionUpgradeRuleOptions>
	{
		protected UpgradeConfigurationVersionUpgradeRule(UpgradeConfigurationVersionUpgradeRuleOptions options)
			: base(options)
		{
		}

		/// <summary>
		/// Returns the configuration storage to use.
		/// </summary>
		/// <param name="namedValueType">The type of data to be read/written.</param>
		/// <returns>The configuration storage.</returns>
		protected virtual IConfigurationStorage GetConfigurationStorage(MFNamedValueType namedValueType)
			=> new ConfigurationStorageInVault
			(
				primaryLocation: namedValueType
			);

		/// <inheritdoc />
		public override bool Execute(Vault vault)
		{
			// Create a configuration storage to use.
			var configurationStorage = this.GetConfigurationStorage(this.Options.Source.NamedValueType);

			// Attempt to load the data from storage.
			if (false == configurationStorage.ReadConfigurationData(vault, this.Options.Source.Namespace, this.Options.Source.Name, out string oldData))
				return false; // Not there, so die.

			// Deserialize it.
			var oldObject = configurationStorage.Deserialize<VersionedConfiguration>(oldData);

			// Already at the correct version.
			if (oldObject.Version >= this.Options.Target)
				return false;

			// Convert it.
			var newObject = this.Convert(oldData);

			// Save the new data to storage.
			configurationStorage.Save(vault, newObject, this.Options.Source.Namespace, this.Options.Source.Name);

			return true;
		}

		/// <summary>
		/// Converts the older configuration to an instance of 
		/// </summary>
		/// <returns>The equivalent new configuration structure.</returns>
		public abstract string Convert(string input);

		/// <summary>
		/// Options for <see cref="UpgradeConfigurationVersionUpgradeRule"/>.
		/// </summary>
		public class UpgradeConfigurationVersionUpgradeRuleOptions
			: UpgradeRuleOptionsBase
		{
			/// <summary>
			/// A definition of where the values are stored.
			/// The value will be read, converted, then written to the same location.
			/// </summary>
			public SingleNamedValueItem Source { get; set; }

			/// <summary>
			/// The minimum version that this can upgrade from.
			/// Can be null to indicate it can handle all versions.
			/// </summary>
			public Version Minimum { get; set; }

			/// <summary>
			/// The version number that this will upgrade to.
			/// </summary>
			public Version Target { get; set; }

			/// <inheritdoc />
			public override bool IsValid()
			{
				return this.Target != null;
			}
		}
	}
}
