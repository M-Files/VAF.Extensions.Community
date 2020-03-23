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
	public class DatePropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<DateTime?>
	{
		public DatePropertyValueSearchCondition()
			: base(new[] { MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp })
		{
		}

		/// <inherit />
		protected override void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			DateTime? value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			DataFunctionCall dataFunctionCall = null,
			PropertyDefOrObjectTypes indirectionLevels = null
			)
		{
			// Sanity.
			if (null == mfSearchBuilder)
				throw new ArgumentNullException(nameof(mfSearchBuilder));

			// Call the property overload.
			mfSearchBuilder.Date
				(
				propertyDef,
				// Value is not nullable in the real world, but needed here for the test to work.
				value ?? new DateTime(2020, 01, 01),
				conditionType,
				parentChildBehavior,
				indirectionLevels
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Date"/>
		/// removes any time component from the date added.
		/// </summary>
		[TestMethod]
		public void StripsTimeComponentFromValue()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the date.
			mfSearchBuilder.Date
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				new DateTime(2018, 10, 1, 5, 29, 0)
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the typed value is correct.
			Assert.AreEqual(MFDataType.MFDatatypeDate, condition.TypedValue.DataType);
			Assert.AreEqual
			(
				new DateTime(2018, 10, 1),
				condition.TypedValue.Value as DateTime?
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Date"/>
		/// correctly populates the data function call.
		/// </summary>
		[TestMethod]
		public void DataFunctionCallPopulated()
		{
			
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the date.
			mfSearchBuilder.Date
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				new DateTime(2018, 10, 1)
			);

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Ensure that the data function call is correct.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionDate,
				condition.Expression.DataPropertyValueDataFunction
			);
		}

	}
}
