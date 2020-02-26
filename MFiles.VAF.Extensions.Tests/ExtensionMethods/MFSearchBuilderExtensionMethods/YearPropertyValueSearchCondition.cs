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
	public class YearPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<int?>
	{
		public YearPropertyValueSearchCondition()
			: base(new[] { MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp })
		{
		}

		/// <inherit />
		protected override void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			int? value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			DataFunctionCall dataFunctionCall = null
			)
		{
			// Sanity.
			if (null == mfSearchBuilder)
				throw new ArgumentNullException(nameof(mfSearchBuilder));

			// Call the property overload.
			mfSearchBuilder.Year
				(
				propertyDef,
				value ?? 2020,
				conditionType,
				parentChildBehavior
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Year"/>
		/// correctly sets the value
		/// </summary>
		[TestMethod]
		public void ValueCorrectlySetToString()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the year.
			mfSearchBuilder.Year
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				2020
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeInteger, condition.TypedValue.DataType);
			Assert.AreEqual
			(
				2020,
				condition.TypedValue.Value as int?
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Year"/>
		/// correctly populates the data function call.
		/// </summary>
		[TestMethod]
		public void DataFunctionCallPopulated()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the year.
			mfSearchBuilder.Year
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				2015
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the data function call is correct.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionYear,
				condition.Expression.DataPropertyValueDataFunction
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Year"/>
		/// throws if three digit year.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThreeDigitYearThrow()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the year.
			mfSearchBuilder.Year
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				999
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Year"/>
		/// throws if five digit year.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void FiveDigitYearThrows()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the year.
			mfSearchBuilder.Year
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				99999
			);
		}

	}
}
