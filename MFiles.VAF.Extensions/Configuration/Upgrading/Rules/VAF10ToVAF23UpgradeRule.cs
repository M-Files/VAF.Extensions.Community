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

	/// <summary>
	/// Defines that the system should attempt to migrate configuration in the associated old VAF 1.0 location
	/// across to the appropriate VAF 2.3 location.
	/// The configuration is literally moved with no other changes.
	/// </summary>
	public class VAF10ToVAF23UpgradeAttribute
		: MoveConfigurationLocationAttribute
	{
		public VAF10ToVAF23UpgradeAttribute(string @namespace, string @name)
			: base()
		{
			this.Source = new SingleNamedValueItem
				(
					MFNamedValueType.MFConfigurationValue,
					@namespace,
					name
				);
		}

		public override IUpgradeRule AsUpgradeRule(VaultApplicationBase vaultApplication)
		{
			return new VAF10ToVAF23UpgradeRule(vaultApplication, this.Source.Namespace, this.Source.Name, new Version("0.0"), new Version("0.0"));
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Migrating configuration from {this.Source} to latest VAF location.";
		}
	}
}
