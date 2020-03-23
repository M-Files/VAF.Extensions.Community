using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFilesAPI;
using MFilesAPI.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class StringPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<string>
	{
		public StringPropertyValueSearchCondition()
			: base(new[] { MFDataType.MFDatatypeText, MFDataType.MFDatatypeMultiLineText})
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
			DataFunctionCall dataFunctionCall = null,
			PropertyDefOrObjectTypes indirectionLevels = null
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
				indirectionLevels,
				dataFunctionCall
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, string, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(StringPropertyValueSearchCondition.GetValidValues), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			string input,
			MFDataType dataType,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				dataType,
				input,
				conditionType,
				parentChildBehavior,
				indirectionLevels
			);
		}

		public static IEnumerable<object[]> GetValidValues()
		{
			foreach (MFConditionType conditionType in Enum.GetValues(typeof(MFConditionType)).Cast<MFConditionType>())
			{
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestTextPropertyId, 
					"hello world", 
					MFDataType.MFDatatypeText,
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone,
					(PropertyDefOrObjectTypes)null
				};
			}
			foreach (MFConditionType conditionType in Enum.GetValues(typeof(MFConditionType)).Cast<MFConditionType>())
			{
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestMultiLineTextPropertyId, 
					"hello world", 
					MFDataType.MFDatatypeMultiLineText,
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone,
					(PropertyDefOrObjectTypes)null
				};
			}
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="VAF.Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, string, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition when using indirection levels.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(StringPropertyValueSearchCondition.GetValidValuesWithIndirectionLevels), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect_WithIndirectionLevels
			(
			int propertyDef, 
			string input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeText,
				input,
				conditionType,
				parentChildBehavior,
				indirectionLevels
			);
		}

		public static IEnumerable<object[]> GetValidValuesWithIndirectionLevels()
		{
			// Single indirection level by property.
			{
				var indirectionLevels = new PropertyDefOrObjectTypes();
				indirectionLevels.AddPropertyDefIndirectionLevel(1234);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestTextPropertyId,
					"hello",
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}

			// Single indirection level by object type.
			{
				var indirectionLevels = new PropertyDefOrObjectTypes();
				indirectionLevels.AddObjectTypeIndirectionLevel(101);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestTextPropertyId,
					"hello",
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}

			// Multiple indirection levels by property.
			{
				var indirectionLevels = new PropertyDefOrObjectTypes();
				indirectionLevels.AddPropertyDefIndirectionLevel(1234);
				indirectionLevels.AddPropertyDefIndirectionLevel(4321);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestTextPropertyId,
					"hello",
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}

			// Multiple indirection levels by object type.
			{
				var indirectionLevels = new PropertyDefOrObjectTypes();
				indirectionLevels.AddObjectTypeIndirectionLevel(101);
				indirectionLevels.AddObjectTypeIndirectionLevel(102);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestTextPropertyId,
					"hello",
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}
		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetYearAndMonth works against a timestamp property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetYearAndMonth()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataYearAndMonth();

			// Add a search condition for the SetDataYearAndMonth data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				"2018-02",
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeText,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionYearAndMonth,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetYearAndMonth throws if the value is invalid.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		[DynamicData(nameof(StringPropertyValueSearchCondition.GetInvalidYearAndMonthValues), DynamicDataSourceType.Method)]
		public void DataFunctionCall_SetYearAndMonth_InvalidValues(string value)
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataYearAndMonth();

			// Add a search condition for the SetDataYearAndMonth data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				value,
				dataFunctionCall: dataFunctionCall
			);
		}

		public static IEnumerable<object[]> GetInvalidYearAndMonthValues()
		{
			// Null.
			yield return new object[] { (string) null };

			// Invalid value.
			yield return new object[] { "hello world" };

			// Invalid value.
			yield return new object[] { "a-b" };

			// Zero month.
			yield return new object[] { "2018-00" };

			// 13 month.
			yield return new object[] { "2018-13" };
		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataMonth works against a timestamp property.
		/// </summary>
		[TestMethod]
		public void DataFunctionCall_SetDataMonth()
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataMonth();

			// Add a search condition for the SetDataMonth data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				"02",
				dataFunctionCall: dataFunctionCall
			);

			// Retrieve the search condition.
			var condition = mfSearchBuilder.Conditions[1];

			// Ensure that the data type is correct.
			Assert.AreEqual
			(
				MFDataType.MFDatatypeText,
				condition.TypedValue.DataType
			);

			// Ensure that the search condition has the correct data function call setting.
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionMonth,
				condition.Expression.DataPropertyValueDataFunction
			);

		}

		/// <summary>
		/// Tests that a search condition using a <see cref="DataFunctionCall"/>
		/// using SetDataMonth throws if the value is invalid.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		[DynamicData(nameof(StringPropertyValueSearchCondition.GetInvalidMonthValues), DynamicDataSourceType.Method)]
		public void DataFunctionCall_SetDataMonth_InvalidValues(string value)
		{

			// Get the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Create the data function call.
			var dataFunctionCall = new DataFunctionCall();
			dataFunctionCall.SetDataMonth();

			// Add a search condition for the SetDataMonth data function call.
			mfSearchBuilder.Property
			(
				PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				value,
				dataFunctionCall: dataFunctionCall
			);
		}

		public static IEnumerable<object[]> GetInvalidMonthValues()
		{
			// Null.
			yield return new object[] { (string) null };

			// Invalid value.
			yield return new object[] { "hello world" };

			// Invalid value.
			yield return new object[] { "ab" };

			// Zero month.
			yield return new object[] { "00" };

			// 13 month.
			yield return new object[] { "13" };
		}
	}
}
