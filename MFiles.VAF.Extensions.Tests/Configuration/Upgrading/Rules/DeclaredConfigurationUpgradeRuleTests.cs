using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	[TestClass]
	public class DeclaredConfigurationUpgradeRuleTests
	{
		[TestMethod]
		public void StringUpgradeMethodCalled()
		{
			string output = "";
			Func<string, object> upgradeMethod = (string input) =>
			{
				return new { value = "upgraded" };
			};
			var type = MFNamedValueType.MFConfigurationValue;
			var ns = "hello.world";
			var name = "config";
			var namedValueMock = new Mock<INamedValueStorageManager>();
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type, ns))
				.Returns(() =>
				{
					var x = new NamedValues();
					x[name] = "{}";
					return x;
				});
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type, ns))
				.Returns(() =>
				{
					var x = new NamedValues();
					x[name] = "{}";
					return x;
				});
			namedValueMock.Setup(m => m.SetNamedValues(It.IsAny<Vault>(), type, ns, It.IsAny<NamedValues>()))
				.Callback((Vault v, MFNamedValueType t, string n, NamedValues namedValues) =>
				{
					output = namedValues[name]?.ToString();
				});

			var rule = new DeclaredConfigurationUpgradeRule
			(
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == ns && m.Name == name && m.NamedValueType == type && m.IsValid() == true),
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == ns && m.Name == name && m.NamedValueType == type && m.IsValid() == true),
				new Version("0.0"),
				new Version("1.0"),
				upgradeMethod.Method
			)
			{
				NamedValueStorageManager = namedValueMock.Object,
				UpgradeFromType = upgradeMethod.Method.DeclaringType,
				UpgradeToType = upgradeMethod.Method.DeclaringType
			};
			Assert.IsTrue(rule.Execute(Mock.Of<Vault>()));

			Assert.AreEqual
			(
				rule.JsonConvert.Serialize(new { value = "upgraded", Version = "1.0" }), 
				output
			);
		}


		[TestMethod]
		public void JObjectMethodCalled()
		{
			string output = "";
			Func<JObject, object> upgradeMethod = (JObject input) =>
			{
				return new { value = "upgraded" };
			};
			var type = MFNamedValueType.MFConfigurationValue;
			var ns = "hello.world";
			var name = "config";
			var namedValueMock = new Mock<INamedValueStorageManager>();
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type, ns))
				.Returns(() =>
				{
					var x = new NamedValues();
					x[name] = "{}";
					return x;
				});
			namedValueMock.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), type, ns))
				.Returns(() =>
				{
					var x = new NamedValues();
					x[name] = "{}";
					return x;
				});
			namedValueMock.Setup(m => m.SetNamedValues(It.IsAny<Vault>(), type, ns, It.IsAny<NamedValues>()))
				.Callback((Vault v, MFNamedValueType t, string n, NamedValues namedValues) =>
				{
					output = namedValues[name]?.ToString();
				});

			var rule = new DeclaredConfigurationUpgradeRule
			(
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == ns && m.Name == name && m.NamedValueType == type && m.IsValid() == true),
				Mock.Of<ISingleNamedValueItem>(m => m.Namespace == ns && m.Name == name && m.NamedValueType == type && m.IsValid() == true),
				new Version("0.0"),
				new Version("1.0"),
				upgradeMethod.Method
			)
			{
				NamedValueStorageManager = namedValueMock.Object,
				UpgradeFromType = upgradeMethod.Method.DeclaringType,
				UpgradeToType = upgradeMethod.Method.DeclaringType
			};
			Assert.IsTrue(rule.Execute(Mock.Of<Vault>()));

			Assert.AreEqual
			(
				rule.JsonConvert.Serialize(new { value = "upgraded", Version = "1.0" }),
				output
			);
		}
	}
}
