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
using static MFiles.VAF.Extensions.Tests.Configuration.Upgrading.ConfigurationUpgradeManager.UpgradeOnNewest;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	public partial class ConfigurationUpgradeManager
	{
		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnNewest_NoUpgradePathsDefined()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(false, c.TryGetDeclaredConfigurationUpgrades<VersionZero>(out var configurationVersion, out var rules));
			Assert.AreEqual("0.0", configurationVersion?.ToString());
			Assert.AreEqual(0, rules.Count());
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnNewest_ZeroToOneUpgradeWithInstanceUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades<VersionOneWithInstanceUpgradePath>(out var configurationVersion, out var rules));
			Assert.AreEqual("1.0", configurationVersion?.ToString());
			Assert.AreEqual(1, rules.Count());
			Assert.AreEqual(typeof(VersionZero), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(0).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnNewest_ZeroToOneUpgradeWithInstanceUpgradePath_WithoutConfigurationAttribute()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades<VersionOneWithoutConfigurationAttributeInstanceUpgradePath>(out var configurationVersion, out var rules));
			Assert.AreEqual("1.0", configurationVersion?.ToString());
			Assert.AreEqual(1, rules.Count());
			Assert.AreEqual(typeof(VersionZeroWithoutConfigurationAttribute), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual("0.0", rules.ElementAt(0).MigrateFromVersion?.ToString());
			Assert.AreEqual(typeof(VersionOneWithoutConfigurationAttributeInstanceUpgradePath), rules.ElementAt(0).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnNewest_ZeroToOneWithStaticUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades<VersionOneWithStaticUpgradePath>(out var configurationVersion, out var rules));
			Assert.AreEqual("1.0", configurationVersion?.ToString());
			Assert.AreEqual(1, rules.Count());
			Assert.AreEqual(typeof(VersionZero), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithStaticUpgradePath), rules.ElementAt(0).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnNewest_ZeroToTwoUpgradeWithInstanceUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades<VersionTwoWithInstanceUpgradePath>(out var configurationVersion, out var rules));
			Assert.AreEqual("2.0", configurationVersion?.ToString());
			Assert.AreEqual(2, rules.Count());
			Assert.AreEqual(typeof(VersionZero), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(0).UpgradeToType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), rules.ElementAt(1).UpgradeFromType);
			Assert.AreEqual(typeof(VersionTwoWithInstanceUpgradePath), rules.ElementAt(1).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnNewest_ZeroToThreeUpgradeWithInstanceUpgradePath()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades<VersionThreeWithInstanceUpgradePath>(out var configurationVersion, out var rules));
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
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnNewest_CyclicUpgradeRule()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(false, c.TryGetDeclaredConfigurationUpgrades<CyclicUpgradeRule2>(out var configurationVersion, out var rules));
		}

		public class UpgradeOnNewest
		{

			[DataContract]
			public class VersionZeroWithoutConfigurationAttribute
			{
				[DataMember]
				public string Hello { get; set; }
			}

			[DataContract]
			[Extensions.Configuration.ConfigurationVersion("1.0")]
			public class VersionOneWithoutConfigurationAttributeInstanceUpgradePath
				: VAF.Extensions.Configuration.VersionedConfigurationBase
			{
				[DataMember]
				public string World { get; set; }

				[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
				public virtual void Upgrade(VersionZeroWithoutConfigurationAttribute input)
				{
					World = input?.Hello;
				}
			}

			// This represents an old VAF 1.0-style configuration.
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
				: VersionZeroWithoutConfigurationAttribute
			{
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
}
