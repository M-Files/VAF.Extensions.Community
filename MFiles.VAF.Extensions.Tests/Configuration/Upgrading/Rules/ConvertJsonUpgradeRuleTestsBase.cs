using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	public abstract class ConvertJsonUpgradeRuleTestsBase
		: UpgradeRuleTestBase
	{

		public Mock<IConfigurationStorage> GetConfigurationStorageMock(string existingData)
		{
			var mock = new Mock<IConfigurationStorage>();

			mock
				.Setup(m => m.ReadConfigurationData(It.IsAny<Vault>(), It.IsAny<string>(), It.IsAny<string>(), out existingData))
				.Returns(false == string.IsNullOrWhiteSpace(existingData));

			mock
				.Setup(m => m.Deserialize<VersionedConfigurationBase>(It.IsAny<string>()))
				.Returns((string input) => Newtonsoft.Json.JsonConvert.DeserializeObject<VersionedConfigurationBase>(input));

			return mock;
		}
		public Mock<IConfigurationStorage> GetConfigurationStorageMock<T>(T existingObject)
			where T : class, new()
		{
			var mock = this.GetConfigurationStorageMock(Newtonsoft.Json.JsonConvert.SerializeObject(existingObject));

			mock
				.Setup(m => m.Deserialize<T>(It.IsAny<string>()))
				.Returns(existingObject);

			return mock;
		}

	}

	[TestClass]
	public class ConvertJsonUpgradeRuleTests
		: ConvertJsonUpgradeRuleTestsBase
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullOptionsThrows()
		{
			new ConvertJsonUpgradeRuleTestsProxy(null, new Version(), new Version(), (s) => s);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullMigrateToThrows()
		{
			new ConvertJsonUpgradeRuleTestsProxy
			(
				new ConvertJsonUpgradeRuleBase.UpgradeRuleOptions()
				{
					Source = new SingleNamedValueItem(MFNamedValueType.MFSystemAdminConfiguration, "a", "b")
				},
				new Version(),
				null,
				(s) => s
			);
		}

		[TestMethod]
		public void Constructor_NullMigrateFromDoesNotThrow()
		{
			new ConvertJsonUpgradeRuleTestsProxy
			(
				new ConvertJsonUpgradeRuleBase.UpgradeRuleOptions()
				{
					Source = new SingleNamedValueItem(MFNamedValueType.MFSystemAdminConfiguration, "a", "b")
				},
				null,
				new Version(),
				(s) => s
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_NullSourceThrows()
		{
			new ConvertJsonUpgradeRuleTestsProxy
			(
				new ConvertJsonUpgradeRuleBase.UpgradeRuleOptions()
				{
					Source = null
				},
				new Version(),
				new Version(),
				(s) => s
			);
		}

		[TestMethod]
		public void Execute_AddsVersionIfMissing()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);
			sourceMock.SetupAllProperties();
			sourceMock.Object.Namespace = "Namespace";
			sourceMock.Object.Name = "key";
			sourceMock.Object.NamedValueType = MFNamedValueType.MFSystemAdminConfiguration;

			var storageMock = this.GetConfigurationStorageMock("{}");

			var proxy = new ConvertJsonUpgradeRuleTestsProxy
			(
				new ConvertJsonUpgradeRuleBase.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				null,
				new Version("1.2"),
				(s) => "{}", // Purposefully do not return the version data here.
				storageMock.Object
			);

			// Execute.
			Assert.IsTrue(proxy.Execute(Mock.Of<Vault>()));

			// Ensure the saved value has the correct version.
			storageMock.Verify(m => m.SaveConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, "{\r\n  \"Version\": \"1.2\"\r\n}", sourceMock.Object.Name));

		}

		[TestMethod]
		public void Execute_UpdatesVersionIfIncorrect()
		{
			var sourceMock = new Mock<ISingleNamedValueItem>();
			sourceMock.Setup(m => m.IsValid()).Returns(true);
			sourceMock.SetupAllProperties();
			sourceMock.Object.Namespace = "Namespace";
			sourceMock.Object.Name = "key";
			sourceMock.Object.NamedValueType = MFNamedValueType.MFSystemAdminConfiguration;

			var storageMock = this.GetConfigurationStorageMock("{}");

			var proxy = new ConvertJsonUpgradeRuleTestsProxy
			(
				new ConvertJsonUpgradeRuleBase.UpgradeRuleOptions()
				{
					Source = sourceMock.Object
				},
				null,
				new Version("1.2"),
				(s) => "{\"Version\":\"1.0\"}", // Purposefully do not return the correct version number.
				storageMock.Object
			);

			// Execute.
			Assert.IsTrue(proxy.Execute(Mock.Of<Vault>()));

			// Ensure the saved value has the correct version.
			storageMock.Verify(m => m.SaveConfigurationData(It.IsAny<Vault>(), sourceMock.Object.Namespace, "{\r\n  \"Version\": \"1.2\"\r\n}", sourceMock.Object.Name));

		}

		public class ConvertJsonUpgradeRuleTestsProxy
			: ConvertJsonUpgradeRuleBase
		{
			private Func<string, string> Conversion { get; }
			public ConvertJsonUpgradeRuleTestsProxy
			(
				UpgradeRuleOptions options,
				Version migrateFrom,
				Version migrateTo,
				Func<string, string> conversion,
				IConfigurationStorage configurationStorage = null
			)
				: base(options, migrateFrom, migrateTo, configurationStorage)
			{
				this.Conversion = conversion ?? throw new ArgumentNullException(nameof(conversion));
			}
			protected override string Convert(string input)
				=> this.Conversion(input);
		}
	}
}
