using MFiles.VAF.Configuration;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class GetOwner
		: TestBaseWithVaultMock
	{

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsIfNullObjVerEx()
		{
			((Common.ObjVerEx) null).GetOwner();
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfObjectTypeHasNoOwner()
		{
			// Set up our configuration.
			var childObjectType = 102;

			// Set up the object type operations mock.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock
				.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int objectTypeId) =>
				{
					// Is it a child?
					if (objectTypeId == childObjectType)
					{
						return new ObjType()
						{
							ID = childObjectType,
							NamePlural = "Children",
							NameSingular = "Child",
							HasOwnerType = false
						};
					}

					// Unexpected object type.
					throw new InvalidOperationException("Unexpected object type");
				});

			// Set up the vault mock.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(v => v.ObjectTypeOperations).Returns(objectTypeOperationsMock.Object);

			// Set up the ObjVerEx.
			var objVerEx = new Common.ObjVerEx
			(
				vaultMock.Object,
				childObjectType,
				1,
				2
			);

			// Run.
			objVerEx.GetOwner();
		}

		[TestMethod]
		public void ReturnsParentObjectIfDataCorrect()
		{
			// Set up our configuration.
			var childObjectType = 102;
			var parentObjectType = 101;
			var ownerPropertyDef = 1020;

			// Unfortunately this requires mocking deep into the vault because we can't override the default ObjVerEx behaviour for GetDirectReference...

			// Set up the property definitions mock.
			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock
				.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDefId) => new PropertyDef()
				{
					BasedOnValueList = true,
					ValueList = parentObjectType,
					ID = propertyDefId,
					DataType = MFDataType.MFDatatypeLookup
				});

			// Set up the object type operations mock.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock
				.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int objectTypeId) =>
				{
					// Is it a child?
					if (objectTypeId == childObjectType)
					{
						return new ObjType()
						{
							ID = childObjectType,
							NamePlural = "Children",
							NameSingular = "Child",
							HasOwnerType = true,
							OwnerType = parentObjectType,
							RealObjectType = true
						};
					}

					// Is it a parent?
					if (objectTypeId == parentObjectType)
					{
						return new ObjType()
						{
							ID = parentObjectType,
							NamePlural = "Parents",
							NameSingular = "Parent",
							HasOwnerType = false,
							OwnerPropertyDef = ownerPropertyDef,
							RealObjectType = true
						};
					}

					// Unexpected object type.
					throw new InvalidOperationException("Unexpected object type");
				});

			// Set up the vault mock.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(v => v.ObjectTypeOperations).Returns(objectTypeOperationsMock.Object);
			vaultMock.Setup(v => v.PropertyDefOperations).Returns(propertyDefOperationsMock.Object);

			// Set up the expected object.
			var expectedParent = new Common.ObjVerEx(vaultMock.Object, parentObjectType, 1234, -1);

			// Set up the ObjVerEx.
			var objectVersion = new Mock<ObjectVersion>();
			objectVersion
				.Setup(m => m.ObjVer)
				.Returns(() =>
				{
					var objVer = new ObjVer();
					objVer.SetIDs(childObjectType, 1, 2);
					return objVer;
				});
			var objVerEx = new Common.ObjVerEx
			(
				vaultMock.Object,
				objectVersion.Object,
				new PropertyValues()
			);

			// Set up the owner property.
			{
				var ownerProperty = new PropertyValue()
				{
					PropertyDef = ownerPropertyDef
				};
				ownerProperty.TypedValue.SetValue(MFDataType.MFDatatypeLookup, new Lookup()
				{
					Item = 1234,
					Deleted = false,
					ObjectType = parentObjectType
				});
				objVerEx.Properties.Add(-1, ownerProperty);
			}

			// Run.
			var owner = objVerEx.GetOwner();

			// Did we get the right object back?
			Assert.IsNotNull(owner);
			Assert.AreEqual(expectedParent.Type, owner.Type);
			Assert.AreEqual(expectedParent.ID, owner.ID);
		}

	}
}
