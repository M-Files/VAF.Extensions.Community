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

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	public partial class ConfigurationUpgradeManager
	{
		private object upgradeRules;

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_NoUpgradePathsDefined()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager<VersionZero>(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(false, c.TryGetDeclaredConfigurationUpgrades(out var configurationVersion, out var rules));
			Assert.AreEqual("0.0", configurationVersion?.ToString());
			Assert.AreEqual(0, rules.Count());
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_ZeroToOneUpgradeWithInstanceUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager<VersionOneWithInstanceUpgradePath>(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades(out var configurationVersion, out var rules));
			Assert.AreEqual("1.0", configurationVersion?.ToString());
			Assert.AreEqual(1, rules.Count());
			Assert.AreEqual(typeof(VersionZero), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(0).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_ZeroToOneWithStaticUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager<VersionOneWithStaticUpgradePath>(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades(out var configurationVersion, out var rules));
			Assert.AreEqual("1.0", configurationVersion?.ToString());
			Assert.AreEqual(1, rules.Count());
			Assert.AreEqual(typeof(VersionZero), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithStaticUpgradePath), rules.ElementAt(0).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_ZeroToTwoUpgradeWithInstanceUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager<VersionTwoWithInstanceUpgradePath>(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades(out var configurationVersion, out var rules));
			Assert.AreEqual("2.0", configurationVersion?.ToString());
			Assert.AreEqual(2, rules.Count());
			Assert.AreEqual(typeof(VersionZero), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(0).UpgradeToType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(1).UpgradeFromType);
			Assert.AreEqual(typeof(VersionTwoWithInstanceUpgradePath), rules.ElementAt(1).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_ZeroToThreeUpgradeWithInstanceUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager<VersionThreeWithInstanceUpgradePath>(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades(out var configurationVersion, out var rules));
			Assert.AreEqual("3.0", configurationVersion?.ToString());
			Assert.AreEqual(3, rules.Count());
			Assert.AreEqual(typeof(VersionZero), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(0).UpgradeToType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(1).UpgradeFromType);
			Assert.AreEqual(typeof(VersionTwoWithInstanceUpgradePath), rules.ElementAt(1).UpgradeToType);
			Assert.AreEqual(typeof(VersionTwoWithInstanceUpgradePath), rules.ElementAt(2).UpgradeFromType);
			Assert.AreEqual(typeof(VersionThreeWithInstanceUpgradePath), rules.ElementAt(2).UpgradeToType);
		}

		/// <summary>
		/// We have a rule that goes from 2->1 and also from 1->2.
		/// Only one should be returned.
		/// </summary>
		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_CyclicUpgradeRule()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager<CyclicUpgradeRule2>(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades(out var configurationVersion, out var rules));
			Assert.AreEqual("2.0", configurationVersion?.ToString());
			Assert.AreEqual(1, rules.Count());
			Assert.AreEqual(typeof(CyclicUpgradeRule1), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(CyclicUpgradeRule2), rules.ElementAt(0).UpgradeToType);
		}

		[DataContract]
		[Extensions.Configuration.ConfigurationVersion
		(
			"0.0",
			UsesCustomNVSLocation = true, 
			Namespace = "Castle.Proxies.VaultApplicationBaseProxy", 
			Key = "config",
			NamedValueType = MFNamedValueType.MFConfigurationValue
		)]
		public class VersionZero
		{
			[DataMember]
			public string Hello { get; set; }
		}

		[DataContract]
		[Extensions.Configuration.ConfigurationVersion("1.0")]
		public class VersionOneWithInstanceUpgradePath
			: VAF.Extensions.Configuration.VersionedConfigurationBase
		{
			[DataMember]
			public string World { get; set; }

			[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
			public virtual void Upgrade(VersionZero input)
			{
				World = input?.Hello;
			}
		}

		[DataContract]
		[Extensions.Configuration.ConfigurationVersion("1.0")]
		public class VersionOneWithStaticUpgradePath
			: VAF.Extensions.Configuration.VersionedConfigurationBase
		{
			[DataMember]
			public string World { get; set; }

			[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
			public static VersionOneWithStaticUpgradePath Upgrade(VersionZero input)
			{
				return new VersionOneWithStaticUpgradePath()
				{
					World = input?.Hello
				};
			}
		}

		[DataContract]
		[Extensions.Configuration.ConfigurationVersion("2.0")]
		public class VersionTwoWithInstanceUpgradePath
			: VAF.Extensions.Configuration.VersionedConfigurationBase
		{
			[DataMember]
			public string World { get; set; }

			[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
			public virtual void Upgrade(VersionOneWithInstanceUpgradePath input)
			{
			}
		}

		[DataContract]
		[Extensions.Configuration.ConfigurationVersion("3.0")]
		public class VersionThreeWithInstanceUpgradePath
			: VAF.Extensions.Configuration.VersionedConfigurationBase
		{
			[DataMember]
			public string World { get; set; }

			[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
			public virtual void Upgrade(VersionTwoWithInstanceUpgradePath input)
			{
			}
		}

		[DataContract]
		[Extensions.Configuration.ConfigurationVersion("1.0")]
		public class CyclicUpgradeRule1
			: VAF.Extensions.Configuration.VersionedConfigurationBase
		{
			[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
			public virtual void Upgrade(CyclicUpgradeRule2 input)
			{
			}
		}

		[DataContract]
		[Extensions.Configuration.ConfigurationVersion("2.0")]
		public class CyclicUpgradeRule2
			: VAF.Extensions.Configuration.VersionedConfigurationBase
		{
			[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
			public virtual void Upgrade(CyclicUpgradeRule1 input)
			{
			}
		}
	}
}
