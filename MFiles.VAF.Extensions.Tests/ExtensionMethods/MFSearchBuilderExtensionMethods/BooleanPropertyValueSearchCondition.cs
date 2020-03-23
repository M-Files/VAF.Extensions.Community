using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFilesAPI.Extensions;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class BooleanPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<bool?>
	{
		public BooleanPropertyValueSearchCondition()
			: base(new[] { MFDataType.MFDatatypeBoolean })
		{
		}

		/// <inherit />
		protected override void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			bool? value,
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
		/// <see cref="VAF.Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, bool?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, false, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, null, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeNotEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, false, MFConditionType.MFConditionTypeNotEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, null, MFConditionType.MFConditionTypeNotEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorIncludeParentValues, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorIncludeChildValues, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorIncludeParentValues | MFParentChildBehavior.MFParentChildBehaviorIncludeChildValues, (PropertyDefOrObjectTypes)null)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			bool? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeBoolean,
				input,
				conditionType,
				parentChildBehavior,
				indirectionLevels
			);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="VAF.Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, bool?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition when using indirection levels.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(BooleanPropertyValueSearchCondition.GetValidValuesWithIndirectionLevels), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect_WithIndirectionLevels
			(
			int propertyDef, 
			bool? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeBoolean,
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
				indirectionLevels.AddPropertyDefIndirectionLevel(PropertyValueSearchConditionTestBase.TestLookupPropertyId);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestBooleanPropertyId,
					true,
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}

			// Single indirection level by object type.
			{
				var indirectionLevels = new PropertyDefOrObjectTypes();
				indirectionLevels.AddObjectTypeIndirectionLevel(PropertyValueSearchConditionTestBase.TestProjectObjectTypeId);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestBooleanPropertyId,
					true,
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}

			// Multiple indirection levels by property.
			{
				var indirectionLevels = new PropertyDefOrObjectTypes();
				indirectionLevels.AddPropertyDefIndirectionLevel(PropertyValueSearchConditionTestBase.TestLookupPropertyId);
				indirectionLevels.AddPropertyDefIndirectionLevel(PropertyValueSearchConditionTestBase.TestMultiSelectLookupPropertyId);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestBooleanPropertyId,
					true,
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}

			// Multiple indirection levels by object type.
			{
				var indirectionLevels = new PropertyDefOrObjectTypes();
				indirectionLevels.AddObjectTypeIndirectionLevel(PropertyValueSearchConditionTestBase.TestProjectObjectTypeId);
				indirectionLevels.AddObjectTypeIndirectionLevel(PropertyValueSearchConditionTestBase.TestCustomerObjectTypeId);
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestBooleanPropertyId,
					true,
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}
		}

	}
}
