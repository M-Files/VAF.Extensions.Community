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
		/// <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods.Property(Common.MFSearchBuilder, int, long?, MFConditionType, MFParentChildBehavior, DataFunctionCall)"/>
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
					PropertyValueSearchConditionTestBase.TestInteger64PropertyId, 
					123L, 
					MFDataType.MFDatatypeInteger64,
					conditionType, 
					MFParentChildBehavior.MFParentChildBehaviorNone
				};
			}
		}

	}
}
