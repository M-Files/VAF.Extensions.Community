using MFiles.VAF.Extensions.ExtensionMethods;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class ExpandPropertyConcatenation
		: TestBaseWithVaultMock
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			((Common.ObjVerEx)null).ExpandPropertyConcatenation("hello world");
		}

		[TestMethod]
		public void NullStringReturnsEmptyString()
		{
			Assert.AreEqual(string.Empty, new Common.ObjVerEx().ExpandPropertyConcatenation(null));
		}

		[TestMethod]
		public void BlankStringReturnsEmptyString()
		{
			Assert.AreEqual(string.Empty, new Common.ObjVerEx().ExpandPropertyConcatenation(""));
		}

		[TestMethod]
		public void SingleInternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123 world", objVerEx.ExpandPropertyConcatenation("hello %INTERNALID% world"));
		}

		[TestMethod]
		public void MultipleInternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123 world 123", objVerEx.ExpandPropertyConcatenation("hello %INTERNALID% world %INTERNALID%"));
		}

		[TestMethod]
		public void SingleExternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123ABCDEF123 world", objVerEx.ExpandPropertyConcatenation("hello %EXTERNALID% world"));
		}

		[TestMethod]
		public void MultipleExternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123ABCDEF123 world 123ABCDEF123", objVerEx.ExpandPropertyConcatenation("hello %EXTERNALID% world %EXTERNALID%"));
		}

		[TestMethod]
		public void PropertyIDWithNumber()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock
			(
				vaultMock,
				propertyValues: new Tuple<int, MFDataType, object>(0, MFDataType.MFDatatypeText, "hello world")
			);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("document is called: hello world", objVerEx.ExpandPropertyConcatenation("document is called: %PROPERTY_0%"));
		}

		[TestMethod]
		public void PropertyIDWithAlias()
		{
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("MF.PD.Title", 0)
			);
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock
			(
				vaultMock,
				propertyValues: new Tuple<int, MFDataType, object>(0, MFDataType.MFDatatypeText, "hello world")
			);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("document is called: hello world", objVerEx.ExpandPropertyConcatenation("document is called: %PROPERTY_{MF.PD.Title}%"));
		}

		//[TestMethod]
		//public void IndirectProperties()
		//{
		//	var vaultMock = this.GetVaultMock
		//	(
		//		new Tuple<string, int>("MF.PD.Company", 123),
		//		new Tuple<string, int>("Name", 0)
		//	);
		//	var related2 = this.GetObjectVersionAndPropertiesMock
		//	(
		//		vaultMock,
		//		propertyValues: new Tuple<int, MFDataType, object>(0, MFDataType.MFDatatypeText, "hello world")
		//	);
		//	var related1 = this.GetObjectVersionAndPropertiesMock
		//	(
		//		vaultMock,
		//		propertyValues: new Tuple<int, MFDataType, object>(0, MFDataType.MFDatatypeText, "hello world")
		//	);
		//	var source = this.GetObjectVersionAndPropertiesMock
		//	(
		//		vaultMock,
		//		propertyValues: new []
		//		{
		//			new Tuple<int, MFDataType, object>(123, MFDataType.MFDatatypeLookup, "hello world")
		//		}
		//	);
		//	var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
		//	Assert.AreEqual("document is called: hello world", objVerEx.ExpandPropertyConcatenation("document is called: %PROPERTY_{MF.PD.Company}.PROPERTY_456.PROPERTY_{Name}%"));
		//}

		protected virtual Mock<Vault> GetVaultMock(params Tuple<string, int>[] propertyDefinitions)
		{
			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock
				.Setup(o => o.GetPropertyDefIDByAlias(It.IsAny<string>()))
				.Returns((string alias) =>
				{
					return propertyDefinitions
						.FirstOrDefault(t => t.Item1 == alias)?
						.Item2 ?? -1;
				});

			var vault = base.GetVaultMock();
			vault.Setup(v => v.PropertyDefOperations).Returns(propertyDefOperationsMock.Object);
			return vault;
		}

		protected virtual Mock<ObjectVersionAndProperties> GetObjectVersionAndPropertiesMock
			(
			Mock<Vault> vaultMock,
			int objectTypeId = 0,
			int objectId = 123,
			int version = 1,
			string externalId = "123ABCDEF123",
			params Tuple<int, MFDataType, object>[] propertyValues
			)
		{
			var objectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
			objectVersionAndPropertiesMock
				.Setup(o => o.ObjVer)
				.Returns(() =>
				{
					var objVer = new ObjVer();
					objVer.SetIDs(objectTypeId, objectId, version);
					return objVer;
				});
			objectVersionAndPropertiesMock
				.Setup(o => o.VersionData)
				.Returns(() =>
				{
					var data = new Mock<ObjectVersion>();
					data.Setup(o => o.DisplayIDAvailable).Returns(true);
					data.Setup(o => o.DisplayID).Returns(externalId);
					return data.Object;
				});
			objectVersionAndPropertiesMock
				.Setup(o => o.Properties)
				.Returns(() =>
				{
					var data = new PropertyValues();
					foreach(var tuple in propertyValues ?? new Tuple<int, MFDataType, object>[0])
					{
						var pv = new PropertyValue();
						pv.PropertyDef = tuple.Item1;
						pv.Value.SetValue(tuple.Item2, tuple.Item3);
						data.Add(-1, pv);
					}
					return data;
				});
			return objectVersionAndPropertiesMock;
		}
	}
}
