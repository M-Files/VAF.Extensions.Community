using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class Owner
		: MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Returns a mock <see cref="Vault"/> that can be used to retrieve data as appropriate.
		/// </summary>
		/// <returns></returns>
		protected override Mock<Vault> GetVaultMock()
		{
			var mock = base.GetVaultMock();

			// Set up the object type operations mock.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock
				.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int id) =>
				{
					var objType = new Mock<ObjType>();
					objType.Setup(ot => ot.OwnerPropertyDef).Returns(999);
					return objType.Object;
				});
			mock
				.SetupGet(m => m.ObjectTypeOperations)
				.Returns(objectTypeOperationsMock.Object);

			return mock;
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Owner"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public void AddsSearchCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the search condition.
			var objID = new ObjID();
			objID.SetIDs(123, 456);
			mfSearchBuilder.Owner(objID);

			// Ensure that there is one item in the collection.
			Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Owner"/>
		/// throws an exception with a null object version.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search by null.
			mfSearchBuilder.Owner((VAF.Common.ObjVerEx)null);
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Owner"/>
		/// throws an exception with a null object version.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search by null.
			mfSearchBuilder.Owner((ObjVer)null);
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Owner"/>
		/// throws an exception with a null object ID.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjIdThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search by null.
			mfSearchBuilder.Owner((ObjID)null);
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Owner"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void SearchConditionIsCorrect()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition.
			var objID = new ObjID();
			objID.SetIDs(123, 456);
			mfSearchBuilder.Owner(objID);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypePropertyValue, condition.Expression.Type);

			// Ensure the property def is correct.
			Assert.AreEqual(999, condition.Expression.DataPropertyValuePropertyDef);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeLookup, condition.TypedValue.DataType);
			Assert.AreEqual(456, condition.TypedValue.GetLookupID());
		}
	}
}
