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
	public abstract class UpgradeRuleTestBase
	{
		public Mock<ISourceNamedValueItem> CreateSourceNamedValueItemMock(bool isValid)
		{
			var mock = new Mock<ISourceNamedValueItem>();
			mock.Setup(m => m.IsValid()).Returns(isValid);
			return mock;
		}
		public Mock<ITargetNamedValueItem> CreateTargetNamedValueItemMock(bool isValid)
		{
			var mock = new Mock<ITargetNamedValueItem>();
			mock.Setup(m => m.IsValid()).Returns(isValid);
			return mock;
		}
	}
	[TestClass]
	public class MoveConfigurationUpgradeRuleTests
		: UpgradeRuleTestBase
	{

		#region Constructor tests

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullOptionsThrows()
		{
			new MoveConfigurationUpgradeRuleProxy(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_InvalidOptionsThrows_NullSource()
		{
			new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = null,
				Target = this.CreateTargetNamedValueItemMock(true).Object
			});
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_InvalidOptionsThrows_InvalidSource()
		{
			new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = this.CreateSourceNamedValueItemMock(false).Object,
				Target = this.CreateTargetNamedValueItemMock(true).Object
			});
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_InvalidOptionsThrows_NullTarget()
		{
			new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = this.CreateSourceNamedValueItemMock(true).Object,
				Target = null
			});
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Constructor_InvalidOptionsThrows_InvalidTarget()
		{
			new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = this.CreateSourceNamedValueItemMock(true).Object,
				Target = this.CreateTargetNamedValueItemMock(false).Object
			});
		}

		[TestMethod]
		public void Constructor_ValidOptions()
		{
			new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = this.CreateSourceNamedValueItemMock(true).Object,
				Target = this.CreateTargetNamedValueItemMock(true).Object
			});
		}

		#endregion

		[TestMethod]
		public void Execute_ReturnsFalseIfNoMatchingNamedValues()
		{
			var mock = new Mock<INamedValueStorageManager>();
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), MFNamedValueType.MFConfigurationValue, "a"))
				.Returns((NamedValues)null)
				.Verifiable("Data was not retrieved from NVS.");

			var sourceMock = this.CreateSourceNamedValueItemMock(true);
			sourceMock.Setup(m => m.GetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>()))
				.Returns((INamedValueStorageManager manager, Vault vault) =>
				{
					return manager?.GetNamedValues(vault, MFNamedValueType.MFConfigurationValue, "a");
				});

			var rule = new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = sourceMock.Object,
				Target = this.CreateTargetNamedValueItemMock(true).Object
			}, mock);

			Assert.IsFalse(rule.Execute(Mock.Of<Vault>()));

			mock.Verify();
		}

		[TestMethod]
		public void Execute_SetsValuesAndReturnsTrue()
		{
			var namedValues = new NamedValues();
			var mock = new Mock<INamedValueStorageManager>();
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), MFNamedValueType.MFConfigurationValue, "a"))
				.Returns(namedValues)
				.Verifiable("Data was not retrieved from NVS.");
			mock.Setup(m => m.SetNamedValues(It.IsAny<Vault>(), MFNamedValueType.MFConfigurationValue, "b", namedValues))
				.Verifiable("Data was not set in NVS.");

			var sourceMock = this.CreateSourceNamedValueItemMock(true);
			sourceMock.Setup(m => m.GetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>()))
				.Returns((INamedValueStorageManager manager, Vault vault) =>
				{
					return manager?.GetNamedValues(vault, MFNamedValueType.MFConfigurationValue, "a");
				});

			var targetMock = this.CreateTargetNamedValueItemMock(true);
			targetMock.Setup(m => m.SetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), namedValues))
				.Callback((INamedValueStorageManager manager, Vault vault, NamedValues nv) =>
				{
					manager?.SetNamedValues(vault, MFNamedValueType.MFConfigurationValue, "b", nv);
				});

			var rule = new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = sourceMock.Object,
				Target = targetMock.Object
			}, mock);

			Assert.IsTrue(rule.Execute(Mock.Of<Vault>()));

			mock.Verify();
		}

		[TestMethod]
		public void Execute_DoesNotRemoveSourceValues()
		{
			var namedValues = new NamedValues();
			var mock = new Mock<INamedValueStorageManager>();
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), MFNamedValueType.MFConfigurationValue, "a"))
				.Returns(namedValues);

			var sourceMock = this.CreateSourceNamedValueItemMock(true);
			sourceMock.Setup(m => m.GetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>()))
				.Returns((INamedValueStorageManager manager, Vault vault) =>
				{
					return manager?.GetNamedValues(vault, MFNamedValueType.MFConfigurationValue, "a");
				});

			var targetMock = this.CreateTargetNamedValueItemMock(true);
			targetMock.Setup(m => m.SetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), namedValues))
				.Callback((INamedValueStorageManager manager, Vault vault, NamedValues nv) =>
				{
					manager?.SetNamedValues(vault, MFNamedValueType.MFConfigurationValue, "b", nv);
				});

			var rule = new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = sourceMock.Object,
				Target = targetMock.Object,
				RemoveMovedValues = false
			}, mock);

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
			mock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), MFNamedValueType.MFConfigurationValue, "a"))
				.Returns(namedValues);
			mock.Setup(m => m.RemoveNamedValues(It.IsAny<Vault>(), MFNamedValueType.MFConfigurationValue, "a", It.IsAny<string[]>()))
				.Callback((Vault vault, MFNamedValueType type, string @namespace, string[] names) =>
				{
					Assert.AreEqual(1, names.Length);
					Assert.AreEqual("hello", names[0]);
				});

			var sourceMock = this.CreateSourceNamedValueItemMock(true);
			sourceMock.Setup(m => m.GetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>()))
				.Returns((INamedValueStorageManager manager, Vault vault) =>
				{
					return manager?.GetNamedValues(vault, MFNamedValueType.MFConfigurationValue, "a");
				});
			sourceMock.Setup(m => m.RemoveNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), It.IsAny<string[]>()))
				.Callback((INamedValueStorageManager manager, Vault vault, string[] names) =>
				{
					manager?.RemoveNamedValues(vault, MFNamedValueType.MFConfigurationValue, "a", names);
				});

			var targetMock = this.CreateTargetNamedValueItemMock(true);
			targetMock.Setup(m => m.SetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), namedValues))
				.Callback((INamedValueStorageManager manager, Vault vault, NamedValues nv) =>
				{
					manager?.SetNamedValues(vault, MFNamedValueType.MFConfigurationValue, "b", nv);
				});

			var rule = new MoveConfigurationUpgradeRuleProxy(new MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions()
			{
				Source = sourceMock.Object,
				Target = targetMock.Object,
				RemoveMovedValues = true
			}, mock);

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
			public MoveConfigurationUpgradeRuleProxy(MoveConfigurationUpgradeRuleOptions options, Mock<INamedValueStorageManager> mock = null) 
				: base(options)
			{
				this.NamedValueStorageManagerMock = mock ?? new Mock<INamedValueStorageManager>();
				this.NamedValueStorageManager = this.NamedValueStorageManagerMock.Object;
			}
		}
	}
}
