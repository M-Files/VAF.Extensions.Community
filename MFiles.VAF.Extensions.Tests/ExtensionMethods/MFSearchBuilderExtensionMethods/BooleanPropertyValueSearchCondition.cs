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
		/// <see cref="MFSearchBuilderExtensionMethods.Property(MFiles.VAF.Common.MFSearchBuilder,int,System.Nullable{bool},MFilesAPI.MFConditionType,MFilesAPI.MFParentChildBehavior,MFilesAPI.DataFunctionCall)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, false, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, null, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeNotEqual, MFParentChildBehavior.MFParentChildBehaviorNone)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, false, MFConditionType.MFConditionTypeNotEqual, MFParentChildBehavior.MFParentChildBehaviorNone)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, null, MFConditionType.MFConditionTypeNotEqual, MFParentChildBehavior.MFParentChildBehaviorNone)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorIncludeParentValues)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorIncludeChildValues)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, true, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorIncludeParentValues | MFParentChildBehavior.MFParentChildBehaviorIncludeChildValues)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			bool? input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeBoolean,
				input,
				conditionType,
				parentChildBehavior
			);
		}

	}
}
