using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	[TestClass]
	public class VAF20ToVAF23UpgradeRuleTests
	{
		[TestMethod]
		public void EnsureBaseClass()
		{
			Assert.IsTrue(typeof(MoveConfigurationUpgradeRule).IsAssignableFrom(typeof(VAF20ToVAF23UpgradeRule)));
		}

		[TestMethod]
		public void Options_SourceValid()
		{
			var vaultApplication = new VaultApplicationProxy();
			var configurationNodeName = "Hello World";

			var instance = new VAF20ToVAF23UpgradeRule(vaultApplication, configurationNodeName);
			var source = instance.Options.Source as SingleNamedValueItem;
			Assert.IsNotNull(source, "The source is not a single named value item.");
			Assert.AreEqual(MFNamedValueType.MFConfigurationValue, source.NamedValueType);
			Assert.AreEqual(VAF20ToVAF23UpgradeRule.SourceNamespaceLocation, source.Namespace);
			Assert.AreEqual(configurationNodeName, source.Name);
		}

		[TestMethod]
		public void Options_TargetValid()
		{
			var vaultApplication = new VaultApplicationProxy();
			var configurationNodeName = "Hello World";

			var instance = new VAF20ToVAF23UpgradeRule(vaultApplication, configurationNodeName);
			var source = instance.Options.Target as SingleNamedValueItem;
			Assert.IsNotNull(source, "The target is not a single named value item.");
			Assert.AreEqual(MFNamedValueType.MFSystemAdminConfiguration, source.NamedValueType);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules.VAF20ToVAF23UpgradeRuleTests+VaultApplicationProxy", source.Namespace);
			Assert.AreEqual(VAF20ToVAF23UpgradeRule.TargetNamedValueName, source.Name);

		}
		public class VaultApplicationProxy
			: VaultApplicationBase
		{
		}
	}
}
