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
	public class FullText
		: MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.FullText"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public void AddsSearchCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the full text search condition.
			mfSearchBuilder.FullText("hello world");

			// Ensure that there is one item in the collection.
			Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullSearchTermThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search by null.
			mfSearchBuilder.FullText(null);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.FullText"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void SearchConditionIsCorrect()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();
			
			// Add the full text search condition.
			mfSearchBuilder.FullText("hello world");

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeContains, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypeAnyField, condition.Expression.Type);

			// Ensure the full text flags are correct.
			Assert.AreEqual(MFFullTextSearchFlags.MFFullTextSearchFlagsLookInFileData
							// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
							| MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData, condition.Expression.DataAnyFieldFTSFlags);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual("hello world", condition.TypedValue.Value as string);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.FullText"/>
		/// adds a valid search condition (explicitly searching only file data).
		/// </summary>
		[TestMethod]
		public void SearchConditionIsCorrect_JustFileData()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();
			
			// Add the full text search condition.
			mfSearchBuilder.FullText("hello world", MFFullTextSearchFlags.MFFullTextSearchFlagsLookInFileData);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeContains, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypeAnyField, condition.Expression.Type);

			// Ensure the full text flags are correct.
			Assert.AreEqual(MFFullTextSearchFlags.MFFullTextSearchFlagsLookInFileData, condition.Expression.DataAnyFieldFTSFlags);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual("hello world", condition.TypedValue.Value as string);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.FullText"/>
		/// adds a valid search condition (explicitly searching only metadata).
		/// </summary>
		[TestMethod]
		public void SearchConditionIsCorrect_JustMetadata()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();
			
			// Add the full text search condition.
			mfSearchBuilder.FullText("hello world", MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeContains, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypeAnyField, condition.Expression.Type);

			// Ensure the full text flags are correct.
			Assert.AreEqual(MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData, condition.Expression.DataAnyFieldFTSFlags);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual("hello world", condition.TypedValue.Value as string);
		}
	}
}
