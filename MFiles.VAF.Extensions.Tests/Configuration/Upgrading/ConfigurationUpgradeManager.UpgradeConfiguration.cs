using MFiles.VAF.Core;
using MFiles.VAF;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Runtime.Serialization;
using MFiles.VAF.Configuration;
using System.Linq;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using System.Collections.Generic;
using System;
using static MFiles.VAF.Extensions.Tests.Configuration.Upgrading.ConfigurationUpgradeManager.UpgradeOnNewest;
using MFiles.VAF.Extensions.Tests.ExtensionMethods;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	public partial class ConfigurationUpgradeManager
		: TestBaseWithVaultMock
	{

		protected Mock<INamedValueStorageManager> GetNamedValueStorageManagerMock()
		{
			var dictionary = new Dictionary<string, Dictionary<MFNamedValueType, Dictionary<string, string>>>();

			var mock = new Mock<INamedValueStorageManager>();
			mock
				.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), It.IsAny<MFNamedValueType>(), It.IsAny<string>()))
				.Returns((Vault v, MFNamedValueType namedValueType, string @namespace) =>
				{
					var namedValues = new NamedValues();

					// Populate the return value.
					if (dictionary.ContainsKey(@namespace))
					{
						if (dictionary[@namespace].ContainsKey(namedValueType))
						{
							foreach (var key in dictionary[@namespace][namedValueType].Keys)
							{
								namedValues[key] = dictionary[@namespace][namedValueType][key];
							}
						}
					}

					return namedValues;
				});
			mock
				.Setup(m => m.SetNamedValues(It.IsAny<Vault>(), It.IsAny<MFNamedValueType>(), It.IsAny<string>(), It.IsAny<NamedValues>()))
				.Callback((Vault v, MFNamedValueType namedValueType, string @namespace, NamedValues namedValues) =>
				{
					namedValues = namedValues ?? new NamedValues();

					// Populate the values
					if (false == dictionary.ContainsKey(@namespace))
						dictionary.Add(@namespace, new Dictionary<MFNamedValueType, Dictionary<string, string>>());
					if(false == dictionary[@namespace].ContainsKey(namedValueType))
						dictionary[@namespace].Add(namedValueType, new Dictionary<string, string>());
					dictionary[@namespace][namedValueType].Clear();
					foreach(string key in namedValues.Names)
					{
						dictionary[@namespace][namedValueType].Add(key, namedValues[key]?.ToString());
					}
				});
			return mock;
		}

		protected NamedValues CreateNamedValues(string key, string item)
			=> this.CreateNamedValues(new Dictionary<string, string>
			{
				{ key, item }
			});
		protected NamedValues CreateNamedValues(Dictionary<string, string> items)
		{
			var namedValues = new NamedValues();
			if(null != items)
				foreach(var key in items.Keys)
					namedValues[key] = items[key];
			return namedValues;
		}

		[TestMethod]
		public void UpgradeConfiguration_ZeroToOneNoExistingConfiguration()
		{
			var vault = this.GetVaultMock().Object;
			var managerMock = this.GetNamedValueStorageManagerMock();
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = managerMock.Object
			};
			c.UpgradeConfiguration<VersionOneWithInstanceUpgradePath>(vault);

			// Attempt to retrieve the data.
			Assert.That.AreEqualJson(null, managerMock.Object.GetValue(vault, MFNamedValueType.MFConfigurationValue, "Castle.Proxies.VaultApplicationBaseProxy", "config"));
		}

		[TestMethod]
		public void UpgradeConfiguration_ZeroToOneUpgradeWithInstanceUpgradePath()
		{
			var vault = this.GetVaultMock().Object;
			var managerMock = this.GetNamedValueStorageManagerMock();
			var source = new VersionZero() { Hello = "World" };
			managerMock.Object.SetNamedValues
			(
				vault, 
				MFNamedValueType.MFConfigurationValue,
				"Castle.Proxies.VaultApplicationBaseProxy",
				this.CreateNamedValues("config", Newtonsoft.Json.JsonConvert.SerializeObject(source))
			);
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = managerMock.Object
			};
			c.UpgradeConfiguration<VersionOneWithInstanceUpgradePath>(vault);

			// Attempt to retrieve the data.
			Assert.That.AreEqualJson
			(
				Newtonsoft.Json.JsonConvert.SerializeObject(new VersionOneWithInstanceUpgradePath() {  World = "World" }, NewtonsoftJsonConvert.DefaultJsonSerializerSettings), 
				managerMock.Object.GetValue(vault, MFNamedValueType.MFSystemAdminConfiguration, "Castle.Proxies.VaultApplicationBaseProxy", "configuration")
			);
		}

		/// <summary>
		/// We changed the serialization process for a timespan property from a string ("01:02:03") to separate properties for hours/minutes/seconds.
		/// Ensure that if we give it the old format to load, it updates the JSON in NVS to the new format.
		/// </summary>
		[TestMethod]
		public void EnsureLatestSerializationSettings_TimeSpan_FromStringToSeparateElements()
		{
			var vault = this.GetVaultMock().Object;
			var managerMock = this.GetNamedValueStorageManagerMock();
			managerMock.Object.SetNamedValues
			(
				vault,
				MFNamedValueType.MFSystemAdminConfiguration,
				"Castle.Proxies.VaultApplicationBaseProxy",
				this.CreateNamedValues("configuration", @"{ ""World"" : ""abc"", ""Version"" : ""3.0"", ""TimeSpan"" : { ""Interval"" : ""01:02:03"", ""RunOnVaultStartup"" : false } }")
			);
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = managerMock.Object
			};
			c.UpgradeConfiguration<VersionThreeWithInstanceUpgradePath>(vault);

			// Attempt to retrieve the data.
			Assert.That.AreEqualJson
			(
				Newtonsoft.Json.JsonConvert.SerializeObject(new VersionThreeWithInstanceUpgradePath() { World = "abc", TimeSpan = new TimeSpanEx() { Hours = 1, Minutes = 2, Seconds = 3, RunOnVaultStartup = false } }, NewtonsoftJsonConvert.DefaultJsonSerializerSettings),
				managerMock.Object.GetValue(vault, MFNamedValueType.MFSystemAdminConfiguration, "Castle.Proxies.VaultApplicationBaseProxy", "configuration")
			);
		}

		/// <summary>
		/// We changed the serialization process for a timespan property from a string ("01:02:03") to separate properties for hours/minutes/seconds.
		/// Ensure that if we give it the old format to load, it updates the JSON in NVS to the new format.
		/// </summary>
		[TestMethod]
		public void EnsureLatestSerializationSettings_TimeSpan_FromStringToSeparateElements_CommentsPersisted()
		{
			var vault = this.GetVaultMock().Object;
			var managerMock = this.GetNamedValueStorageManagerMock();
			managerMock.Object.SetNamedValues
			(
				vault,
				MFNamedValueType.MFSystemAdminConfiguration,
				"Castle.Proxies.VaultApplicationBaseProxy",
				this.CreateNamedValues("configuration", @"{ ""Version"" : ""3.0"", ""TimeSpan"" : { ""Interval"" : ""01:02:03"", ""RunOnVaultStartup"" : false }, ""TimeSpan-Comment"": ""hello world"" }")
			);
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = managerMock.Object
			};
			c.UpgradeConfiguration<VersionThreeWithInstanceUpgradePath>(vault);

			// Attempt to retrieve the data, and that the basics are correct.
			var jObject = Newtonsoft.Json.Linq.JObject.Parse(managerMock.Object.GetValue(vault, MFNamedValueType.MFSystemAdminConfiguration, "Castle.Proxies.VaultApplicationBaseProxy", "configuration"));
			Assert.IsNotNull(jObject);

			// Ensure we have a comment!
			var timeSpanComment = jObject["TimeSpan-Comment"];
			Assert.AreEqual("hello world", timeSpanComment.ToString());
		}

	}
}
