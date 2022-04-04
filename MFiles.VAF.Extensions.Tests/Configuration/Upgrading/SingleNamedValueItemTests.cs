using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	[TestClass]
	public class SingleNamedValueItemTests
	{
		[TestMethod]
		public void IsValid_False_NullNamespace()
		{
			Assert.IsFalse(new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, null, "world").IsValid());
		}
		[TestMethod]
		public void IsValid_False_EmptyNamespace()
		{
			Assert.IsFalse(new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "", "world").IsValid());
		}
		[TestMethod]
		public void IsValid_False_NullName()
		{
			Assert.IsFalse(new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", null).IsValid());
		}
		[TestMethod]
		public void IsValid_False_EmptyName()
		{
			Assert.IsFalse(new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "").IsValid());
		}
		[TestMethod]
		public void IsValid_True_EmptyName()
		{
			Assert.IsTrue(new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world").IsValid());
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetNamedValues_ThrowsWithNullManager()
		{
			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");
			instance.GetNamedValues(null, Mock.Of<Vault>());
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetNamedValues_ThrowsWithNullVault()
		{
			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");
			instance.GetNamedValues(Mock.Of<INamedValueStorageManager>(), null);
		}

		[TestMethod]
		public void GetNamedValues_HandlesNullFromManager()
		{
			var storageMock = new Mock<INamedValueStorageManager>();
			storageMock
				.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), It.IsAny<MFNamedValueType>(), It.IsAny<string>()))
				.Returns((NamedValues)null)
				.Verifiable();

			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");
			Assert.IsNull(instance.GetNamedValues(storageMock.Object, Mock.Of<Vault>()));

			storageMock.Verify();
		}

		[TestMethod]
		public void GetNamedValues_HandlesMissingItem()
		{
			var namedValues = new NamedValues();
			var storageMock = new Mock<INamedValueStorageManager>();
			storageMock
				.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), It.IsAny<MFNamedValueType>(), It.IsAny<string>()))
				.Returns(namedValues)
				.Verifiable();

			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");
			Assert.IsNull(instance.GetNamedValues(storageMock.Object, Mock.Of<Vault>()));

			storageMock.Verify();
		}

		[TestMethod]
		public void GetNamedValues_ReturnsSingleItem()
		{
			var namedValues = new NamedValues();
			namedValues["world"] = "abcd";

			var storageMock = new Mock<INamedValueStorageManager>();
			storageMock
				.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), It.IsAny<MFNamedValueType>(), It.IsAny<string>()))
				.Returns(namedValues)
				.Verifiable();

			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");

			var output = instance.GetNamedValues(storageMock.Object, Mock.Of<Vault>());
			Assert.IsNotNull(output);
			Assert.AreEqual(1, output.Names.Count);
			Assert.AreEqual("world", output.Names.Cast<string>().First());
			Assert.AreEqual("abcd", output["world"]);

			storageMock.Verify();
		}

		[TestMethod]
		public void GetNamedValues_ReturnsCorrectSingleItem()
		{
			var namedValues = new NamedValues();
			namedValues["hello"] = "dcba";
			namedValues["world"] = "abcd";

			var storageMock = new Mock<INamedValueStorageManager>();
			storageMock
				.Setup(m => m.GetNamedValues(It.IsAny<Vault>(), It.IsAny<MFNamedValueType>(), It.IsAny<string>()))
				.Returns(namedValues)
				.Verifiable();

			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");

			var output = instance.GetNamedValues(storageMock.Object, Mock.Of<Vault>());
			Assert.IsNotNull(output);
			Assert.AreEqual(1, output.Names.Count);
			Assert.AreEqual("world", output.Names.Cast<string>().First());
			Assert.AreEqual("abcd", output["world"]);

			storageMock.Verify();
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetNamedValues_ThrowsWithNullManager()
		{
			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");
			instance.SetNamedValues(null, Mock.Of<Vault>(), new NamedValues());
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetNamedValues_ThrowsWithNullVault()
		{
			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");
			instance.SetNamedValues(Mock.Of<INamedValueStorageManager>(), null, new NamedValues());
		}

		[TestMethod]
		public void SetNamedValues_CallsSetNamedValues()
		{
			var namedValues = new NamedValues();
			namedValues["hello"] = "abcd";

			var storageMock = new Mock<INamedValueStorageManager>();
			storageMock
				.Setup(m => m.SetNamedValues(It.IsAny<Vault>(), It.IsAny<MFNamedValueType>(), It.IsAny<string>(), It.IsAny<NamedValues>()))
				.Callback((Vault vault, MFNamedValueType type, string @namespace, NamedValues nv) =>
				{
					Assert.IsNotNull(nv);
					Assert.AreEqual(1, nv.Names.Count);

					// Even though it was "hello" in the source, it should now be "world" here.
					Assert.AreEqual("world", nv.Names.Cast<string>().First());
					Assert.AreEqual("abcd", nv["world"]);
				})
				.Verifiable();

			var instance = new SingleNamedValueItem(MFNamedValueType.MFConfigurationValue, "hello", "world");

			instance.SetNamedValues(storageMock.Object, Mock.Of<Vault>(), namedValues);

			storageMock.Verify();
		}
	}
}
