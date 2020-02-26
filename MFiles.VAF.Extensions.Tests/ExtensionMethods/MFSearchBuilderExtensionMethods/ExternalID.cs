using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class ExternalId
		: MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.ExternalId"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public void AddsSearchCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the search condition for the external ID.
			mfSearchBuilder.ExternalId("hello-world");

			// Ensure that there is one item in the collection.
			Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullExternalIdThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search by null.
			mfSearchBuilder.ExternalId(null);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.ExternalId"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void SearchConditionIsCorrect()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the external ID.
			mfSearchBuilder.ExternalId("hello-world");

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypeStatusValue, condition.Expression.Type);

			// Ensure the status value is correct.
			Assert.AreEqual(MFStatusType.MFStatusTypeExtID, condition.Expression.DataStatusValueType);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual("hello-world", condition.TypedValue.Value as string);
		}
	}
}
