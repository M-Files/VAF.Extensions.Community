using System;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class SegmentedSearch
		: MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.ForEachEx"/>
		/// executes a segmented search.
		/// </summary>
		[TestMethod]
		public void SegmentedSearchForEachEx_Empty()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Count up the items in the vault (should be none!).
			var count1 = 0;
			var count2 = mfSearchBuilder.ForEachEx( obj => { count1 += 1; } );

			// Ensure that nothing is returned as there are no items.
			Assert.AreEqual(0, count1);
			Assert.AreEqual(0, count2);
		}

		[TestMethod]
		public void SegmentedCount()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			var count = mfSearchBuilder.SegmentedCount();

			// Ensure that nothing is returned as there are no items.
			Assert.AreEqual(0, count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullMethodThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute with null delegate.
			mfSearchBuilder.ForEachEx(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativeStartSegmentThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				startSegment: -1
			);
		}

		[TestMethod]
		public void ZeroStartSegmentDoesNotThrow()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the extension.
			mfSearchBuilder.ObjType(0);

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				startSegment: 0
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativeSegmentLimitThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				segmentLimit: -1
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ZeroSegmentLimitThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				segmentLimit: 0
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativeSegmentSizeThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				segmentSize: -1
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ZeroSegmentSizeThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				segmentSize: 0
			);
		}

		[TestMethod]
		public void PositiveSegmentSizeDoesNotThrow()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the extension.
			mfSearchBuilder.ObjType(0);

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				segmentSize: 1
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativeTimeoutThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				searchTimeoutInSeconds: -1
			);
		}

		[TestMethod]
		public void ZeroTimeoutDoesNotThrow()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the extension.
			mfSearchBuilder.ObjType(0);

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				searchTimeoutInSeconds: 0
			);
		}

		[TestMethod]
		public void PositiveTimeoutDoesNotThrow()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the extension.
			mfSearchBuilder.ObjType(0);

			// Attempt to execute method.
			mfSearchBuilder.ForEachEx
			(
				(o) => { },
				searchTimeoutInSeconds: 200
			);
		}

		#region MinObjId

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MinObjIdNegativeSegmentThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			mfSearchBuilder.MinObjId(-1, 1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MinObjIdZeroSegmentThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Zero segment index should throw because this method is expected to be used for finding out if any future segment has results
			mfSearchBuilder.MinObjId(0, 1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MinObjIdNegativeSegmentSizeThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			mfSearchBuilder.MinObjId(1, -1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MinObjIdZeroSegmentSizeThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			mfSearchBuilder.MinObjId(1, 0);
		}

		[TestMethod]
		public void MinObjIdReturnsValidSearchCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the condition.
			mfSearchBuilder.MinObjId(10, 50);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Fail("Only one search condition should exist");

			var condition = mfSearchBuilder.Conditions[1];

			// Condition must have valid data.
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFConditionType.MFConditionTypeGreaterThanOrEqual, condition.ConditionType);
			Assert.AreEqual(MFStatusType.MFStatusTypeObjectID, condition.Expression.DataStatusValueType);
			Assert.AreEqual(500, condition.TypedValue.Value);
		}

		#endregion MinObjId

		#region RemoveLastCondition

		[TestMethod]
		public void RemoveLastConditionIsANoopWithNoConditions()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// If there are any conditions then fail.
			if (mfSearchBuilder.Conditions.Count != 0)
				Assert.Fail("No search conditions should exist");

			// Should not throw but also not affect the count of conditions
			mfSearchBuilder.RemoveLastCondition();

			// If there are any conditions then fail.
			if (mfSearchBuilder.Conditions.Count != 0)
				Assert.Fail("No search conditions should exist");
		}

		[TestMethod]
		public void RemoveLastConditionCorrectlyRemovesOnlyCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the condition.
			mfSearchBuilder.MinObjId(10, 50);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Fail("Only one search condition should exist");

			// Should remove the only condition
			mfSearchBuilder.RemoveLastCondition();

			// If there are any conditions then fail.
			if (mfSearchBuilder.Conditions.Count != 0)
				Assert.Fail("No search conditions should exist");
		}

		[TestMethod]
		public void RemoveLastConditionCorrectlyRemovesLastCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the condition.
			mfSearchBuilder.MinObjId(10, 50);

			// Add the condition that will be removed
			mfSearchBuilder.ObjectIdSegment(20, 500);

			// If there's anything other than two conditions then fail.
			if (mfSearchBuilder.Conditions.Count != 2)
				Assert.Fail("Two search conditions should exist");

			// Should remove the latest condition
			mfSearchBuilder.RemoveLastCondition();

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Fail("Only one search condition should exist");

			var condition = mfSearchBuilder.Conditions[1];

			// Condition must have valid data.
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFConditionType.MFConditionTypeGreaterThanOrEqual, condition.ConditionType);
			Assert.AreEqual(MFStatusType.MFStatusTypeObjectID, condition.Expression.DataStatusValueType);
			Assert.AreEqual(500, condition.TypedValue.Value);
		}

		#endregion RemoveLastCondition

		#region ForEachSegment

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ForEachSegmentNullFuncThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute with null delegate.
			mfSearchBuilder.ForEachSegment(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ForEachSegmentNegativeStartSegmentThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute with negative start segment.
			mfSearchBuilder.ForEachSegment
			(
				(v, sc) => { return 0; },
				startSegment: -1
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ForEachSegmentNegativeSegmentSizeThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute with negative segment size.
			mfSearchBuilder.ForEachSegment
			(
				(v, sc) => { return 0; },
				segmentSize: -1
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ForEachSegmentZeroSegmentSizeThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute with 0 segment size.
			mfSearchBuilder.ForEachSegment
			(
				(v, sc) => { return 0; },
				segmentSize: 0
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ForEachSegmentNegativeTimeoutThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to execute with negative search timeout.
			mfSearchBuilder.ForEachSegment
			(
				( v, sc ) => { return 0; },
				searchTimeoutInSeconds: -1
			);
		}

		[TestMethod]
		public void ForEachSegmentCallsFirstSegment()
		{
			// Get the standard vault mock.
			var vaultMock = this.GetVaultMock();

			// Set up the search operations mock to return one item.
			var searchOperationsMock = new Mock<VaultObjectSearchOperations>();
			searchOperationsMock.SetupAllProperties();
			searchOperationsMock
				.Setup(m => m.SearchForObjectsByConditionsEx(It.IsAny<SearchConditions>(), It.IsAny<MFSearchFlags>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((SearchConditions searchConditions, MFSearchFlags searchFlags, bool sortResults, int maxResults, int timeout) =>
				{
					// This will be called once to check that there are no additional items.

					// Confirm arguments are correct.
					Assert.IsNotNull(searchConditions);
					Assert.AreEqual(MFSearchFlags.MFSearchFlagDisableRelevancyRanking, searchFlags);
					Assert.AreEqual(false, sortResults);
					Assert.AreEqual(1, maxResults); // Are there ANY in a subsequent segment?
					Assert.AreEqual(Extensions.MFSearchBuilderExtensionMethods.DefaultSearchTimeoutInSeconds, timeout);
					Assert.AreEqual(1, searchConditions.Count);

					// Check that search condition.
					var searchCondition = searchConditions[1];
					Assert.IsNotNull(searchCondition);
					Assert.AreEqual(MFConditionType.MFConditionTypeGreaterThanOrEqual, searchCondition.ConditionType);
					Assert.AreEqual(MFStatusType.MFStatusTypeObjectID, searchCondition.Expression.DataStatusValueType);

					// This is the minimum object id search condition, so for the first segment we expect a value of 1 * segmentSize
					Assert.AreEqual
					(
						1 * Extensions.MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment, // Anything outside of the first segment.
						searchCondition.TypedValue.Value
					);

					// No more items.
					return new ObjectSearchResults();
				});
			vaultMock.Setup(m => m.ObjectSearchOperations).Returns(searchOperationsMock.Object);

			// Create the search builder.
			var searchBuilder = new MFSearchBuilder(vaultMock.Object);

			// Execute ForEachSegment.
			var itemsCount = searchBuilder.ForEachSegment
			(
				(v, s) =>
				{
					// Ensure that the search condition has been added.
					Assert.IsNotNull(s);
					Assert.AreEqual(1, s.Count);

					// Check that search condition.
					var searchCondition = s[1];
					Assert.IsNotNull(searchCondition);
					Assert.AreEqual(MFConditionType.MFConditionTypeEqual, searchCondition.ConditionType);
					Assert.AreEqual(MFExpressionType.MFExpressionTypeObjectIDSegment, searchCondition.Expression.Type);
					Assert.AreEqual(Extensions.MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment, searchCondition.Expression.DataObjectIDSegmentSegmentSize);

					// This is the segment condition, so we expect the index of the segment to be 0 (the first segment)
					Assert.AreEqual(0, searchCondition.TypedValue.Value);

					// Return no items hit (will cause system to check whether there are any more items).
					return 0;
				}
			);

			// We expect SearchForObjectsByConditionsEx to be hit once.
			searchOperationsMock.Verify
				(
					m => m.SearchForObjectsByConditionsEx(It.IsAny<SearchConditions>(), It.IsAny<MFSearchFlags>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()),
					Times.Exactly(1)
				);

			// Ensure that all expectations are met.
			vaultMock.Verify();

			// Ensure that we no items (nothing in the vault!)
			Assert.AreEqual(0, itemsCount);
		}

		[TestMethod]
		public void ForEachSegmentCallsSecondSegment()
		{
			// Get the standard vault mock.
			var vaultMock = this.GetVaultMock();

			// For keeping track of the current segment
			var segment = 0;

			// Set up the search operations mock to return one item.
			var searchOperationsMock = new Mock<VaultObjectSearchOperations>();
			searchOperationsMock.SetupAllProperties();
			searchOperationsMock
				.Setup(m => m.SearchForObjectsByConditionsEx(It.IsAny<SearchConditions>(), It.IsAny<MFSearchFlags>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((SearchConditions searchConditions, MFSearchFlags searchFlags, bool sortResults, int maxResults, int timeout) =>
				{
					// This will be called once to check that there are no additional items.

					// Confirm arguments are correct.
					Assert.IsNotNull(searchConditions);
					Assert.AreEqual(MFSearchFlags.MFSearchFlagDisableRelevancyRanking, searchFlags);
					Assert.AreEqual(false, sortResults);
					Assert.AreEqual(1, maxResults); // Are there ANY in a subsequent segment?
					Assert.AreEqual(Extensions.MFSearchBuilderExtensionMethods.DefaultSearchTimeoutInSeconds, timeout);
					Assert.AreEqual(1, searchConditions.Count);

					// Check that search condition.
					var searchCondition = searchConditions[1];
					Assert.IsNotNull(searchCondition);
					Assert.AreEqual(MFConditionType.MFConditionTypeGreaterThanOrEqual, searchCondition.ConditionType);
					Assert.AreEqual(MFStatusType.MFStatusTypeObjectID, searchCondition.Expression.DataStatusValueType);

					// This is the minimum object id search condition
					// We should only hit this for the second segment, so for the second segment we expect a value of 2 * segmentSize
					Assert.AreEqual
					(
						2 * Extensions.MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment, // Anything after the second segment.
						searchCondition.TypedValue.Value
					);

					// No more items.
					return new ObjectSearchResults();
				});
			vaultMock.Setup(m => m.ObjectSearchOperations).Returns(searchOperationsMock.Object);

			// Create the search builder.
			var searchBuilder = new MFSearchBuilder(vaultMock.Object);

			// Execute ForEachSegment.
			var itemsCount = searchBuilder.ForEachSegment
			(
				(v, s) =>
				{
					// Get the current segment and increment out counter
					var currentSegment = segment;
					++segment;

					// Ensure that the search condition has been added.
					Assert.IsNotNull(s);
					Assert.AreEqual(1, s.Count);

					// Check that search condition.
					var searchCondition = s[1];
					Assert.IsNotNull(searchCondition);
					Assert.AreEqual(MFConditionType.MFConditionTypeEqual, searchCondition.ConditionType);
					Assert.AreEqual(MFExpressionType.MFExpressionTypeObjectIDSegment, searchCondition.Expression.Type);
					Assert.AreEqual(Extensions.MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment, searchCondition.Expression.DataObjectIDSegmentSegmentSize);

					// This is the segment condition, so we expect the index of the segment to be the same as our counter
					Assert.AreEqual(currentSegment, searchCondition.TypedValue.Value);

					// Fake that we are hitting one item in the first segment
					if (currentSegment == 0)
						return 1;

					// Return no items hit (will cause system to check whether there are any more items).
					return 0;
				}
			);

			// We expect SearchForObjectsByConditionsEx to be hit once.
			searchOperationsMock.Verify
				(
					m => m.SearchForObjectsByConditionsEx(It.IsAny<SearchConditions>(), It.IsAny<MFSearchFlags>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()),
					Times.Exactly(1)
				);

			// Ensure that all expectations are met.
			vaultMock.Verify();

			// We processed segment 0 and found no items in or after segment 1 so our counter should end on 2 (gets incremented when looking at segment 1)
			Assert.AreEqual(2, segment);

			// Ensure that we hit one item (not really, but it causes us to go into the second segment)
			Assert.AreEqual(1, itemsCount);
		}

		#endregion

		protected override Mock<Vault> GetVaultMock()
		{
			// Get the standard vault mock.
			var vaultMock = base.GetVaultMock();

			// Set up the search operations mock.
			var searchOperationsMock = new Mock<VaultObjectSearchOperations>();
			searchOperationsMock.SetupAllProperties();
			searchOperationsMock
				.Setup(m => m.SearchForObjectsByConditionsEx(It.IsAny<SearchConditions>(), It.IsAny<MFSearchFlags>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(new ObjectSearchResults());
			vaultMock.Setup(m => m.ObjectSearchOperations).Returns(searchOperationsMock.Object);

			return vaultMock;
		}
	}
}
