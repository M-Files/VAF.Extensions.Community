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
	public class MonthPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<int?>
	{
		public MonthPropertyValueSearchCondition()
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
			mfSearchBuilder.Month
				(
				propertyDef,
				value ?? 5,
				conditionType,
				parentChildBehavior
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Month"/>
		/// correctly sets the value
		/// </summary>
		[TestMethod]
		public void ValueCorrectlySetToString()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.Month
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				5
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual
			(
				"05",
				condition.TypedValue.Value as string
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Month"/>
		/// correctly populates the data function call.
		/// </summary>
		[TestMethod]
		public void DataFunctionCallPopulated()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.Month
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				5
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the data function call is correct.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionMonth,
				condition.Expression.DataPropertyValueDataFunction
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Month"/>
		/// throws if zero month.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ZeroMonthThrows()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.Month
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				0
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.Month"/>
		/// throws if 13 month.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThirteenMonthThrows()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.Month
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				13
			);
		}

	}
}
