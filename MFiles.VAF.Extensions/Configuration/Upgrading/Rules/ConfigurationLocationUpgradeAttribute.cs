using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public abstract class ConfigurationLocationUpgradeAttribute
		: Attribute
	{
		public MFNamedValueType NamedValueType { get; set; }
		public string Namespace { get; set; }
		public string Name { get; set; }
		public bool RunBeforeOtherUpgradeRules { get; set; } = true;

		public abstract IUpgradeRule AsUpgradeRule(VaultApplicationBase vaultApplication);
	}
}
