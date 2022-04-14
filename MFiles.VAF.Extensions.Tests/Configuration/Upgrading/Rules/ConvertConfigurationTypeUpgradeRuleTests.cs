using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{

	[TestClass]
	public class ConvertConfigurationTypeUpgradeRuleTests
		: ConvertJsonUpgradeRuleTestsBase
	{

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullOptionsThrows()
		{
			new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>(null, (a) => new ConfigurationVersion1());
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
			var instance = new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
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
			var instance = new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
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
			var instance = new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
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
			var instance = new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
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
			var instance = new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>
			(
				new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion0, ConfigurationVersion1>.UpgradeRuleOptions()
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
			var instance = new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>
			(
				new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>.UpgradeRuleOptions()
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
					m => m.SaveConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, "{\r\n  \"Y\": \"hello world\",\r\n  \"Version\": \"2.0\"\r\n}", sourceMock.Object.Name),
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
			var instance = new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>
			(
				new ConvertConfigurationTypeUpgradeRule<ConfigurationVersion1, ConfigurationVersion2>.UpgradeRuleOptions()
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
					m => m.SaveConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, It.IsAny<string>(), sourceMock.Object.Name),
					Times.Never
				);
		}

		[DataContract]
		public class ConfigurationVersion0
			: VersionedConfigurationBase
		{
			[DataMember]
			public string X { get; set; }
		}
		[DataContract]
		[ConfigurationVersion("1.0")]
		public class ConfigurationVersion1
			: VersionedConfigurationBase
		{
			[DataMember]
			public string Y { get; set; }
		}
		[DataContract]
		[ConfigurationVersion("2.0")]
		public class ConfigurationVersion2
			: ConfigurationVersion1
		{
			[DataMember]
			public string Z { get; set; }
		}
	}
}
