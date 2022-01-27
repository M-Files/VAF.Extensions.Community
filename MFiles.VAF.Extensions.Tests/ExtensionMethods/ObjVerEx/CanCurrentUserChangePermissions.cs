using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public partial class CanCurrentUserChangePermissions
		: TestBaseWithVaultMock
	{

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsIfNullObjVerEx()
		{
			SessionInfo sessionInfo = new SessionInfo();
			((Common.ObjVerEx) null).CanCurrentUserChangePermissions(sessionInfo);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsIfNullSessionInfo()
		{
			// Set up the vault mock.
			var vaultMock = this.GetVaultMock();

			// Set up the ObjVerEx.
			var objVerEx = new Common.ObjVerEx
			(
				vaultMock.Object,
				0,
				1,
				2
			);

			objVerEx.CanCurrentUserChangePermissions((SessionInfo)null);
		}

		[TestMethod]
		public void CreateTestSessionInfo()
		{
			//SessionInfo sessionInfoFalse = new CanCurrentUserChangePermissionsSessionInfoTest(canCurrentUserChangePermissions: false);
			SessionInfo sessionInfoFalse = new SessionInfo();
			Assert.IsNotNull(sessionInfoFalse);
		}

		[TestMethod]
		public void ReturnsTrueForDefaultObjVerEx()
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

			// Set up the session info objects
			//SessionInfo sessionInfoTrue = new CanCurrentUserChangePermissionsSessionInfoTest(canCurrentUserChangePermissions: true);
			//SessionInfo sessionInfoFalse = new CanCurrentUserChangePermissionsSessionInfoTest(canCurrentUserChangePermissions: false);
			SessionInfo sessionInfoTrue = new SessionInfo();
			SessionInfo sessionInfoFalse = new SessionInfo();

			// TODO: Mock SessionInfo, objVerEx.ACL and objVerEx.Permissions to set it to the right result here

			// Return true if the method returns true
			//Assert.IsTrue(
			//	objVerEx.CanCurrentUserChangePermissions(sessionInfoTrue)
			//);

			// Return false if the method returns false
			//Assert.IsFalse(
			//	objVerEx.CanCurrentUserChangePermissions(sessionInfoFalse)
			//);
		}

	}
}
