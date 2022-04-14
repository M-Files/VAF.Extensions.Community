using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	[TestClass]
	public class SimpleConvertConfigurationTypeUpgradeRuleTests
		: UpgradeRuleTestBase
	{
		public Mock<IConfigurationStorage> GetConfigurationStorageMock(string existingData)
		{
			var mock = new Mock<IConfigurationStorage>();

			mock
				.Setup(m => m.ReadConfigurationData(It.IsAny<Vault>(), It.IsAny<string>(), It.IsAny<string>(), out existingData))
				.Returns(false == string.IsNullOrWhiteSpace(existingData));

			return mock;
		}
		public Mock<IConfigurationStorage> GetConfigurationStorageMock<T>(T existingObject)
			where T : class, new()
		{
			var mock = this.GetConfigurationStorageMock("hello world");

			mock
				.Setup(m => m.Deserialize<T>(It.IsAny<string>()))
				.Returns(existingObject);

			return mock;
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullOptionsThrows()
		{
			new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>(null, (a) => new ConfigurationVersion1());
		}

		[TestMethod]
		public void Execute_CallsReadsConfiguration()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);
			sourceMock.SetupAllProperties();
			sourceMock.Object.Namespace = "Namespace";
			sourceMock.Object.Name = "key";
			sourceMock.Object.NamedValueType = MFNamedValueType.MFSystemAdminConfiguration;

			var storageMock = this.GetConfigurationStorageMock("hello world");
			var instance = new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				(a) => new ConfigurationVersion1(),
				storageMock.Object
			);

			instance.Execute(Mock.Of<Vault>());

			storageMock.Verify(m => m.ReadConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, sourceMock.Object.Name, out It.Ref<string>.IsAny));
		}

		[TestMethod]
		public void Execute_CallsReadsConfiguration_ReturnsFalseIfNoData()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);

			var storageMock = this.GetConfigurationStorageMock((string)null);
			var instance = new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				(a) => new ConfigurationVersion1(),
				storageMock.Object
			);

			Assert.IsFalse(instance.Execute(Mock.Of<Vault>()));
		}

		[TestMethod]
		public void Execute_CallsDeserialize()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);

			var storageMock = this.GetConfigurationStorageMock(new ConfigurationVersion0() { X = "hello world" });
			var instance = new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				(a) => new ConfigurationVersion1(),
				storageMock.Object
			);

			Assert.IsTrue(instance.Execute(Mock.Of<Vault>()));

			storageMock.Verify(m => m.Deserialize<ConfigurationVersion0>(It.IsAny<string>()));
		}

		[TestMethod]
		public void Execute_CallsConvert()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);

			var storageMock = this.GetConfigurationStorageMock(new ConfigurationVersion0() { X = "hello world" });
			var calledConvert = false;
			var instance = new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				(input) =>
				{
					calledConvert = true;
					return new ConfigurationVersion1();
				},
				storageMock.Object
			);

			Assert.IsTrue(instance.Execute(Mock.Of<Vault>()));
			Assert.IsTrue(calledConvert);
		}

		[TestMethod]
		public void Execute_CallsSaveConfigurationData()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);
			sourceMock.SetupAllProperties();
			sourceMock.Object.Namespace = "Namespace";
			sourceMock.Object.Name = "key";
			sourceMock.Object.NamedValueType = MFNamedValueType.MFSystemAdminConfiguration;

			var storageMock = this.GetConfigurationStorageMock(new ConfigurationVersion0() { X = "hello world" });
			var output = new ConfigurationVersion1();
			var instance = new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				(input) => output,
				storageMock.Object
			);

			Assert.IsTrue(instance.Execute(Mock.Of<Vault>()));

			storageMock.Verify(m => m.SaveConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, "{\r\n  \"Version\": \"1.0\"\r\n}", sourceMock.Object.Name));
		}

		[TestMethod]
		public void Execute_OneToTwo()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);
			sourceMock.SetupAllProperties();
			sourceMock.Object.Namespace = "Namespace";
			sourceMock.Object.Name = "key";
			sourceMock.Object.NamedValueType = MFNamedValueType.MFSystemAdminConfiguration;

			var storageMock = this.GetConfigurationStorageMock<ConfigurationVersion1>(new ConfigurationVersion1() { Y = "hello world" });
			var instance = new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>
			(
				new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				(input) => new ConfigurationVersion2() { Y = input.Y },
				storageMock.Object
			);

			Assert.IsTrue(instance.Execute(Mock.Of<Vault>()));

			storageMock
				.Verify
				(
					m => m.SaveConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, "{\r\n  \"Version\": \"2.0\"\r\n}", sourceMock.Object.Name),
					Times.Once
				);
		}

		[TestMethod]
		public void Execute_SkipsConversionIfAlreadyAtSameLevel()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);
			sourceMock.SetupAllProperties();
			sourceMock.Object.Namespace = "Namespace";
			sourceMock.Object.Name = "key";
			sourceMock.Object.NamedValueType = MFNamedValueType.MFSystemAdminConfiguration;

			var storageMock = this.GetConfigurationStorageMock<ConfigurationVersion1>(new ConfigurationVersion2() { Y = "hello world" });
			var instance = new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>
			(
				new SimpleConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				(input) => new ConfigurationVersion2() { Y = input.Y },
				storageMock.Object
			);

			Assert.IsFalse(instance.Execute(Mock.Of<Vault>()));

			storageMock
				.Verify
				(
					m => m.SaveConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, "{\r\n  \"Version\": \"2.0\"\r\n}", sourceMock.Object.Name),
					Times.Never
				);
		}

		private class ConfigurationVersion0
			: VersionedConfigurationBase
		{
			public string X { get; set; }
		}
		[ConfigurationVersion("1.0")]
		private class ConfigurationVersion1
			: VersionedConfigurationBase
		{
			public string Y { get; set; }
		}
		[ConfigurationVersion("2.0")]
		private class ConfigurationVersion2
			: ConfigurationVersion1
		{
			public string Z { get; set; }
		}
	}
}
