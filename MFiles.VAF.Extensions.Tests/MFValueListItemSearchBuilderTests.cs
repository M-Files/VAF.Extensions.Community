using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests
{
	[TestClass]
	public class MFValueListItemSearchBuilderTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullVaultThrows()
		{
			new MFValueListItemSearchBuilder(null, 1);
		}

		[TestMethod]
		public void ValidVaultDoesNotThrow()
		{
			new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
		}

		[TestMethod]
		public void ValueListIdSet()
		{
			Assert.AreEqual
			(
				123,
				new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 123).ValueListId
			);
		}

		[TestMethod]
		public void SearchConditionsEmptyByDefault()
		{
			Assert.AreEqual
			(
				0,
				new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1).SearchConditions.Count
			);
		}

		[TestMethod]
		public void Name()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.Name("hello world");
			Assert.AreEqual(searchBuilder, ret);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFExpressionType.MFExpressionTypePropertyValue, condition.Expression.Type);
			Assert.AreEqual((int)MFValueListItemPropertyDef.MFValueListItemPropertyDefName, condition.Expression.DataPropertyValuePropertyDef);
			Assert.AreEqual("hello world", condition.TypedValue.Value);
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);
		}

		[TestMethod]
		public void Name_CustomConditionType()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.Name("hello", MFConditionType.MFConditionTypeStartsWith);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFConditionType.MFConditionTypeStartsWith, condition.ConditionType);
		}

		[TestMethod]
		public void Deleted()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.Deleted(true);
			Assert.AreEqual(searchBuilder, ret);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFExpressionType.MFExpressionTypePropertyValue, condition.Expression.Type);
			Assert.AreEqual((int)MFValueListItemPropertyDef.MFValueListItemPropertyDefDeleted, condition.Expression.DataPropertyValuePropertyDef);
			Assert.AreEqual(true, condition.TypedValue.Value);
			Assert.AreEqual(MFDataType.MFDatatypeBoolean, condition.TypedValue.DataType);
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);
		}

		[TestMethod]
		public void Deleted_CustomConditionType()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.Deleted(true, MFConditionType.MFConditionTypeNotEqual);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFConditionType.MFConditionTypeNotEqual, condition.ConditionType);
		}

		[TestMethod]
		public void Owner()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.Owner(12345);
			Assert.AreEqual(searchBuilder, ret);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFExpressionType.MFExpressionTypePropertyValue, condition.Expression.Type);
			Assert.AreEqual((int)MFValueListItemPropertyDef.MFValueListItemPropertyDefOwner, condition.Expression.DataPropertyValuePropertyDef);
			Assert.AreEqual(MFDataType.MFDatatypeLookup, condition.TypedValue.DataType);
			Assert.AreEqual(12345, condition.TypedValue.GetLookupID());
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);
		}

		[TestMethod]
		public void Owner_CustomConditionType()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.Owner(12345, MFConditionType.MFConditionTypeNotEqual);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFConditionType.MFConditionTypeNotEqual, condition.ConditionType);
		}

		[TestMethod]
		public void ExternalId()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.ExternalId("extid-11");
			Assert.AreEqual(searchBuilder, ret);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFExpressionType.MFExpressionTypePropertyValue, condition.Expression.Type);
			Assert.AreEqual((int)MFValueListItemPropertyDef.MFValueListItemPropertyDefExtID, condition.Expression.DataPropertyValuePropertyDef);
			Assert.AreEqual("extid-11", condition.TypedValue.Value);
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);
		}

		[TestMethod]
		public void ExternalId_CustomConditionType()
		{
			var searchBuilder = new MFValueListItemSearchBuilder(Mock.Of<Vault>(), 1);
			var ret = searchBuilder.ExternalId("extid-11", MFConditionType.MFConditionTypeNotEqual);
			var condition = searchBuilder.SearchConditions.Cast<SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFConditionType.MFConditionTypeNotEqual, condition.ConditionType);
		}

		[TestMethod]
		public void Find()
		{
			// Set up our mock objects.
			var valueListItemOperationsMock = new Moq.Mock<VaultValueListItemOperations>();
			valueListItemOperationsMock
				.Setup(m => m.SearchForValueListItemsEx2
				(
					It.IsAny<int>(),
					It.IsAny<SearchConditions>(),
					It.IsAny<bool>(),
					It.IsAny<MFExternalDBRefreshType>(),
					It.IsAny<bool>(),
					It.IsAny<int>(),
					It.IsAny<int>()
				)).Callback((int vl, SearchConditions c, bool u, MFExternalDBRefreshType rt, bool rcu, int pid, int mr) =>
				{
					Assert.AreEqual(12345, vl);
					Assert.AreEqual(1, c.Count);
					Assert.AreEqual(false, u);
					Assert.AreEqual(MFExternalDBRefreshType.MFExternalDBRefreshTypeNone, rt);
					Assert.AreEqual(false, rcu);
					Assert.AreEqual(-1, pid);
					Assert.AreEqual(5000, mr);
				}).Returns(Mock.Of<ValueListItemSearchResults>());
			var vaultMock = new Moq.Mock<Vault>();
			vaultMock
				.SetupGet(m => m.ValueListItemOperations)
				.Returns(valueListItemOperationsMock.Object);

			// Instantiate the builder and set something to check for.
			var searchBuilder = new MFValueListItemSearchBuilder(vaultMock.Object, 12345);
			searchBuilder.ExternalId("extid-11");

			// Find.
			var results = searchBuilder.Find();

			// Ensure that we hit the underlying method..
			valueListItemOperationsMock
				.Verify
				(
					m => m.SearchForValueListItemsEx2
					(
						It.IsAny<int>(),
						It.IsAny<SearchConditions>(),
						It.IsAny<bool>(),
						It.IsAny<MFExternalDBRefreshType>(),
						It.IsAny<bool>(),
						It.IsAny<int>(),
						It.IsAny<int>()
					),
					Times.Once()
				);
		}

		public static IEnumerable<object[]> FindParameters()
		{
			// First couple are different, rest are defaults.
			yield return new object[]
			{
					12345,
					new SearchConditions(),
					false,
					MFExternalDBRefreshType.MFExternalDBRefreshTypeNone,
					false,
					-1,
					5000
			};

			// Now let's vary some
			yield return new object[]
			{
					12345,
					new SearchConditions(),
					true,
					MFExternalDBRefreshType.MFExternalDBRefreshTypeNone,
					false,
					-1,
					5000
			};
			yield return new object[]
				{
					12345,
					new SearchConditions(),
					false,
					MFExternalDBRefreshType.MFExternalDBRefreshTypeFull,
					false,
					-1,
					5000
				};
			yield return new object[]
				{
					1,
					new SearchConditions(),
					false,
					MFExternalDBRefreshType.MFExternalDBRefreshTypeNone,
					false,
					-1,
					5000
				};
			yield return new object[]
				{
					12345,
					new SearchConditions(),
					false,
					MFExternalDBRefreshType.MFExternalDBRefreshTypeNone,
					true,
					-1,
					5000
				};
			yield return new object[]
				{
					12345,
					new SearchConditions(),
					false,
					MFExternalDBRefreshType.MFExternalDBRefreshTypeNone,
					false,
					123,
					5000
				};
			yield return new object[]
				{
					12345,
					new SearchConditions(),
					false,
					MFExternalDBRefreshType.MFExternalDBRefreshTypeNone,
					false,
					-1,
					1000
				};
		}

		[TestMethod]
		[DynamicData(nameof(FindParameters), DynamicDataSourceType.Method)]
		public void Find_CustomParameters
		(
			int valueListId,
			SearchConditions searchConditions,
			bool updateFromServer,
			MFExternalDBRefreshType refreshType,
			bool replaceCurrentUserWithCallersIdentity,
			int propertyDefId,
			int maximumResults
		)
		{
			// Set up our mock objects.
			var valueListItemOperationsMock = new Moq.Mock<VaultValueListItemOperations>();
			valueListItemOperationsMock
				.Setup(m => m.SearchForValueListItemsEx2
				(
					It.IsAny<int>(),
					It.IsAny<SearchConditions>(),
					It.IsAny<bool>(),
					It.IsAny<MFExternalDBRefreshType>(),
					It.IsAny<bool>(),
					It.IsAny<int>(),
					It.IsAny<int>()
				)).Callback((int vl, SearchConditions c, bool u, MFExternalDBRefreshType rt, bool rcu, int pid, int mr) =>
				{
					Assert.AreEqual(valueListId, vl);
					Assert.AreEqual(searchConditions.Count, c.Count); // TODO: Check the contents.
					Assert.AreEqual(updateFromServer, u);
					Assert.AreEqual(refreshType, rt);
					Assert.AreEqual(replaceCurrentUserWithCallersIdentity, rcu);
					Assert.AreEqual(propertyDefId, pid);
					Assert.AreEqual(maximumResults, mr);
				}).Returns(Mock.Of<ValueListItemSearchResults>());
			var vaultMock = new Moq.Mock<Vault>();
			vaultMock
				.SetupGet(m => m.ValueListItemOperations)
				.Returns(valueListItemOperationsMock.Object);

			// Instantiate the builder.
			var searchBuilder = new MFValueListItemSearchBuilder(vaultMock.Object, valueListId);
			if(null != searchConditions)
			{
				foreach (var sc in searchConditions.Cast<SearchCondition>())
					searchBuilder.SearchConditions.Add(0, sc);
			}

			// Find.
			var results = searchBuilder.Find
			(
				updateFromServer,
				refreshType,
				replaceCurrentUserWithCallersIdentity,
				propertyDefId,
				maximumResults
			);

			// Ensure that we hit the underlying method..
			valueListItemOperationsMock
				.Verify
				(
					m => m.SearchForValueListItemsEx2
					(
						It.IsAny<int>(),
						It.IsAny<SearchConditions>(),
						It.IsAny<bool>(),
						It.IsAny<MFExternalDBRefreshType>(),
						It.IsAny<bool>(),
						It.IsAny<int>(),
						It.IsAny<int>()
					),
					Times.Once()
				);
		}

	}
}
