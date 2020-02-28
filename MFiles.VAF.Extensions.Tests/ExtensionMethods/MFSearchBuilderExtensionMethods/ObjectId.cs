using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class ObjectId
		: MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.ObjectId"/>
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
			mfSearchBuilder.ObjectId(1);

			// Ensure that there is one item in the collection.
			Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativeObjectIdThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search.
			mfSearchBuilder.ObjectId(-1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ZeroObjectIdThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search.
			mfSearchBuilder.ObjectId(0);
		}
		
		/// <summary>
		/// Handles that providing an invalid <see cref="MFConditionType"/> throws an exception.
		/// </summary>
		/// <param name="objectId">The object Id to test.</param>
		/// <param name="conditionType">The invalid condition type</param>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		[DataRow(123, MFConditionType.MFConditionTypeContains)]
		[DataRow(123, MFConditionType.MFConditionTypeContainsAnyBitwise)]
		[DataRow(123, MFConditionType.MFConditionTypeDoesNotContain)]
		[DataRow(123, MFConditionType.MFConditionTypeDoesNotContainAnyBitwise)]
		[DataRow(123, MFConditionType.MFConditionTypeDoesNotMatchWildcardPattern)]
		[DataRow(123, MFConditionType.MFConditionTypeDoesNotStartWith)]
		[DataRow(123, MFConditionType.MFConditionTypeMatchesWildcardPattern)]
		[DataRow(123, MFConditionType.MFConditionTypeStartsWith)]
		[DataRow(123, MFConditionType.MFConditionTypeStartsWithAtWordBoundary)]
		public void UnhandledConditionTypeThrows
		(
			int objectId,
			MFConditionType conditionType
		)
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition.
			mfSearchBuilder.ObjectId(objectId, conditionType);
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.ObjectIdSegment"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DataRow(123, MFConditionType.MFConditionTypeEqual)]
		[DataRow(123, MFConditionType.MFConditionTypeNotEqual)]
		[DataRow(123, MFConditionType.MFConditionTypeLessThan)]
		[DataRow(123, MFConditionType.MFConditionTypeLessThanOrEqual)]
		[DataRow(123, MFConditionType.MFConditionTypeGreaterThan)]
		[DataRow(123, MFConditionType.MFConditionTypeGreaterThanOrEqual)]
		public void SearchConditionIsCorrect
			(
			int objectId,
			MFConditionType conditionType
			)
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition.
			mfSearchBuilder.ObjectId(objectId, conditionType);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(conditionType, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypeStatusValue, condition.Expression.Type);

			// Ensure the status value is correct.
			Assert.AreEqual(MFStatusType.MFStatusTypeObjectID, condition.Expression.DataStatusValueType);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeInteger, condition.TypedValue.DataType);
			Assert.AreEqual(objectId, condition.TypedValue.Value as int?);
		}
	}
}
