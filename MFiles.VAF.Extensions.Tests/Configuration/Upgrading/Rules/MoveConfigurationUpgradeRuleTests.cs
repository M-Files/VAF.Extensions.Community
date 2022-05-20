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
	public class MoveConfigurationUpgradeRuleTests
		: UpgradeRuleTestBase
	{

		#region Constructor tests

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullReadFromThrows()
		{
			new MoveConfigurationUpgradeRuleProxy(null, this.CreateSingleNamedValueItemMock(true).Object, new Version("0.0"), new Version("0.0"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullWriteToThrows()
		{
			new MoveConfigurationUpgradeRuleProxy(this.CreateSingleNamedValueItemMock(true).Object, null, new Version("0.0"), new Version("0.0"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullMigrateFromThrows()
		{
			new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(true).Object,
				null,
				new Version("0.0")
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullMigrateToThrows()
		{
			new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(true).Object,
				new Version("0.0"),
				null
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_InvalidOptionsThrows_InvalidSource()
		{
			new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(false).Object, 
				this.CreateSingleNamedValueItemMock(true).Object,
				new Version("0.0"),
				new Version("0.0")
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_InvalidOptionsThrows_InvalidTarget()
		{
			new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(false).Object,
				new Version("0.0"),
				new Version("0.0")
			);
		}

		[TestMethod]
		public void Constructor_ValidOptions()
		{
			new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(true).Object,
				new Version("0.0"),
				new Version("0.0")
			);
		}

		#endregion

		[TestMethod]
		public void Execute_ReturnsFalseIfNoMatchingNamedValues()
		{
			var mock = new Mock<INamedValueStorageManager>();
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), DefaultSourceNVSType, DefaultSourceNamespace))
				.Returns((NamedValues)null)
				.Verifiable("Data was not retrieved from NVS.");

			var rule = new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(true).Object,
				new Version("0.0"),
				new Version("0.0"),
				mock
			);

			Assert.IsFalse(rule.Execute(Mock.Of<Vault>()));

			mock.Verify();
		}

		[TestMethod]
		public void Execute_SetsValuesAndReturnsTrue()
		{
			var namedValues = new NamedValues();
			namedValues["conf"] = "hello world";
			var mock = new Mock<INamedValueStorageManager>();
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), DefaultSourceNVSType, DefaultSourceNamespace))
				.Returns(namedValues)
				.Verifiable("Data was not retrieved from NVS.");
			mock.Setup(m => m.SetNamedValues(It.IsAny<Vault>(), DefaultTargetNVSType, DefaultTargetNamespace, namedValues))
				.Verifiable("Data was not set in NVS.");

			var rule = new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(true).Object,
				new Version("0.0"),
				new Version("0.0"),
				mock
			);

			Assert.IsTrue(rule.Execute(Mock.Of<Vault>()));

			mock.Verify();
		}

		[TestMethod]
		public void Execute_DoesNotRemoveSourceValues()
		{
			var namedValues = new NamedValues();
			var mock = new Mock<INamedValueStorageManager>();
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), DefaultSourceNVSType, DefaultSourceNamespace))
				.Returns(namedValues);

			var rule = new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(true).Object,
				new Version("0.0"),
				new Version("0.0"),
				mock
			);

			Assert.IsTrue(rule.Execute(Mock.Of<Vault>()));

			mock.Verify();
			mock.Verify
			(
				m => m.RemoveNamedValues
				(
					It.IsAny<Vault>(),
					It.IsAny<MFNamedValueType>(),
					It.IsAny<string>(),
					It.IsAny<string[]>()
				),
				Times.Never
			);
		}

		[TestMethod]
		public void Execute_DoesRemoveSourceValues()
		{
			var namedValues = new NamedValues();
			namedValues["hello"] = "world";
			var mock = new Mock<INamedValueStorageManager>();
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), DefaultSourceNVSType, DefaultSourceNamespace))
				.Returns(namedValues);
			mock.Setup(m => m.RemoveNamedValues(It.IsAny<Vault>(), DefaultSourceNVSType, DefaultSourceNamespace, It.IsAny<string[]>()))
				.Callback((Vault vault, MFNamedValueType type, string @namespace, string[] names) =>
				{
					Assert.AreEqual(1, names.Length);
					Assert.AreEqual("hello", names[0]);
				});

			var rule = new MoveConfigurationUpgradeRuleProxy
			(
				this.CreateSingleNamedValueItemMock(true).Object,
				this.CreateSingleNamedValueItemMock(true).Object,
				new Version("0.0"),
				new Version("0.0"),
				mock
			);

			Assert.IsTrue(rule.Execute(Mock.Of<Vault>()));

			mock.Verify();
			mock.Verify
			(
				m => m.RemoveNamedValues
				(
					It.IsAny<Vault>(),
					It.IsAny<MFNamedValueType>(),
					It.IsAny<string>(),
					It.IsAny<string[]>()
				),
				Times.Once
			);
		}

		public class MoveConfigurationUpgradeRuleProxy
			: MoveConfigurationUpgradeRule
		{
			public Mock<INamedValueStorageManager> NamedValueStorageManagerMock { get; }
			public MoveConfigurationUpgradeRuleProxy(ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo, Version migrateFromVersion, Version migrateToVersion, Mock<INamedValueStorageManager> mock = null) 
				: base(readFrom, writeTo, migrateFromVersion, migrateToVersion)
			{
				this.NamedValueStorageManagerMock = mock ?? new Mock<INamedValueStorageManager>();
				this.NamedValueStorageManager = this.NamedValueStorageManagerMock.Object;
			}
		}
	}
}
