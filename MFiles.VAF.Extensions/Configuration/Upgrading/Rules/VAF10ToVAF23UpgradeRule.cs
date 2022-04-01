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

		/// <summary>
		/// Creates a rule to move the configuration for <paramref name="vaultApplication"/>
		/// from the location used in VAF 1.0 to the location used in VAF 2.3.
		/// In VAF 1.0 the namespace and name were defined by the application itself, so must be provided
		/// in <paramref name="namespace"/> and <paramref name="name"/> respectively.
		/// </summary>
		/// <param name="vaultApplication">The vault application that this is running within.</param>
		/// <param name="namespace">The previously-used namespace for the configuration data.  Used to locate data to read only.</param>
		/// <param name="name">The previously-used named value name (key) for the configuration data.  Used to locate data to read only.</param>
		public VAF10ToVAF23UpgradeRule(VaultApplicationBase vaultApplication, string @namespace, string name)
			: base(new MoveConfigurationUpgradeRuleOptions()
			{
				RemoveMovedValues = true,
				Source = new SingleNamedValueItem
				(
					MFNamedValueType.MFConfigurationValue,
					@namespace,
					name
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
