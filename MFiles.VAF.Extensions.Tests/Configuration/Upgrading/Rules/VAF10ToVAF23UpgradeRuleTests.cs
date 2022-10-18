using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	[TestClass]
	public class VAF10ToVAF23UpgradeRuleTests
	{
		[TestMethod]
		public void EnsureBaseClass()
		{
			Assert.IsTrue(typeof(MoveConfigurationUpgradeRule).IsAssignableFrom(typeof(VAF10ToVAF23UpgradeRule)));
		}

		[TestMethod]
		public void Options_SourceValid()
		{
			var vaultApplication = new VaultApplicationProxy();
			var sourceNamespace = "MySourceNamespace";
			var keyName = "config";

			var instance = new VAF10ToVAF23UpgradeRule(vaultApplication, sourceNamespace, keyName, new Version("0.0"), new Version("0.0"));
			var source = instance.ReadFrom as SingleNamedValueItem;
			Assert.IsNotNull(source, "The source is not a single named value item.");
			Assert.AreEqual(MFNamedValueType.MFConfigurationValue, source.NamedValueType);
			Assert.AreEqual(sourceNamespace, source.Namespace);
			Assert.AreEqual(keyName, source.Name);
		}

		[TestMethod]
		public void Options_TargetValid()
		{
			var vaultApplication = new VaultApplicationProxy();
			var sourceNamespace = "MySourceNamespace";
			var keyName = "Hello World";

			var instance = new VAF10ToVAF23UpgradeRule(vaultApplication, sourceNamespace, keyName, new Version("0.0"), new Version("0.0"));
			var source = instance.WriteTo as SingleNamedValueItem;
			Assert.IsNotNull(source, "The target is not a single named value item.");
			Assert.AreEqual(MFNamedValueType.MFSystemAdminConfiguration, source.NamedValueType);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules.VAF10ToVAF23UpgradeRuleTests+VaultApplicationProxy", source.Namespace);
			Assert.AreEqual(VAF10ToVAF23UpgradeRule.TargetNamedValueName, source.Name);

		}
		public class VaultApplicationProxy 
			: VaultApplicationBase
		{
		}
	}
}
