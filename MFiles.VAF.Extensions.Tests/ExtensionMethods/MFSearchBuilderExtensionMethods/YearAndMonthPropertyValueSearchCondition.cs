using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class YearAndMonthPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<string>
	{
		public YearAndMonthPropertyValueSearchCondition()
			: base(new[] { MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp })
		{
		}

		/// <inherit />
		protected override void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			string value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			DataFunctionCall dataFunctionCall = null
			)
		{
			// Sanity.
			if (null == mfSearchBuilder)
				throw new ArgumentNullException(nameof(mfSearchBuilder));

			// Call the property overload.
			mfSearchBuilder.YearAndMonth
				(
				propertyDef,
				2020,
				1,
				conditionType,
				parentChildBehavior
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.YearAndMonth(Common.MFSearchBuilder, int, int, int, MFConditionType, MFParentChildBehavior)"/>
		/// correctly sets the value.
		/// </summary>
		[TestMethod]
		public void ValueCorrectlySetToString()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.YearAndMonth
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				2018,
				5
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeText, condition.TypedValue.DataType);
			Assert.AreEqual
			(
				"2018-05",
				condition.TypedValue.Value as string
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.YearAndMonth(Common.MFSearchBuilder, int, int, int, MFConditionType, MFParentChildBehavior)"/>
		/// correctly populates the data function call.
		/// </summary>
		[TestMethod]
		public void DataFunctionCallPopulated()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.YearAndMonth
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				2019,
				5
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the data function call is correct.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionYearAndMonth,
				condition.Expression.DataPropertyValueDataFunction
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.YearAndMonth(Common.MFSearchBuilder, int, int, int, MFConditionType, MFParentChildBehavior)"/>
		/// throws if zero month.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ZeroMonthThrows()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.YearAndMonth
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				2019,
				0
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.YearAndMonth(Common.MFSearchBuilder, int, int, int, MFConditionType, MFParentChildBehavior)"/>
		/// throws if 13 month.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThirteenMonthThrows()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the month.
			mfSearchBuilder.YearAndMonth
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				2019,
				13
			);
		}

	}
}
