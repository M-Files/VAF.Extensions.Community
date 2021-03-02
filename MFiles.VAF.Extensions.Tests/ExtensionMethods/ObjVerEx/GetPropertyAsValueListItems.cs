using MFiles.VAF.Configuration;

using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.Runtime.InteropServices;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class GetPropertyAsValueListItems
		: TestBaseWithVaultMock
	{
		/// <summary>
		/// Default Vault mock object for this test class
		/// </summary>
		protected Vault MockVault;

		/// <summary>
		/// Default ObjVerEx object for this test class
		/// </summary>
		protected Common.ObjVerEx DefaultObjVerEx;

		[TestInitialize]
		public void InitializeData()
		{
			// Get the vault mock with metadata and populate it if needed.
			var mock = new Mock<Vault>
			{
				DefaultValue = DefaultValue.Mock
			};
			MockVault = mock.Object;

			// Create the ObjVerEx and set the properties.
			DefaultObjVerEx = new Common.ObjVerEx(MockVault, (int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument, id: 1, version: 1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsIfNullObjVerEx()
		{
			((Common.ObjVerEx) null).GetPropertyAsValueListItems((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThrowsIfPropDefIdNegative()
		{
			DefaultObjVerEx.GetPropertyAsValueListItems((int) -1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfPropDoesNotExist()
		{
			// max int should not point to an existing property definition
			DefaultObjVerEx.GetPropertyAsValueListItems(int.MaxValue);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfPropIsNotBasedOnValueList()
		{
			DefaultObjVerEx.GetPropertyAsValueListItems((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefDeleted);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfPropIsNotMultiSelectLookup()
		{
			DefaultObjVerEx.GetPropertyAsValueListItems((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow);
		}

		[TestMethod]
		public void ReturnsEmptyCollectionIfPropertyNotInCollection()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;

			// Mock the property definition operations object.
			var propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefinitionsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDef) =>
				{
					// Ensure that the property definition Id is correct.
					Assert.AreEqual(propertyDefId, propertyDef);

					// Return a property definition that is not based on a value list.
					return new PropertyDef()
					{
						ID = propertyDefId,
						DataType = MFDataType.MFDatatypeMultiSelectLookup,
						BasedOnValueList = true,
						ValueList = valueListId
					};
				})
				.Verifiable();

			// Mock the object type operations object.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int objectTypeId) =>
				{
					return new ObjType()
					{
						ID = 101
					};
				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ObjectTypeOperations).Returns(objectTypeOperationsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			var items = objVerEx.GetPropertyAsValueListItems(propertyDefId);
			Assert.IsNotNull(items);
			Assert.AreEqual(0, items.Count);
		}

		[TestMethod]
		public void ReturnsEmptyCollectionIfPropertyIsNull()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;

			// Mock the property definition operations object.
			var propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefinitionsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDef) =>
				{
					// Ensure that the property definition Id is correct.
					Assert.AreEqual(propertyDefId, propertyDef);

					// Return a property definition that is not based on a value list.
					return new PropertyDef()
					{
						ID = propertyDefId,
						DataType = MFDataType.MFDatatypeMultiSelectLookup,
						BasedOnValueList = true,
						ValueList = valueListId
					};
				})
				.Verifiable();

			// Mock the object type operations object.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int objectTypeId) =>
				{
					return new ObjType()
					{
						ID = 101
					};
				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ObjectTypeOperations).Returns(objectTypeOperationsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();
			{
				var pv = new PropertyValue();
				pv.PropertyDef = propertyDefId;
				pv.TypedValue.SetValueToNULL(MFDataType.MFDatatypeMultiSelectLookup);
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			var items = objVerEx.GetPropertyAsValueListItems(propertyDefId);
			Assert.IsNotNull(items);
			Assert.AreEqual(0, items.Count);
		}

		[TestMethod]
		public void ReturnsCorrectValueListItems_AllAvailable()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;

			// Mock the property definition operations object.
			var propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefinitionsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDef) =>
				{
					// Ensure that the property definition Id is correct.
					Assert.AreEqual(propertyDefId, propertyDef);

					// Return a property definition that is not based on a value list.
					return new PropertyDef()
					{
						ID = propertyDefId,
						DataType = MFDataType.MFDatatypeMultiSelectLookup,
						BasedOnValueList = true,
						ValueList = valueListId
					};
				})
				.Verifiable();

			// Mock the value list item operations object.
			var valueListItemsMock = new Mock<VaultValueListItemOperations>();
			valueListItemsMock.Setup(m => m.GetValueListItemByID(It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int vlid, int vliid) =>
				{
					// Did we get the right list and item ids?
					Assert.AreEqual(valueListId, vlid);

					// Return an undeleted item.
					return Mock.Of<ValueListItem>(i => i.ID == vliid && i.ValueListID == valueListId && i.Deleted == false);

				});

			// Mock the object type operations object.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int objectTypeId) =>
				{
					return new ObjType()
					{
						ID = 101
					};
				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ValueListItemOperations).Returns(valueListItemsMock.Object);
			vaultMock.Setup(m => m.ObjectTypeOperations).Returns(objectTypeOperationsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();
			{
				var pv = new PropertyValue();
				pv.PropertyDef = propertyDefId;
				pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, new object[]{
					123,
					456,
					789
				});
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			var items = objVerEx.GetPropertyAsValueListItems(propertyDefId);
			Assert.IsNotNull(items);
			Assert.AreEqual(3, items.Count);
			Assert.AreEqual(123, items[0].ID);
			Assert.AreEqual(456, items[1].ID);
			Assert.AreEqual(789, items[2].ID);
			Assert.IsFalse(items[0].Deleted);
			Assert.IsFalse(items[1].Deleted);
			Assert.IsFalse(items[2].Deleted);
		}

		[TestMethod]
		public void ReturnsCorrectValueListItems_OneDeleted()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;

			// Mock the property definition operations object.
			var propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefinitionsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDef) =>
				{
					// Ensure that the property definition Id is correct.
					Assert.AreEqual(propertyDefId, propertyDef);

					// Return a property definition that is not based on a value list.
					return new PropertyDef()
					{
						ID = propertyDefId,
						DataType = MFDataType.MFDatatypeMultiSelectLookup,
						BasedOnValueList = true,
						ValueList = valueListId
					};
				})
				.Verifiable();

			// Mock the value list item operations object.
			var valueListItemsMock = new Mock<VaultValueListItemOperations>();
			valueListItemsMock.Setup(m => m.GetValueListItemByID(It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int vlid, int vliid) =>
				{
					// Did we get the right list and item ids?
					Assert.AreEqual(valueListId, vlid);

					// Return an undeleted item.
					return Mock.Of<ValueListItem>
					(
						i => i.ID == vliid
							&& i.ValueListID == valueListId
							&& i.Deleted == (789 == vliid)
					);

				});

			// Mock the object type operations object.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int objectTypeId) =>
				{
					return new ObjType()
					{
						ID = 101
					};
				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ValueListItemOperations).Returns(valueListItemsMock.Object);
			vaultMock.Setup(m => m.ObjectTypeOperations).Returns(objectTypeOperationsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();
			{
				var pv = new PropertyValue();
				pv.PropertyDef = propertyDefId;
				pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, new object[]{
					123,
					456,
					789
				});
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			var items = objVerEx.GetPropertyAsValueListItems(propertyDefId);
			Assert.IsNotNull(items);
			Assert.AreEqual(3, items.Count);
			Assert.AreEqual(123, items[0].ID);
			Assert.AreEqual(456, items[1].ID);
			Assert.AreEqual(789, items[2].ID);
			Assert.IsFalse(items[0].Deleted);
			Assert.IsFalse(items[1].Deleted);
			Assert.IsTrue(items[2].Deleted);
		}

		[TestMethod]
		public void ReturnsCorrectValueListItems_OneInvalid()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;

			// Mock the property definition operations object.
			var propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefinitionsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDef) =>
				{
					// Ensure that the property definition Id is correct.
					Assert.AreEqual(propertyDefId, propertyDef);

					// Return a property definition that is not based on a value list.
					return new PropertyDef()
					{
						ID = propertyDefId,
						DataType = MFDataType.MFDatatypeMultiSelectLookup,
						BasedOnValueList = true,
						ValueList = valueListId
					};
				})
				.Verifiable();

			// Mock the value list item operations object.
			var valueListItemsMock = new Mock<VaultValueListItemOperations>();
			valueListItemsMock.Setup(m => m.GetValueListItemByID(It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int vlid, int vliid) =>
				{
					// Did we get the right list and item ids?
					Assert.AreEqual(valueListId, vlid);

					if (vliid == 456)
						throw new COMException();

					// Return an undeleted item.
					return Mock.Of<ValueListItem>
					(
						i => i.ID == vliid
							&& i.ValueListID == valueListId
							&& i.Deleted == false
					);

				});

			// Mock the object type operations object.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int objectTypeId) =>
				{
					return new ObjType()
					{
						ID = 101
					};
				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ValueListItemOperations).Returns(valueListItemsMock.Object);
			vaultMock.Setup(m => m.ObjectTypeOperations).Returns(objectTypeOperationsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();
			{
				var pv = new PropertyValue();
				pv.PropertyDef = propertyDefId;
				pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, new object[]{
					123,
					456,
					789
				});
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			var items = objVerEx.GetPropertyAsValueListItems(propertyDefId);
			Assert.IsNotNull(items);
			Assert.AreEqual(2, items.Count);
			Assert.AreEqual(123, items[0].ID);
			Assert.AreEqual(789, items[1].ID);
			Assert.IsFalse(items[0].Deleted);
			Assert.IsFalse(items[1].Deleted);
		}

	}
}
