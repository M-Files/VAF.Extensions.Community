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
		public void GetConfigurationUpgradeRules_NoUpgradePathsDefined()
		{
			var c = new ConfigurableVaultApplicationBaseProxy<VersionZero>()
			{
				ConfigurationStorage = this.GetConfigurationStorage()
			};
			Assert.AreEqual(0, c.GetConfigurationUpgradeRules(this.GetVaultMock().Object).Count());
		}

		[TestMethod]
		public void GetConfigurationUpgradeRules_ZeroToOneUpgradeWithInstanceUpgradePath()
		{
			var source = new VersionZero();
			var c = new ConfigurableVaultApplicationBaseProxy<VersionOneWithInstanceUpgradePath>()
			{
				ConfigurationStorage = this.GetConfigurationStorage(source)
			};
			var rules = c.GetConfigurationUpgradeRules(this.GetVaultMock().Object)?.ToList();
			Assert.AreEqual(1, rules.Count);
			Assert.AreEqual(typeof(VersionZero), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeToType);
		}

		[TestMethod]
		public void GetConfigurationUpgradeRules_ZeroToOneWithStaticUpgradePath()
		{
			var source = new VersionZero();
			var c = new ConfigurableVaultApplicationBaseProxy<VersionOneWithStaticUpgradePath>()
			{
				ConfigurationStorage = this.GetConfigurationStorage(source)
			};
			var rules = c.GetConfigurationUpgradeRules(this.GetVaultMock().Object)?.ToList();
			Assert.AreEqual(1, rules.Count);
			Assert.AreEqual(typeof(VersionZero), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithStaticUpgradePath), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeToType);
		}

		[TestMethod]
		public void GetConfigurationUpgradeRules_ZeroToTwoUpgradeWithInstanceUpgradePath()
		{
			var source = new VersionZero();
			var c = new ConfigurableVaultApplicationBaseProxy<VersionTwoWithInstanceUpgradePath>()
			{
				ConfigurationStorage = this.GetConfigurationStorage(source)
			};
			var rules = c.GetConfigurationUpgradeRules(this.GetVaultMock().Object)?.ToList();
			Assert.AreEqual(2, rules.Count);
			Assert.AreEqual(typeof(VersionZero), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeToType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), (rules[1] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(VersionTwoWithInstanceUpgradePath), (rules[1] as DeclaredConfigurationUpgradeRule).UpgradeToType);
		}

		[TestMethod]
		public void GetConfigurationUpgradeRules_ZeroToThreeUpgradeWithInstanceUpgradePath()
		{
			var source = new VersionZero();
			var c = new ConfigurableVaultApplicationBaseProxy<VersionThreeWithInstanceUpgradePath>()
			{
				ConfigurationStorage = this.GetConfigurationStorage(source)
			};
			var rules = c.GetConfigurationUpgradeRules(this.GetVaultMock().Object)?.ToList();
			Assert.AreEqual(3, rules.Count);
			Assert.AreEqual(typeof(VersionZero), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeToType);
			Assert.AreEqual(typeof(VersionOneWithInstanceUpgradePath), (rules[1] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(VersionTwoWithInstanceUpgradePath), (rules[1] as DeclaredConfigurationUpgradeRule).UpgradeToType);
			Assert.AreEqual(typeof(VersionTwoWithInstanceUpgradePath), (rules[2] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(VersionThreeWithInstanceUpgradePath), (rules[2] as DeclaredConfigurationUpgradeRule).UpgradeToType);
		}

		[TestMethod]
		public void GetConfigurationUpgradeRules_AlreadyAtCorrectVersion()
		{
			var source = new VersionThreeWithInstanceUpgradePath();
			var c = new ConfigurableVaultApplicationBaseProxy<VersionThreeWithInstanceUpgradePath>()
			{
				ConfigurationStorage = this.GetConfigurationStorage(source)
			};
			var rules = c.GetConfigurationUpgradeRules(this.GetVaultMock().Object)?.ToList();
			Assert.AreEqual(0, rules.Count);
		}

		/// <summary>
		/// We have a rule that goes from 2->1 and also from 1->2.
		/// Only one should be returned.
		/// </summary>
		[TestMethod]
		public void GetConfigurationUpgradeRules_CyclicUpgradeRule()
		{
			var source = new CyclicUpgradeRule1();
			var c = new ConfigurableVaultApplicationBaseProxy<CyclicUpgradeRule2>()
			{
				ConfigurationStorage = this.GetConfigurationStorage(source)
			};
			var rules = c.GetConfigurationUpgradeRules(this.GetVaultMock().Object)?.ToList();
			Assert.AreEqual(1, rules.Count);
			Assert.AreEqual(typeof(CyclicUpgradeRule1), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeFromType);
			Assert.AreEqual(typeof(CyclicUpgradeRule2), (rules[0] as DeclaredConfigurationUpgradeRule).UpgradeToType);
		}

		[DataContract]
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
