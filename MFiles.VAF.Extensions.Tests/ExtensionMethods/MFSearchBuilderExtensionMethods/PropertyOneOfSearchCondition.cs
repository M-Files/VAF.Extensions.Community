using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFilesAPI.Extensions;
using System.Collections;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class PropertyOneOfSearchCondition
		: PropertyValueSearchConditionTestBase<int[]>
	{
		public PropertyOneOfSearchCondition()
			: base(new[] { MFDataType.MFDatatypeLookup, MFDataType.MFDatatypeMultiSelectLookup })
		{
		}

		/// <inherit />
		protected override void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			int[] value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			DataFunctionCall dataFunctionCall = null,
			PropertyDefOrObjectTypes indirectionLevels = null
			)
		{
			// Sanity.
			if (null == mfSearchBuilder)
				throw new ArgumentNullException(nameof(mfSearchBuilder));

			// Use our extension method.
			mfSearchBuilder.PropertyOneOf(propertyDef, value, parentChildBehavior, indirectionLevels);
		}
		
		/// <summary>
		/// Tests that calling
		/// <see cref="VAF.Extensions.MFSearchBuilderExtensionMethods.Property(MFSearchBuilder, int, bool?, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a valid search condition.
		/// </summary>
		[TestMethod]
		[DataRow(PropertyValueSearchConditionTestBase.TestLookupPropertyId, new int[] { 1 }, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		[DataRow(PropertyValueSearchConditionTestBase.TestMultiSelectLookupPropertyId, new int[] { 1, 2, 3 }, MFConditionType.MFConditionTypeEqual, MFParentChildBehavior.MFParentChildBehaviorNone, (PropertyDefOrObjectTypes)null)]
		public void SearchConditionIsCorrect
			(
			int propertyDef, 
			int[] input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			base.AssertSearchConditionIsCorrect
			(
				propertyDef,
				MFDataType.MFDatatypeMultiSelectLookup,
				input,
				conditionType,
				parentChildBehavior,
				indirectionLevels
			);
		}
		protected override void AssertAddSearchCondition(int[] value)
		{
			// We will throw if we have no items, so we need to include at least one.
			base.AssertAddSearchCondition(new int[] { 1 });
		}

		protected override void AssertValueIsCorrect(int[] expected, TypedValue actual)
		{
			// Default comparison doesn't work with the arrays we're using, so do it the hard way.

			// Copy the lookup IDs into a collection.
			var actualValues = new List<int>(actual?.GetValueAsLookups()?.Cast<Lookup>().Select(l => l.Item));

			// Make sure we have the same data.
			Assert.AreEqual(expected.Length, actualValues.Count);
			for(var i=0; i<expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actualValues[i]);
			}
		}

	}
}
