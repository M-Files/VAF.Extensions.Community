using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MFiles.VAF.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MFilesAPI;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class CreateCopy
		: TestBaseWithVaultMock
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			((Common.ObjVerEx)null).CreateCopy();
		}

		protected Mock<Vault> GetVaultMockWithObjectOperations
		(
			Func<int, PropertyValues, SourceObjectFiles, bool, bool, AccessControlList, ObjectVersionAndProperties> createNewObjectExCallback
		)
		{
			var vaultMock = base.GetVaultMock();

			// Set up the mock for object operations.
			var objectOperationsMock = new Mock<VaultObjectOperations>();
			objectOperationsMock.Setup
			(
				m => m.CreateNewObjectEx(It.IsAny<int>(), It.IsAny<PropertyValues>(), It.IsAny<SourceObjectFiles>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<AccessControlList>())
			)
			.Returns(createNewObjectExCallback)
			.Verifiable();
			objectOperationsMock.Setup
			(
				m => m.GetObjectPermissions(It.IsAny<ObjVer>())
			)
			.Returns(new Mock<ObjectVersionPermissions>().Object);
			objectOperationsMock.Setup
			(
				m => m.CheckIn(It.IsAny<ObjVer>())
			)
			.Returns((ObjVer objVer) =>
			{
				var objectVersionMock = new Mock<ObjectVersion>();
				objectVersionMock.Setup(m => m.ObjVer).Returns(objVer);
				return objectVersionMock.Object;
			});

			// Set up the mock for object property operations.
			var objectPropertyOperationsMock = new Mock<VaultObjectPropertyOperations>();
			objectPropertyOperationsMock.Setup
			(
				m => m.SetProperties(It.IsAny<ObjVer>(), It.IsAny<PropertyValues>())
			)
			.Returns((ObjVer objVer, PropertyValues propertyValues) =>
			{
				var copyObjectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
				copyObjectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(objVer);
				copyObjectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(propertyValues);
				return copyObjectVersionAndPropertiesMock.Object;
			});

			// Return the object operations instance when needed.
			vaultMock.Setup(m => m.ObjectOperations).Returns(objectOperationsMock.Object);
			vaultMock.Setup(m => m.ObjectPropertyOperations).Returns(objectPropertyOperationsMock.Object);

			return vaultMock;
		}

		[TestMethod]
		public void CopyIsNotNull()
		{
			Vault vault = null;

			// What should our "create new object" method return?
			Func<int, PropertyValues, SourceObjectFiles, bool, bool, AccessControlList, ObjectVersionAndProperties> createNewObjectExCallback = (int objectTypeId, PropertyValues propertyValues, SourceObjectFiles sourceObjectFiles, bool sfd, bool checkIn, AccessControlList accessControlList) =>
			{
				var copyObjVer = new ObjVer();
				copyObjVer.SetIDs(objectTypeId, 123, 1);
				var copyObjectVersionMock = new Mock<ObjectVersion>();
				copyObjectVersionMock.Setup(m => m.ObjVer).Returns(copyObjVer);
				var copyObjectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
				copyObjectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(copyObjVer);
				copyObjectVersionAndPropertiesMock.Setup(m => m.VersionData).Returns(copyObjectVersionMock.Object);
				copyObjectVersionAndPropertiesMock.Setup(m => m.Vault).Returns(vault);
				copyObjectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(propertyValues);
				return copyObjectVersionAndPropertiesMock.Object;
			};

			// Create our mock objects.
			var vaultMock = this.GetVaultMockWithObjectOperations
			(
				createNewObjectExCallback
			);
			vault = vaultMock.Object;

			// Create our source object.
			var objVer = new ObjVer();
			objVer.SetIDs(0, 123, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.Setup(m => m.ObjVer).Returns(objVer);
			var objectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
			objectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(objVer);
			objectVersionAndPropertiesMock.Setup(m => m.VersionData).Returns(objectVersionMock.Object);
			objectVersionAndPropertiesMock.Setup(m => m.Vault).Returns(vaultMock.Object);
			objectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(new PropertyValues());
			var objVerExSource = new VAF.Common.ObjVerEx
			(
				vaultMock.Object,
				objectVersionAndPropertiesMock.Object
			);

			// Execute.
			var copy = objVerExSource.CreateCopy();

			// Verify we got our callback.
			vaultMock.Verify();

			// Target should not be null.
			Assert.IsNotNull(copy);
			Assert.IsNotNull(copy.Properties);
		}

		//[TestMethod]
		//public void SourcePropertiesCopied()
		//{
		//	Vault vault = null;

		//	// What should our "create new object" method return?
		//	Func<int, PropertyValues, SourceObjectFiles, bool, bool, AccessControlList, ObjectVersionAndProperties> createNewObjectExCallback = (int objectTypeId, PropertyValues propertyValues, SourceObjectFiles sourceObjectFiles, bool sfd, bool checkIn, AccessControlList accessControlList) =>
		//	{
		//		var copyObjVer = new ObjVer();
		//		copyObjVer.SetIDs(objectTypeId, 123, 1);
		//		var copyObjectVersionMock = new Mock<ObjectVersion>();
		//		copyObjectVersionMock.Setup(m => m.ObjVer).Returns(copyObjVer);
		//		var copyObjectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
		//		copyObjectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(copyObjVer);
		//		copyObjectVersionAndPropertiesMock.Setup(m => m.VersionData).Returns(copyObjectVersionMock.Object);
		//		copyObjectVersionAndPropertiesMock.Setup(m => m.Vault).Returns(vault);
		//		copyObjectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(propertyValues);
		//		return copyObjectVersionAndPropertiesMock.Object;
		//	};

		//	// Create our mock objects.
		//	var vaultMock = this.GetVaultMockWithObjectOperations
		//	(
		//		createNewObjectExCallback
		//	);
		//	vault = vaultMock.Object;

		//	// Create our source object.
		//	var objVer = new ObjVer();
		//	objVer.SetIDs(0, 123, 1);
		//	var objectVersionMock = new Mock<ObjectVersion>();
		//	objectVersionMock.Setup(m => m.ObjVer).Returns(objVer);
		//	var objectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
		//	objectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(objVer);
		//	objectVersionAndPropertiesMock.Setup(m => m.VersionData).Returns(objectVersionMock.Object);
		//	objectVersionAndPropertiesMock.Setup(m => m.Vault).Returns(vaultMock.Object);
		//	var pvs = new PropertyValues()
		//	{
		//		{
		//			-1,
		//			new PropertyValue(){ PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass }
		//		},
		//		{
		//			-1,
		//			new PropertyValue(){ PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle }
		//		}
		//	};
		//	pvs[1].TypedValue.SetValue(MFDataType.MFDatatypeLookup, 123);
		//	pvs[2].TypedValue.SetValue(MFDataType.MFDatatypeText, "hello world");
		//	objectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(pvs);
		//	var objVerExSource = new VAF.Common.ObjVerEx
		//	(
		//		vaultMock.Object,
		//		objectVersionAndPropertiesMock.Object
		//	);

		//	// Execute.
		//	var copy = objVerExSource.CreateCopy();

		//	// Verify we got our callback.
		//	vaultMock.Verify();

		//	// Properties should be correct.
		//	Assert.AreEqual(2, copy.Properties.Count);
		//	Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass, copy.Properties[1].PropertyDef);
		//	Assert.AreEqual(123, copy.Properties[1].TypedValue.GetLookupID());
		//	Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle, copy.Properties[2].PropertyDef);
		//	Assert.AreEqual("hello world", copy.Properties[2].TypedValue.DisplayValue);
		//}

	}
}
