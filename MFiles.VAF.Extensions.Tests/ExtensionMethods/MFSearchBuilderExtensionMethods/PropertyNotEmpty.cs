using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class PropertyNotEmpty
		: PropertyValueSearchConditionTestBase<bool>
	{
		public PropertyNotEmpty()
			: base(new[] { MFDataType.MFDatatypeBoolean })
		{
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.MFSearchBuilderExtensionMethods.PropertyEmpty"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public new void AddsSearchCondition()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Ensure it has no items in the collection.
			Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

			// Add the search condition.
			mfSearchBuilder.PropertyNotEmpty(PropertyValueSearchConditionTestBase.TestBooleanPropertyId);

			// Ensure that there is one item in the collection.
			Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.MFSearchBuilderExtensionMethods.PropertyEmpty"/>
		/// with a negative property ID throws an exception.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativePropertyIdThrows()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition.
			mfSearchBuilder.PropertyNotEmpty(-1);
		}

		/// <summary>
		/// Tests that calling
		/// <see cref="MFiles.VAF.Extensions.MFSearchBuilderExtensionMethods.PropertyEmpty"/>
		/// with a positive property ID adds a valid search condition.
		/// </summary>
		[TestMethod]
		public void SearchConditionAdded()
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition.
			mfSearchBuilder.PropertyNotEmpty(PropertyValueSearchConditionTestBase.TestBooleanPropertyId);

			// Get the search condition.
			var condition = mfSearchBuilder.Conditions.Cast<MFilesAPI.SearchCondition>().FirstOrDefault();
			Assert.IsNotNull(condition);
			Assert.AreEqual(MFilesAPI.MFConditionType.MFConditionTypeNotEqual, condition.ConditionType);
			Assert.AreEqual(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, condition.Expression.DataPropertyValuePropertyDef);
			Assert.IsTrue(condition.TypedValue.IsNULL());
		}

		/// <inherit />
		protected override void AddSearchCondition(MFSearchBuilder mfSearchBuilder, int propertyDef, bool value, MFConditionType conditionType = MFConditionType.MFConditionTypeEqual, MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone, DataFunctionCall dataFunctionCall = null, PropertyDefOrObjectTypes indirectionLevels = null)
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
	}
}
