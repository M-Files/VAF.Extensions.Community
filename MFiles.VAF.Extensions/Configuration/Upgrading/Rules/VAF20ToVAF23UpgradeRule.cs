using MFilesAPI;

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

		public VAF20ToVAF23UpgradeRule(VaultApplicationBase vaultApplication, string configurationNodeName)
			: base
			(
				new SingleNamedValueItem
				(
					MFNamedValueType.MFConfigurationValue,
					SourceNamespaceLocation,
					configurationNodeName
				),
				SingleNamedValueItem.ForLatestVAFVersion(vaultApplication)
			)
		{
		}
	}
}
