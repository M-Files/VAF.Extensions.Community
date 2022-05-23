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
	public class SingleNamedValueItemUpgradeRuleBaseTests
	{
		[DataRow
		(
			"hello", "world", MFNamedValueType.MFConfigurationValue,
			"hello1", "world", MFNamedValueType.MFConfigurationValue,
			"{}"
		)]
		[DataRow
		(
			"hello", "world", MFNamedValueType.MFConfigurationValue,
			"hello", "world", MFNamedValueType.MFSystemAdminConfiguration,
			"{}"
		)]
		[TestMethod]
		public void RemoveNamedValues_IsCalledIfDifferentWriteLocation
		(
			string @ns1, string name1, MFNamedValueType type1, 
			string @ns2, string name2, MFNamedValueType type2,
			string existingValue
		)
		{
			var namedValueMock = new Mock<INamedValueStorageManager>();
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type1, @ns1))
				.Returns(() =>
				{
					var x = new NamedValues();
					x[name1] = existingValue;
					return x;
				});
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type2, @ns2))
				.Returns(() =>
				{
					return new NamedValues();
				});

			var rule = new SingleNamedValueItemUpgradeRuleBaseProxy
			(
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == @ns1 && m.Name == name1 && m.NamedValueType == type1 && m.IsValid() == true),
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == @ns2 && m.Name == name2 && m.NamedValueType == type2 && m.IsValid() == true),
				new Version("0.0"),
				new Version("1.0")
			)
			{
				NamedValueStorageManager = namedValueMock.Object
			};
			rule.Execute(Mock.Of<Vault>());

			namedValueMock.Verify
			(
				m => m.RemoveNamedValues(It.IsAny<Vault>(), type1, @ns1, new[] { name1 }),
				Times.Exactly(1)
			);
		}

		[DataRow
		(
			"hello", "world", MFNamedValueType.MFConfigurationValue,
			"hello", "world", MFNamedValueType.MFConfigurationValue,
			"{}"
		)]
		[TestMethod]
		public void RemoveNamedValues_IsNotCalledIfSameWriteLocation
		(
			string @ns1, string name1, MFNamedValueType type1,
			string @ns2, string name2, MFNamedValueType type2,
			string existingValue
		)
		{
			var namedValueMock = new Mock<INamedValueStorageManager>();
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type1, @ns1))
				.Returns(() =>
				{
					var x = new NamedValues();
					x[name1] = existingValue;
					return x;
				});
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type2, @ns2))
				.Returns(() =>
				{
					return new NamedValues();
				});

			var rule = new SingleNamedValueItemUpgradeRuleBaseProxy
			(
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == @ns1 && m.Name == name1 && m.NamedValueType == type1 && m.IsValid() == true),
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == @ns2 && m.Name == name2 && m.NamedValueType == type2 && m.IsValid() == true),
				new Version("0.0"),
				new Version("1.0")
			)
			{
				NamedValueStorageManager = namedValueMock.Object
			};
			rule.Execute(Mock.Of<Vault>());

			namedValueMock.Verify
			(
				m => m.RemoveNamedValues(It.IsAny<Vault>(), type1, @ns1, new[] { name1 }),
				Times.Exactly(0)
			);
		}
		[DataRow
		(
			"hello", "world", MFNamedValueType.MFConfigurationValue,
			"{}"
		)]
		[TestMethod]
		public void RemoveNamedValues_IsNotCalledIfMissingWriteLocation
		(
			string @ns1, string name1, MFNamedValueType type1,
			string existingValue
		)
		{
			var namedValueMock = new Mock<INamedValueStorageManager>();
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type1, @ns1))
				.Returns(() =>
				{
					var x = new NamedValues();
					x[name1] = existingValue;
					return x;
				});

			var rule = new SingleNamedValueItemUpgradeRuleBaseProxy
			(
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == @ns1 && m.Name == name1 && m.NamedValueType == type1 && m.IsValid() == true),
				new Version("0.0"),
				new Version("1.0")
			)
			{
				NamedValueStorageManager = namedValueMock.Object
			};
			rule.Execute(Mock.Of<Vault>());

			namedValueMock.Verify
			(
				m => m.RemoveNamedValues(It.IsAny<Vault>(), type1, @ns1, new[] { name1 }),
				Times.Exactly(0)
			);
		}
	}
	public class SingleNamedValueItemUpgradeRuleBaseProxy
		: SingleNamedValueItemUpgradeRuleBase
	{
		public SingleNamedValueItemUpgradeRuleBaseProxy
		(
			VaultApplicationBase vaultApplication, 
			Version migrateFromVersion, 
			Version migrateToVersion
		) : base(vaultApplication, migrateFromVersion, migrateToVersion)
		{
		}

		public SingleNamedValueItemUpgradeRuleBaseProxy
		(
			ISingleNamedValueItem readFromAndWriteTo, 
			Version migrateFromVersion,
			Version migrateToVersion
		) : base(readFromAndWriteTo, migrateFromVersion, migrateToVersion)
		{
		}

		public SingleNamedValueItemUpgradeRuleBaseProxy
		(
			ISingleNamedValueItem readFrom,
			ISingleNamedValueItem writeTo, 
			Version migrateFromVersion, 
			Version migrateToVersion
		) : base(readFrom, writeTo, migrateFromVersion, migrateToVersion)
		{
		}

		protected override string Convert(string input)
		{
			return input;
		}
	}
}
