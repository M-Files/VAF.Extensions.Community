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
	public class DateTimePropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<DateTime?>
	{
		public DateTimePropertyValueSearchCondition()
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
			DataFunctionCall dataFunctionCall = null
			)
		{
			// Sanity.
			if (null == mfSearchBuilder)
				throw new ArgumentNullException(nameof(mfSearchBuilder));

			// Call the property overload.
			mfSearchBuilder.Property
				(
				propertyDef,
				value,
				conditionType,
				parentChildBehavior,
				dataFunctionCall
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.Property(Common.MFSearchBuilder, int, DateTime?, MFConditionType, MFParentChildBehavior, DataFunctionCall)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(DateTimePropertyValueSearchCondition.GetValidValues), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			DateTime? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeDate,
				input,
				conditionType,
				parentChildBehavior
			);
		}

		public static IEnumerable<object[]> GetValidValues()
		{
			foreach (MFConditionType conditionType in Enum.GetValues(typeof(MFConditionType)).Cast<MFConditionType>())
			{
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestDatePropertyId, 
					DateTime.Today, 
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone
				};
			}
		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataDate works against a timestamp property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataDate()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataDate();

			// Add a search condition for the SetDataDate data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				new DateTime(2018, 10, 29),
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeDate,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionDate,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

	}
}
