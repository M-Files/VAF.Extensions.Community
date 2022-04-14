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

		/// <summary>
		/// Creates a rule to move the configuration for <paramref name="vaultApplication"/>
		/// from the location used in VAF 2.0 to the location used in VAF 2.3.
		/// In VAF 2.0 the name of the node in the configuration tree was used as the named value name (key),
		/// so must be provided in <paramref name="configurationNodeName"/>.
		/// </summary>
		/// <param name="vaultApplication">The vault application that this is running within.</param>
		/// <param name="configurationNodeName">The name of the (previous) configuration node.  Used to locate data to read only.</param>
		public VAF20ToVAF23UpgradeRule(VaultApplicationBase vaultApplication, string configurationNodeName)
			: base(new UpgradeRuleOptions()
			{
				RemoveMovedValues = true,
				Source = new SingleNamedValueItem
				(
					MFNamedValueType.MFConfigurationValue, 
					SourceNamespaceLocation,
					configurationNodeName
				),
				Target = new SingleNamedValueItem
				(
					MFNamedValueType.MFSystemAdminConfiguration,
					vaultApplication.GetType().FullName,
					TargetNamedValueName
				)
			})
		{

		}
	}
}
