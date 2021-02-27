using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;

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

		// TODO Think about further test methods + Question how lookups to object types were handled

	}
}
