using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class GetLookupIDs : TestBaseWithVaultMock
	{
		[TestMethod]
		[DataRow(0, 1, new int[] { 1, 2, 3 })]
		[DataRow(0, 1, new int[] { 1 })]
		[DataRow(0, 1, new int[] { })]
		[DataRow(0, 1, null)]
		public void GetLookupIDsWithValidLookups
			(
			int objectType,
			int objectID,
			int[] inputIDs
			)
		{

			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;
			// Mock the vault.
			var vaultMock = this.GetVaultMock();

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
						BasedOnValueList = false,
						ValueList = valueListId
					};
				})
				.Verifiable();

			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(objectType, objectID, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();
			{
				// If the inputIDs is null, don't add the property to the collection at all
				if (inputIDs != null)
				{
					var pv = new PropertyValue();
					pv.PropertyDef = propertyDefId;
					pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, inputIDs);
					properties.Add(-1, pv);
				}
			}



			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);



			// Assert.
			// If the inputIDs isn't null, then we can compare the inputIDs to the GetLookupIDs return value
			if (inputIDs != null)
			{
				Assert.AreEqual(inputIDs.Length, objVerEx.GetLookupIDs(propertyDefId).Count());
				CollectionAssert.AreEquivalent(inputIDs.ToList(), objVerEx.GetLookupIDs(propertyDefId));
			}
			else
			{
				// When the inputIDs is null, then the GetLookupIDs list should be empty because the property isn'n in the collection
				Assert.AreEqual(0, objVerEx.GetLookupIDs(propertyDefId).Count());

			}

		}

		[TestMethod]
		[DataRow(0, 1, new int[] { 1, 2, 3 })]
		[DataRow(0, 1, new int[] { 1 })]
		[DataRow(0, 1, new int[] { })]
		[DataRow(0, 1, null)]
		public void GetLookupIDsWithDeletedLookupsIgnored
		(
		int objectType,
		int objectID,
		int[] inputIDs
		)
		{

			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;
			// Mock the vault.
			var vaultMock = this.GetVaultMock();

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
						BasedOnValueList = false,
						ValueList = valueListId
					};
				})
				.Verifiable();

			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(objectType, objectID, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();
			{
				if (inputIDs != null)
				{
					// Mark each lookup as deleted
					Lookups lks = new Lookups();
					foreach (int inputID in inputIDs)
					{
						Lookup lookup = new Lookup
						{
							Item = inputID,
							Deleted = true
						};
						lks.Add(-1, lookup);
					}

					if (inputIDs != null)
					{
						var pv = new PropertyValue();
						pv.PropertyDef = propertyDefId;
						pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, lks);
						properties.Add(-1, pv);
					}
				}
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Assert.
			// Every lookup is deleted and we don't read those. Therefore the count is 0.
			Assert.AreEqual(0, objVerEx.GetLookupIDs(propertyDefId, false).Count());
		}

		[TestMethod]
		[DataRow(0, 1, new int[] { 1, 2, 3 })]
		[DataRow(0, 1, new int[] { 1 })]
		[DataRow(0, 1, new int[] { })]
		[DataRow(0, 1, null)]
		public void GetLookupIDsWithDeletedLookupsReturned
		(
		int objectType,
		int objectID,
		int[] inputIDs
		)
		{

			// IDs used.
			var propertyDefId = 1234;
			var valueListId = 123;
			// Mock the vault.
			var vaultMock = this.GetVaultMock();

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
						BasedOnValueList = false,
						ValueList = valueListId
					};
				})
				.Verifiable();

			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

			// Set up the data for the ObjVerEx.
			var objVer = new ObjVer();
			objVer.SetIDs(objectType, objectID, 1);
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer)
				.Returns(objVer);
			var properties = new PropertyValues();
			{
				if (inputIDs != null)
				{
					// Mark each lookup as deleted
					Lookups lks = new Lookups();
					foreach (int inputID in inputIDs)
					{
						Lookup lookup = new Lookup
						{
							Item = inputID,
							Deleted = true
						};
						lks.Add(-1, lookup);
					}

					// If the inputIDs is null, don't add the property to the collection at all
					if (inputIDs != null)
					{
						var pv = new PropertyValue();
						pv.PropertyDef = propertyDefId;
						pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, lks);
						properties.Add(-1, pv);
					}
				}
			}

			// Create the ObjVerEx.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Assert.
			if (inputIDs != null)
			{
				// Every lookup is deleted and we'll read those. Therefore the count and list should match the input .
				Assert.AreEqual(inputIDs.Length, objVerEx.GetLookupIDs(propertyDefId, true).Count());
				CollectionAssert.AreEquivalent(inputIDs.ToList(), objVerEx.GetLookupIDs(propertyDefId, true));
			}
			else
			{
				// When the inputIDs is null, then the GetLookupIDs list should be empty because the property isn't in the collection
				Assert.AreEqual(0, objVerEx.GetLookupIDs(propertyDefId, true).Count());

			}
		}
	}
}
