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
	public class IntegerPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<int?>
	{
		public IntegerPropertyValueSearchCondition()
			: base(new[]
			{ 
				MFDataType.MFDatatypeInteger,
				MFDataType.MFDatatypeInteger64,
				MFDataType.MFDatatypeFloating,
				MFDataType.MFDatatypeLookup,
				MFDataType.MFDatatypeMultiSelectLookup
			})
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
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.Property(Common.MFSearchBuilder, int, int?, MFConditionType, MFParentChildBehavior, DataFunctionCall)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(IntegerPropertyValueSearchCondition.GetValidValues), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			int? input,
			MFDataType dataType,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				dataType,
				input,
				conditionType,
				parentChildBehavior
			);
		}

		public static IEnumerable<object[]> GetValidValues()
		{
			// Null is valid (no value).
			yield return new object[]
			{
				PropertyValueSearchConditionTestBase.TestInteger64PropertyId, 
				null, 
				MFDataType.MFDatatypeInteger64,
				MFConditionType.MFConditionTypeEqual, 
				MFParentChildBehavior.MFParentChildBehaviorNone
			};

			foreach (MFConditionType conditionType in Enum.GetValues(typeof(MFConditionType)).Cast<MFConditionType>())
			{
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestIntegerPropertyId, 
					123, 
					MFDataType.MFDatatypeInteger,
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone
				};
			}
		}

		#region Test SetDataYear data function call

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataYear works against a timestamp property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataYear_Timestamp()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataYear();

			// Add a search condition for the SetDataYear data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				2018,
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeInteger,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionYear,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataYear works against a date property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataYear_Date()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataYear();

			// Add a search condition for the SetDataYear data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestDatePropertyId,
				2018,
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeInteger,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionYear,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetYear throws if the value is invalid.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		[DynamicData(nameof(IntegerPropertyValueSearchCondition.GetInvalidYearValues), DynamicDataSourceType.Method)]
		public void DataFunctionCall_SetYear_InvalidValues(int? value)
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataYear();

			// Add a search condition for the SetDataYear data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				value,
				dataFunctionCall: dataFunctionCall
			);
		}

		public static IEnumerable<object[]> GetInvalidYearValues()
		{
			// Null.
			yield return new object[] { (int?) null };

			// 3-digit value.
			yield return new object[] { 123 };

			// 5-digit value.
			yield return new object[] { 12345 };
		}

		#endregion

		#region Test SetDataDaysFrom data function call

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataDaysFrom works against a timestamp property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataDaysFrom_Timestamp()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataDaysFrom();

			// Add a search condition for the SetDataDaysFrom data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				5,
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeInteger,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionDaysFrom,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataDaysFrom works against a date property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataDaysFrom_Date()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataDaysFrom();

			// Add a search condition for the SetDataDaysFrom data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestDatePropertyId,
				5,
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeInteger,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionDaysFrom,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataDaysFrom throws if the value is invalid.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		[DynamicData(nameof(IntegerPropertyValueSearchCondition.GetInvalidDaysFromValues), DynamicDataSourceType.Method)]
		public void DataFunctionCall_SetDataDaysFrom_InvalidValues(int? value)
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataDaysFrom();

			// Add a search condition for the SetDataDaysFrom data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				value,
				dataFunctionCall: dataFunctionCall
			);
		}

		public static IEnumerable<object[]> GetInvalidDaysFromValues()
		{
			// Null.
			yield return new object[] { (int?) null };
		}

		#endregion

		#region Test SetDataDaysFrom data function call

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataDaysTo works against a timestamp property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataDaysTo_Timestamp()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataDaysTo();

			// Add a search condition for the SetDataDate data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				5,
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeInteger,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionDaysTo,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataDaysTo works against a date property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataDaysTo_Date()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataDaysTo();

			// Add a search condition for the SetDataDaysTo data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestDatePropertyId,
				5,
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeInteger,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionDaysTo,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataDaysTo throws if the value is invalid.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		[DynamicData(nameof(IntegerPropertyValueSearchCondition.GetInvalidDaysToValues), DynamicDataSourceType.Method)]
		public void DataFunctionCall_SetDataDaysTo_InvalidValues(int? value)
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataDaysTo();

			// Add a search condition for the SetDataDaysTo data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				value,
				dataFunctionCall: dataFunctionCall
			);
		}

		public static IEnumerable<object[]> GetInvalidDaysToValues()
		{
			// Null.
			yield return new object[] { (int?) null };
		}

		#endregion
	}
}
