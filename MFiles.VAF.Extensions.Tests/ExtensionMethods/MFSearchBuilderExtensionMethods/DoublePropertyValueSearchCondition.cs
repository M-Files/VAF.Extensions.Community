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
	public class DoublePropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<double?>
	{
		public DoublePropertyValueSearchCondition()
			: base(new[]
			{ 
				MFDataType.MFDatatypeFloating
			})
		{
		}

		/// <inherit />
		protected override void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			double? value,
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
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, double?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(DoublePropertyValueSearchCondition.GetValidValues), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			double? input,
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
			// Null is valid (no value).
			yield return new object[]
			{
				PropertyValueSearchConditionTestBase.TestFloatPropertyId, 
				null, 
				MFDataType.MFDatatypeFloating,
				MFConditionType.MFConditionTypeEqual, 
				MFParentChildBehavior.MFParentChildBehaviorNone,
				(PropertyDefOrObjectTypes)null
			};

			foreach (MFConditionType conditionType in Enum.GetValues(typeof(MFConditionType)).Cast<MFConditionType>())
			{
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestFloatPropertyId, 
					12.0D, 
					MFDataType.MFDatatypeFloating,
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone,
					(PropertyDefOrObjectTypes)null
				};
			}
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="VAF.Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, double?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition when using indirection levels.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(DoublePropertyValueSearchCondition.GetValidValuesWithIndirectionLevels), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect_WithIndirectionLevels
			(
			int propertyDef, 
			double? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeFloating,
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
					PropertyValueSearchConditionTestBase.TestFloatPropertyId,
					12.0D,
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
					PropertyValueSearchConditionTestBase.TestFloatPropertyId,
					12.0D,
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
					PropertyValueSearchConditionTestBase.TestFloatPropertyId,
					12.0D,
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
					PropertyValueSearchConditionTestBase.TestFloatPropertyId,
					12.0D,
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}
		}

	}
}
