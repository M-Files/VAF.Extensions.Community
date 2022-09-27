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
using static MFiles.VAF.Extensions.Tests.Configuration.Upgrading.ConfigurationUpgradeManager.UpgradeOnOldest;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	public partial class ConfigurationUpgradeManager
	{
		/// <summary>
		/// Unlike on the "newest" set of tests, the upgrade manager will inherently find all
		/// the upgrade methods when defined on the older configuration classes.
		/// </summary>
		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnOldest_AllUpgradePathsIdentified()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(true, c.TryGetDeclaredConfigurationUpgrades<VersionThreeWithStaticUpgradePath>(out var configurationVersion, out var rules));
			Assert.AreEqual("3.0", configurationVersion?.ToString());
			Assert.AreEqual(3, rules.Count());
			Assert.AreEqual(typeof(VersionZeroWithoutConfigurationAttribute), rules.ElementAt(0).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithStaticUpgradePath), rules.ElementAt(0).UpgradeToType);
			Assert.AreEqual(typeof(VersionOneWithStaticUpgradePath), rules.ElementAt(1).UpgradeFromType);
			Assert.AreEqual(typeof(VersionTwoWithStaticUpgradePath), rules.ElementAt(1).UpgradeToType);
			Assert.AreEqual(typeof(VersionTwoWithStaticUpgradePath), rules.ElementAt(2).UpgradeFromType);
			Assert.AreEqual(typeof(VersionThreeWithStaticUpgradePath), rules.ElementAt(2).UpgradeToType);
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnOldest_CyclicUpgradeRule()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(false, c.TryGetDeclaredConfigurationUpgrades<CyclicUpgradeRule2>(out var configurationVersion, out var rules));
		}

		[TestMethod]
		public void TryGetDeclaredConfigurationUpgrades_UpgradeMethodOnOldest_CyclicUpgradeRule_Same()
		{
			var c = new VAF.Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>());
			Assert.AreEqual(false, c.TryGetDeclaredConfigurationUpgrades<CyclicUpgradeRule_Same>(out var configurationVersion, out var rules));
		}

		public class UpgradeOnOldest
		{

			[DataContract]
			public class VersionZeroWithoutConfigurationAttribute
			{
				[DataMember]
				public string Hello { get; set; }

				[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
				public static VersionOneWithStaticUpgradePath Upgrade(VersionZeroWithoutConfigurationAttribute input)
				{
					return new VersionOneWithStaticUpgradePath()
					{
						World = input?.Hello
					};
				}
			}

			[DataContract]
			[Extensions.Configuration.ConfigurationVersion("1.0", PreviousVersionType = typeof(VersionZeroWithoutConfigurationAttribute))]
			public class VersionOneWithStaticUpgradePath
				: VAF.Extensions.Configuration.VersionedConfigurationBase
			{
				[DataMember]
				public string World { get; set; }

				[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
				public static VersionTwoWithStaticUpgradePath Upgrade(VersionOneWithStaticUpgradePath input)
				{
					return new VersionTwoWithStaticUpgradePath()
					{
						World = input?.World
					};
				}
			}

			[DataContract]
			[Extensions.Configuration.ConfigurationVersion("2.0", PreviousVersionType = typeof(VersionOneWithStaticUpgradePath))]
			public class VersionTwoWithStaticUpgradePath
				: VAF.Extensions.Configuration.VersionedConfigurationBase
			{
				[DataMember]
				public string World { get; set; }

				[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
				public static VersionThreeWithStaticUpgradePath Upgrade(VersionTwoWithStaticUpgradePath input)
				{
					return new VersionThreeWithStaticUpgradePath()
					{
						World = input?.World
					};
				}
			}

			[DataContract]
			[Extensions.Configuration.ConfigurationVersion("3.0", PreviousVersionType = typeof(VersionTwoWithStaticUpgradePath))]
			public class VersionThreeWithStaticUpgradePath
				: VAF.Extensions.Configuration.VersionedConfigurationBase
			{
				[DataMember]
				public string World { get; set; }
			}

			[DataContract]
			[Extensions.Configuration.ConfigurationVersion("1.0", PreviousVersionType = typeof(CyclicUpgradeRule2))]
			public class CyclicUpgradeRule1
				: VAF.Extensions.Configuration.VersionedConfigurationBase
			{
				[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
				public static CyclicUpgradeRule2 Upgrade(CyclicUpgradeRule1 input)
				{
					return new CyclicUpgradeRule2();
				}
			}

			[DataContract]
			[Extensions.Configuration.ConfigurationVersion("2.0", PreviousVersionType = typeof(CyclicUpgradeRule1))]
			public class CyclicUpgradeRule2
				: VAF.Extensions.Configuration.VersionedConfigurationBase
			{
				[VAF.Extensions.Configuration.ConfigurationUpgradeMethod]
				public static CyclicUpgradeRule1 Upgrade(CyclicUpgradeRule2 input)
				{
					return new CyclicUpgradeRule1();
				}
			}

			[DataContract]
			[Extensions.Configuration.ConfigurationVersion("2.0", PreviousVersionType = typeof(CyclicUpgradeRule2))]
			public class CyclicUpgradeRule_Same
				: VAF.Extensions.Configuration.VersionedConfigurationBase
			{
			}

		}
	}
}
