using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MFiles.VAF.Common;
using MFilesAPI;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Extension methods for the <see cref="MFSearchBuilder"/> class.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static partial class MFSearchBuilderExtensionMethods
	{
		#region DateTime overloads

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeDate"/>
		/// or <see cref="MFDataType.MFDatatypeTimestamp"/> property definition.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="value">The value to search for.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="dataFunctionCall">An expression for modifying how the results of matches are evaluated (defaults to null).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Property
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			DateTime? value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			DataFunctionCall dataFunctionCall = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), "Property Ids must be greater than -1; ensure that your property alias was resolved.");

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// Do we need to change the data type for the data function call?
			if (dataFunctionCall != null)
			{
				switch (dataFunctionCall.DataFunction)
				{
					case MFDataFunction.MFDataFunctionDate:
						// If it's a timestamp then convert to date.
						if (dataType == MFDataType.MFDatatypeTimestamp)
							dataType = MFDataType.MFDatatypeDate;
						break;
				}
			}

			// If it is not valid then throw.
			if (dataType != MFDataType.MFDatatypeDate
				&& dataType != MFDataType.MFDatatypeTimestamp)
				throw new ArgumentException($"Property {propertyDef} is not a date or timestamp property.", nameof(propertyDef));
			
			// Add the search condition.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				propertyDef,
				dataType,
				value,
				conditionType,
				parentChildBehavior,
				dataFunctionCall
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeDate"/>
		/// or <see cref="MFDataType.MFDatatypeTimestamp"/> property definition.
		/// This method ignores any time component in the property value or in <paramref name="value"/>, equivalent to using
		/// a <see cref="DataFunctionCall" /> set to <see cref="DataFunctionCall.SetDataDate"/>.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="value">The value to search for.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Date
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			DateTime value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), "Property Ids must be greater than -1; ensure that your property alias was resolved.");

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// What is the data type of the property?
			DataFunctionCall dataFunctionCall = null;
			switch (dataType)
			{
				case MFDataType.MFDatatypeTimestamp:
				case MFDataType.MFDatatypeDate:

					// Timestamps should be converted to dates using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataDate();

					break;

				default:
					throw new ArgumentException($"Property {propertyDef} is not a date or timestamp property.", nameof(propertyDef));
			}

			// Use the property method.
			return searchBuilder.Property
			(
				propertyDef,
				value.Date,
				conditionType,
				parentChildBehavior,
				dataFunctionCall
			);

		}

		#endregion
	}
}
