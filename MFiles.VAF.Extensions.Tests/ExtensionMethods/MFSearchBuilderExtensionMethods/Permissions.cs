using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class Permissions
		: MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.UserHasPermissionTo"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public void AddsSearchCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the search condition for whether the user has the required permissions.
			mfSearchBuilder.UserHasPermissionTo(23, MFPermissionsExpressionType.MFVisibleTo);

			// Ensure that there is one item in the collection.
			Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.UserHasPermissionTo"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DataRow(25, MFPermissionsExpressionType.MFVisibleTo)]
		[DataRow(255, MFPermissionsExpressionType.MFEditableBy)]
		[DataRow(250, MFPermissionsExpressionType.MFDeletableBy)]
		[DataRow(21, MFPermissionsExpressionType.MFPermissionsChangeableBy)]
		[DataRow(20, MFPermissionsExpressionType.MFFullControlBy)]
		[DataRow(50, MFPermissionsExpressionType.MFObjectsAttachableToThisItemBy)]
		public void SearchConditionIsCorrect
		(
			int userId,
			MFPermissionsExpressionType permission
		)
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();
			
			// Add the search condition for whether the user has the required permissions.
			mfSearchBuilder.UserHasPermissionTo(userId, permission);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypePermissions, condition.Expression.Type);

			// Ensure the status value is correct.
			Assert.AreEqual(permission, condition.Expression.DataPermissionsType);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeLookup, condition.TypedValue.DataType);
			Assert.AreEqual(userId, condition.TypedValue.GetLookupID());
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.VisibleTo"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void VisibleToSearchConditionIsCorrect()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();
			
			// Add the search condition for whether the user has the required permissions.
			mfSearchBuilder.VisibleTo(25);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypePermissions, condition.Expression.Type);

			// Ensure the status value is correct.
			Assert.AreEqual(MFPermissionsExpressionType.MFVisibleTo, condition.Expression.DataPermissionsType);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeLookup, condition.TypedValue.DataType);
			Assert.AreEqual(25, condition.TypedValue.GetLookupID());
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.EditableBy"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void EditableBySearchConditionIsCorrect()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();
			
			// Add the search condition for whether the user has the required permissions.
			mfSearchBuilder.EditableBy(25);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypePermissions, condition.Expression.Type);

			// Ensure the status value is correct.
			Assert.AreEqual(MFPermissionsExpressionType.MFEditableBy, condition.Expression.DataPermissionsType);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeLookup, condition.TypedValue.DataType);
			Assert.AreEqual(25, condition.TypedValue.GetLookupID());
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.DeletableBy"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void DeletableBySearchConditionIsCorrect()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();
			
			// Add the search condition for whether the user has the required permissions.
			mfSearchBuilder.DeletableBy(25);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeEqual, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypePermissions, condition.Expression.Type);

			// Ensure the status value is correct.
			Assert.AreEqual(MFPermissionsExpressionType.MFDeletableBy, condition.Expression.DataPermissionsType);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeLookup, condition.TypedValue.DataType);
			Assert.AreEqual(25, condition.TypedValue.GetLookupID());
		}

	}
}
