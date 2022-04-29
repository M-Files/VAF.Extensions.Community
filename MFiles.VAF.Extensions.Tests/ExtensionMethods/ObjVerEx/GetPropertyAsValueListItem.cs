using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.Runtime.InteropServices;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class GetPropertyAsValueListItem
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
			((Common.ObjVerEx) null).GetPropertyAsValueListItem((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThrowsIfPropDefIdNegative()
		{
			DefaultObjVerEx.GetPropertyAsValueListItem((int) -1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfPropDoesNotExist()
		{
			// max int should not point to an existing property definition
			DefaultObjVerEx.GetPropertyAsValueListItem(int.MaxValue);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfPropIsNotBasedOnValueList()
		{
			// max int should not point to an existing property definition
			DefaultObjVerEx.GetPropertyAsValueListItem((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefDeleted);
		}

		[TestMethod]
		public void ReturnsNullIfPropertyNotInCollection()
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
						DataType = MFDataType.MFDatatypeLookup,
						BasedOnValueList = true,
						ValueList = valueListId
					};
				})
				.Verifiable();

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

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
			Assert.IsNull(objVerEx.GetPropertyAsValueListItem(propertyDefId));
		}

		[TestMethod]
		public void ReturnsNullIfPropertyIsNull()
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
						DataType = MFDataType.MFDatatypeLookup,
						BasedOnValueList = true,
						ValueList = valueListId
					};
				})
				.Verifiable();

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

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
				pv.TypedValue.SetValueToNULL(MFDataType.MFDatatypeLookup);
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			Assert.IsNull(objVerEx.GetPropertyAsValueListItem(propertyDefId));
		}

		/// <summary>
		/// Tests that the COMException raised by the underlying API is bubbled to the caller.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(COMException))]
		public void ThrowsIfValueListItemIsNotFound()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;
			var valueListItemId = 1;

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
						DataType = MFDataType.MFDatatypeLookup,
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
					Assert.AreEqual(valueListItemId, vliid);

					// Throw (destroyed).
					throw new COMException();

				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ValueListItemOperations).Returns(valueListItemsMock.Object);

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
				pv.TypedValue.SetValue(MFDataType.MFDatatypeLookup, valueListItemId);
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			objVerEx.GetPropertyAsValueListItem(propertyDefId);
		}

		[TestMethod]
		public void ReturnsValueListItemIfIsDeleted()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;
			var valueListItemId = 1;

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
						DataType = MFDataType.MFDatatypeLookup,
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
					Assert.AreEqual(valueListItemId, vliid);

					// Deleted is okay.
					return Mock.Of<ValueListItem>(i => i.ID == valueListItemId && i.ValueListID == valueListId && i.Deleted == true);

				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ValueListItemOperations).Returns(valueListItemsMock.Object);

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
				pv.TypedValue.SetValue(MFDataType.MFDatatypeLookup, valueListItemId);
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			var item = objVerEx.GetPropertyAsValueListItem(propertyDefId);
			Assert.IsNotNull(item);
			Assert.AreEqual(valueListItemId, item.ID);
			Assert.AreEqual(valueListId, item.ValueListID);
			Assert.IsTrue(item.Deleted);
		}

		[TestMethod]
		public void ReturnsCorrectValueListItem()
		{
			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;
			var valueListItemId = 1;

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
						DataType = MFDataType.MFDatatypeLookup,
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
					Assert.AreEqual(valueListItemId, vliid);

					// Return an undeleted item.
					return Mock.Of<ValueListItem>(i => i.ID == valueListItemId && i.ValueListID == valueListId && i.Deleted == false);

				});

			// Mock the vault.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);
			vaultMock.Setup(m => m.ValueListItemOperations).Returns(valueListItemsMock.Object);

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
				pv.TypedValue.SetValue(MFDataType.MFDatatypeLookup, valueListItemId);
				properties.Add(1, pv);
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Use the method.
			var item = objVerEx.GetPropertyAsValueListItem(propertyDefId);
			Assert.IsNotNull(item);
			Assert.AreEqual(valueListItemId, item.ID);
			Assert.AreEqual(valueListId, item.ValueListID);
			Assert.IsFalse(item.Deleted);
		}

	}
}
