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
	public class DaysFromPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<int?>
	{
		public DaysFromPropertyValueSearchCondition()
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
			mfSearchBuilder.DaysFrom
				(
				propertyDef,
				value ?? 30,
				conditionType,
				parentChildBehavior
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.DaysFrom"/>
		/// correctly sets the value
		/// </summary>
		[TestMethod]
		public void ValueCorrectlySetToString()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the days from.
			mfSearchBuilder.DaysFrom
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				31
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeInteger, condition.TypedValue.DataType);
			Assert.AreEqual
			(
				31,
				condition.TypedValue.Value as int?
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFSearchBuilderExtensionMethods.DaysFrom"/>
		/// correctly populates the data function call.
		/// </summary>
		[TestMethod]
		public void DataFunctionCallPopulated()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the days from.
			mfSearchBuilder.DaysFrom
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				60
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the data function call is correct.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionDaysFrom,
				condition.Expression.DataPropertyValueDataFunction
			);
		}

	}
}
