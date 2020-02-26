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
	public class FileSize
		: MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.FileSize(Common.MFSearchBuilder, long, MFConditionType)"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public void AddsSearchCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the search condition for the file size.
			mfSearchBuilder.FileSize(1014 * 1024, MFConditionType.MFConditionTypeGreaterThan);

			// Ensure that there is one item in the collection.
			Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativeSizeThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Attempt to search by negative size.
			mfSearchBuilder.FileSize(-1000, MFConditionType.MFConditionTypeEqual);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.FileSize(Common.MFSearchBuilder, long, MFConditionType)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void SearchConditionIsCorrect()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the file size.
			mfSearchBuilder.FileSize(1024 * 1024, MFConditionType.MFConditionTypeGreaterThan);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure the condition type is correct.
			Assert.AreEqual(MFConditionType.MFConditionTypeGreaterThan, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypeFileValue, condition.Expression.Type);

			// Ensure the status value is correct.
			Assert.AreEqual(MFFileValueType.MFFileValueTypeFileSize, condition.Expression.DataFileValueType);

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeInteger64, condition.TypedValue.DataType);
			Assert.AreEqual(1024 * 1024, condition.TypedValue.Value as long?);
		}

	}
}
