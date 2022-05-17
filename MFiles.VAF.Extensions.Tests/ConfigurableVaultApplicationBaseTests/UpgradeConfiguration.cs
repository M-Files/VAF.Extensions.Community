using MFiles.VAF.Core;
using MFiles.VAF;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Runtime.Serialization;
using MFiles.VAF.Configuration;
using System.Linq;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;

namespace MFiles.VAF.Extensions.Tests
{
	public partial class ConfigurableVaultApplicationBaseTests
		: TestBaseWithVaultMock
	{

		[TestMethod]
		public void UpgradeConfiguration_NoUpgradePathsDefined()
		{
			var vault = this.GetVaultMock().Object;
			var configurationStorage = this.GetConfigurationStorage();
			var c = new ConfigurableVaultApplicationBaseProxy<VersionZero>()
			{
				ConfigurationStorage = configurationStorage
			};

			c.UpgradeConfiguration(vault);

			Assert.AreEqual(null, configurationStorage.ReadConfigurationData(vault, "", ""));
		}

		[TestMethod]
		public void UpgradeConfiguration_ZeroToOneUpgradeWithInstanceUpgradePath()
		{
			var vault = this.GetVaultMock().Object;
			var source = new VersionZero() {  Hello = "World" };
			var configurationStorage = this.GetConfigurationStorage(source);
			var c = new ConfigurableVaultApplicationBaseProxy<VersionOneWithInstanceUpgradePath>()
			{
				ConfigurationStorage = configurationStorage
			};

			c.UpgradeConfiguration(vault);

			Assert.AreEqual("{\"World\":\"World\",\"Version\":\"1.0\"}", configurationStorage.ReadConfigurationData(vault, "", ""));
		}

	}
}
