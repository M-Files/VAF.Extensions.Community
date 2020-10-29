using System;
using System.Collections.Generic;
using System.Linq;
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
		public void SegmentedSearchForEach()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the search condition for the extension.
			mfSearchBuilder.ObjType(0);

			var count1 = 0;
			var count2 = mfSearchBuilder.ForEachEx( obj => { count1 += 1; } );

			// Ensure that there is one item in the vault.
			Assert.AreEqual(1, count1);
			Assert.AreEqual(1, count2);
		}

		[TestMethod]
		public void SegmentedCount()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the search condition for the extension.
			mfSearchBuilder.ObjType(0);

			var count = mfSearchBuilder.SegmentedCount();

			// Ensure that there is one item in the vault.
			Assert.AreEqual(1, count);
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

			// Attempt to execute with null delegate.
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

			// Attempt to execute with null delegate.
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

			// Attempt to execute with null delegate.
			mfSearchBuilder.ForEachSegment
			(
				(v, sc) => { return 0; },
				segmentSize: 0
			);
		}

		#endregion

		protected override Mock<Vault> GetVaultMock()
		{
			// Get the standard vault mock.
			var vaultMock = base.GetVaultMock();

			// Set up the search operations mock.
			var objectSearchOperationsMock = new Mock<VaultObjectSearchOperations>();

			// Set up the SearchForObjectsByConditionsEx method.
			objectSearchOperationsMock
				.Setup( m => m.SearchForObjectsByConditionsEx( It.IsAny<SearchConditions>(), It.IsAny<MFSearchFlags>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>() ) )
				.Returns( ( SearchConditions searchConditions, MFSearchFlags searchFlags, bool sortResults, int maxResults, int timeout ) =>
				{
					if (searchConditions.Count > 0 )
					{
						var searchConditionsList = searchConditions.Cast<SearchCondition>().ToList();
						var minIdCondition = searchConditionsList.FirstOrDefault( sc => sc.Expression.Type == MFExpressionType.MFExpressionTypeStatusValue && sc.Expression.DataStatusValueType == MFStatusType.MFStatusTypeObjectID );
						if( minIdCondition != null && (int) minIdCondition.TypedValue.Value > 0 )
						{
							return new ObjectSearchResults();
						}

						var segmentCondition = searchConditionsList.FirstOrDefault( sc => sc.Expression.Type == MFExpressionType.MFExpressionTypeObjectIDSegment );
						if( segmentCondition != null && (int) segmentCondition.TypedValue.Value > 0 )
						{
							return new ObjectSearchResults();
						}
					}

					var result = new Mock<ObjectVersion>();
					result.Setup( m => m.ObjVer ).Returns( new ObjVer() );
					var items = new List<ObjectVersion> { result.Object };
					var results = new Mock<ObjectSearchResults>();
					results.Setup( m => m.Count ).Returns( () => items.Count );
					results.Setup( m => m[ It.IsAny<int>() ] ).Returns<int>( i => items.ElementAt( i - 1 ) ); // 1-indexed list
					results.Setup( m => m.GetEnumerator() ).Returns( () => items.GetEnumerator() );

					return results.Object;
				} );

			// Set up the GetObjectCountInSearch method.
			objectSearchOperationsMock
				.Setup( m => m.GetObjectCountInSearch( It.IsAny<SearchConditions>(), It.IsAny<MFSearchFlags>() ) )
				.Returns( ( SearchConditions searchConditions, MFSearchFlags searchFlags ) =>
				{
					if( searchConditions.Count > 0 )
					{
						var searchConditionsList = searchConditions.Cast<SearchCondition>().ToList();
						var segmentCondition = searchConditionsList.FirstOrDefault( sc => sc.Expression.Type == MFExpressionType.MFExpressionTypeObjectIDSegment );
						if( segmentCondition != null && (int) segmentCondition.TypedValue.Value > 0 )
						{
							return 0;
						}
					}

					return 1;
				} );

			// Make the vault return the search operations mock as needed.
			vaultMock
				.SetupGet( m => m.ObjectSearchOperations )
				.Returns( objectSearchOperationsMock.Object );

			// Set up the object property operations mock.
			var objectPropertyOperationsMock = new Mock<VaultObjectPropertyOperations>();

			objectPropertyOperationsMock
				.Setup( m => m.GetPropertiesOfMultipleObjects( It.IsAny<ObjVers>() ) )
				.Returns( ( ObjVers objVers ) =>
				{
					var items = new List<PropertyValues>();
					for( int i = 0; i < objVers.Count; i++ )
					{
						var result = new Mock<PropertyValues>();
						items.Add( result.Object );
					}

					var results = new Mock<PropertyValuesOfMultipleObjects>();
					results.Setup( m => m.Count ).Returns( () => items.Count );
					results.Setup( m => m[ It.IsAny<int>() ] ).Returns<int>( i => items.ElementAt( i - 1 ) ); // 1-indexed list
					results.Setup( m => m.GetEnumerator() ).Returns( () => items.GetEnumerator() );

					return results.Object;
				} );

			// Make the vault return the object property operations mock as needed.
			vaultMock
				.SetupGet( m => m.ObjectPropertyOperations )
				.Returns( objectPropertyOperationsMock.Object );

			// Return the vault mock.
			return vaultMock;
		}
	}
}
