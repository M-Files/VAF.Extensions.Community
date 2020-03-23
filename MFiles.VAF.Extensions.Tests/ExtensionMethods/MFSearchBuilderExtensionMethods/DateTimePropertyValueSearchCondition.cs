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
				dataFunctionCall,
				indirectionLevels
				);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, DateTime?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(DateTimePropertyValueSearchCondition.GetValidValues), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect
		(
			int propertyDef, 
			DateTime? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
		)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeDate,
				input,
				conditionType,
				parentChildBehavior,
				indirectionLevels
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, DateTime?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void SearchConditionIsCorrect_TimeStrippedFromDateProperties
			(
			)
		{
			// Create the search builder.
			var searchBuilder = new MFSearchBuilder(this.GetVaultMock().Object);

			// Set up the expected data.
			var propertyDef = PropertyValueSearchConditionTestBase.TestDatePropertyId;
			var parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone;
			var conditionType = MFConditionType.MFConditionTypeEqual;
			var indirectionLevels = new PropertyDefOrObjectTypes();
			var val = DateTime.Now;

			// If we happen to run right at midnight then make sure we have a time component.
			if (val.Hour == 0 && val.Minute == 0)
				val = val.AddMinutes(30);

			// Add the condition.
			searchBuilder.Property
			(
				propertyDef,
				val,
				conditionType,
				parentChildBehavior,
				indirectionLevels: indirectionLevels
			);

			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeDate,
				val.Date, // Ensure that the time was stripped!
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
					PropertyValueSearchConditionTestBase.TestDatePropertyId, 
					DateTime.Today, 
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone,
					(PropertyDefOrObjectTypes)null
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
		
		/// <summary>
		/// Tests that calling
		/// <see cref="VAF.Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, DateTime?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition when using indirection levels.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(DateTimePropertyValueSearchCondition.GetValidValuesWithIndirectionLevels), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect_WithIndirectionLevels
			(
			int propertyDef, 
			DateTime? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeDate,
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
					PropertyValueSearchConditionTestBase.TestDatePropertyId,
					DateTime.Now.Date,
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
					PropertyValueSearchConditionTestBase.TestDatePropertyId,
					DateTime.Now.Date,
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
					PropertyValueSearchConditionTestBase.TestDatePropertyId,
					DateTime.Now.Date,
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
					PropertyValueSearchConditionTestBase.TestDatePropertyId,
					DateTime.Now.Date,
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}
		}

	}
}
