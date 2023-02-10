using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// A rule to move configuration from the location used in VAF 1.0 to the location used in VAF 2.3 and higher.
	/// </summary>
	/// <remarks>
	/// In VAF 1.0 <see cref="MFiles.VAF.Common.MFConfigurationAttribute"/> was used to define the location of the configuration.
	/// In VAF 2.3, the location of the configuration is based upon the vault application class namespace.
	/// </remarks>
	public class VAF10ToVAF23UpgradeRule
		: MoveConfigurationUpgradeRule
	{
		/// <summary>
		/// The named value name (key) used by all VAF 2.3 applications.
		/// </summary>
		public const string TargetNamedValueName = "configuration";

		public VAF10ToVAF23UpgradeRule(VaultApplicationBase vaultApplication, string @namespace, string name, Version migrateFromVersion, Version migrateToVersion)
			: base
			(
				new SingleNamedValueItem
				(
					MFNamedValueType.MFConfigurationValue,
					@namespace,
					name
				),
				SingleNamedValueItem.ForLatestVAFVersion(vaultApplication),
				migrateFromVersion,
				migrateToVersion
			)
		{
		}

	}

	public class VAF10ToVAF23UpgradeAttribute
		: ConfigurationLocationUpgradeAttribute
	{
		public VAF10ToVAF23UpgradeAttribute(string @namespace, string @name)
		{
			this.NamedValueType = MFNamedValueType.MFConfigurationValue;
			this.Namespace = @namespace;
			this.Name = @name;
		}

		public override IUpgradeRule AsUpgradeRule(VaultApplicationBase vaultApplication)
		{
			return new VAF10ToVAF23UpgradeRule(vaultApplication, this.Namespace, this.Name, new Version("0.0"), new Version("0.0"));
		}
	}
}
