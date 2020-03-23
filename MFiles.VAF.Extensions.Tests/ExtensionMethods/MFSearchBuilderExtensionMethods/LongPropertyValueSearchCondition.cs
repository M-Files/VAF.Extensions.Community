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
	public class LongPropertyValueSearchCondition
		: PropertyValueSearchConditionTestBase<long?>
	{
		public LongPropertyValueSearchCondition()
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
			long? value,
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
		/// <see cref="Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, long?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(LongPropertyValueSearchCondition.GetValidValues), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			long? input,
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
				PropertyValueSearchConditionTestBase.TestInteger64PropertyId, 
				null, 
				MFDataType.MFDatatypeInteger64,
				MFConditionType.MFConditionTypeEqual, 
				MFParentChildBehavior.MFParentChildBehaviorNone,
				(PropertyDefOrObjectTypes)null
			};

			foreach (MFConditionType conditionType in Enum.GetValues(typeof(MFConditionType)).Cast<MFConditionType>())
			{
				yield return new object[]
				{
					PropertyValueSearchConditionTestBase.TestInteger64PropertyId, 
					123L, 
					MFDataType.MFDatatypeInteger64,
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone,
					(PropertyDefOrObjectTypes)null
				};
			}
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="VAF.Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, long?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition when using indirection levels.
		/// </summary>
		[TestMethod]
		[DynamicData(nameof(LongPropertyValueSearchCondition.GetValidValuesWithIndirectionLevels), DynamicDataSourceType.Method)]
		public void SearchConditionIsCorrect_WithIndirectionLevels
			(
			int propertyDef, 
			long? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeInteger64,
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
					PropertyValueSearchConditionTestBase.TestInteger64PropertyId,
					12L,
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
					PropertyValueSearchConditionTestBase.TestInteger64PropertyId,
					12L,
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
					PropertyValueSearchConditionTestBase.TestInteger64PropertyId,
					12L,
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
					PropertyValueSearchConditionTestBase.TestInteger64PropertyId,
					12L,
					MFConditionType.MFConditionTypeEqual,
					MFParentChildBehavior.MFParentChildBehaviorNone,
					indirectionLevels
				};
			}
		}

	}
}
