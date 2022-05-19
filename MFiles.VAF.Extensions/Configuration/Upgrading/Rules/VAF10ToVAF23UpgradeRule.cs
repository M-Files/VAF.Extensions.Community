using MFilesAPI;

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

		public VAF10ToVAF23UpgradeRule(VaultApplicationBase vaultApplication, string @namespace, string name)
			: base
			(
				new SingleNamedValueItem
				(
					MFNamedValueType.MFConfigurationValue,
					@namespace,
					name
				),
				SingleNamedValueItem.ForLatestVAFVersion(vaultApplication)
			)
		{
		}

	}
}
