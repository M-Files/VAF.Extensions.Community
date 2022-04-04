using MFiles.VAF.Configuration;
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
	public class ConvertConfigurationTypeUpgradeRuleTests
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
			new ConvertConfigurationTypeUpgradeRuleProxy(null);
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
			var instance = new ConvertConfigurationTypeUpgradeRuleProxy(new ConvertConfigurationTypeUpgradeRule<Input, Output>.ConvertConfigurationTypeUpgradeRuleOptions()
			{
				Source = sourceMock.Object
			}, storageMock.Object);

			instance.Execute(Mock.Of<Vault>());

			storageMock.Verify(m => m.ReadConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, sourceMock.Object.Name, out It.Ref<string>.IsAny));
		}

		[TestMethod]
		public void Execute_CallsReadsConfiguration_ReturnsFalseIfNoData()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);

			var storageMock = this.GetConfigurationStorageMock((string)null);
			var instance = new ConvertConfigurationTypeUpgradeRuleProxy(new ConvertConfigurationTypeUpgradeRule<Input, Output>.ConvertConfigurationTypeUpgradeRuleOptions()
			{
				Source = sourceMock.Object
			}, storageMock.Object);

			Assert.IsFalse(instance.Execute(Mock.Of<Vault>()));
		}

		[TestMethod]
		public void Execute_CallsDeserialize()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);

			var storageMock = this.GetConfigurationStorageMock(new Input() { X = "hello world" });
			var instance = new ConvertConfigurationTypeUpgradeRuleProxy(new ConvertConfigurationTypeUpgradeRule<Input, Output>.ConvertConfigurationTypeUpgradeRuleOptions()
			{
				Source = sourceMock.Object
			}, storageMock.Object);

			Assert.IsTrue(instance.Execute(Mock.Of<Vault>()));

			storageMock.Verify(m => m.Deserialize<Input>(It.IsAny<string>()));
		}

		[TestMethod]
		public void Execute_CallsConvert()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);

			var storageMock = this.GetConfigurationStorageMock(new Input() { X = "hello world" });
			var calledConvert = false;
			var instance = new ConvertConfigurationTypeUpgradeRuleProxy
			(
				new ConvertConfigurationTypeUpgradeRule<Input, Output>.ConvertConfigurationTypeUpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				storageMock.Object,
				(input) =>
				{
					calledConvert = true;
					return new Output();
				}
			);

			Assert.IsTrue(instance.Execute(Mock.Of<Vault>()));
			Assert.IsTrue(calledConvert);
		}

		[TestMethod]
		public void Execute_CallsSave()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);
			sourceMock.SetupAllProperties();
			sourceMock.Object.Namespace = "Namespace";
			sourceMock.Object.Name = "key";
			sourceMock.Object.NamedValueType = MFNamedValueType.MFSystemAdminConfiguration;

			var storageMock = this.GetConfigurationStorageMock(new Input() { X = "hello world" });
			var output = new Output();
			var instance = new ConvertConfigurationTypeUpgradeRuleProxy
			(
				new ConvertConfigurationTypeUpgradeRule<Input, Output>.ConvertConfigurationTypeUpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				storageMock.Object,
				(input) => output
			);

			Assert.IsTrue(instance.Execute(Mock.Of<Vault>()));

			storageMock.Verify(m => m.Save(It.IsAny<Vault>(), It.IsAny<object>(), sourceMock.Object.Namespace, sourceMock.Object.Name));
		}

		private class ConvertConfigurationTypeUpgradeRuleProxy
			: ConvertConfigurationTypeUpgradeRule<Input, Output>
		{
			public Func<Input, Output> Conversion { get; set; }
			public ConvertConfigurationTypeUpgradeRuleProxy
			(
				ConvertConfigurationTypeUpgradeRuleOptions options,
				Func<Input, Output> conversion = null
			)
				: base(options)
			{
				this.Conversion = conversion?? new Func<Input, Output>(input => this.DefaultConvert(input));
			}

			public ConvertConfigurationTypeUpgradeRuleProxy
			(
				ConvertConfigurationTypeUpgradeRuleOptions options, 
				IConfigurationStorage configurationStorage,
				Func<Input, Output> conversion = null
			)
				: base(options, configurationStorage)
			{
				this.Conversion = conversion ?? new Func<Input, Output>(input => this.DefaultConvert(input));
			}

			public Output DefaultConvert(Input input)
				=> new Output()
				{
					Y = input?.X
				};

			public override Output Convert(Input input)
				=> Conversion(input);

		}

		private class Input
		{
			public string X { get; set; }
		}
		private class Output
		{
			public string Y { get; set; }
		}
	}
}
