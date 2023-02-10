using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// A rule to move configuration from the location used in VAF 2.0 to the location used in VAF 2.3.
	/// </summary>
	public class VAF20ToVAF23UpgradeRule
		: MoveConfigurationUpgradeRule
	{
		/// <summary>
		/// The namespace used for all VAF 2.0 configurations.
		/// </summary>
		public const string SourceNamespaceLocation = "M-Files.Configuration.SavedConfigurations";

		/// <summary>
		/// The named value name (key) used by all VAF 2.3 applications.
		/// </summary>
		public const string TargetNamedValueName = "configuration";

		public VAF20ToVAF23UpgradeRule(VaultApplicationBase vaultApplication, string configurationNodeName, Version migrateFromVersion, Version migrateToVersion)
			: base
			(
				new SingleNamedValueItem
				(
					MFNamedValueType.MFConfigurationValue,
					SourceNamespaceLocation,
					configurationNodeName
				),
				SingleNamedValueItem.ForLatestVAFVersion(vaultApplication),
				migrateFromVersion,
				migrateToVersion
			)
		{
		}
	}

	public class VAF20ToVAF23UpgradeAttribute
		: ConfigurationLocationUpgradeAttribute
	{
		public VAF20ToVAF23UpgradeAttribute(string @name)
		{
			this.NamedValueType = MFNamedValueType.MFConfigurationValue;
			this.Namespace = VAF20ToVAF23UpgradeRule.SourceNamespaceLocation;
			this.Name = @name;
		}

		public override IUpgradeRule AsUpgradeRule(VaultApplicationBase vaultApplication)
		{
			return new VAF20ToVAF23UpgradeRule(vaultApplication, this.Name, new Version("0.0"), new Version("0.0"));
		}
	}
}
