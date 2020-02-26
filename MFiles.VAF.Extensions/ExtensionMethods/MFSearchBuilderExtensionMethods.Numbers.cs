using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFilesAPI;

namespace MFiles.VAF.Extensions.ExtensionMethods
{
	/// <summary>
	/// Extension methods for the <see cref="MFSearchBuilder"/> class.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static partial class MFSearchBuilderExtensionMethods
	{
		#region Integer overloads

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeInteger"/>,
		/// <see cref="MFDataType.MFDatatypeInteger64"/> or <see cref="MFDataType.MFDatatypeFloating"/> property definition.
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
			int? value,
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
					case MFDataFunction.MFDataFunctionDaysFrom:
					case MFDataFunction.MFDataFunctionDaysTo:
					case MFDataFunction.MFDataFunctionYear:
					{
						// If it's a timestamp then convert to integer.
						if (dataType == MFDataType.MFDatatypeDate
							|| dataType == MFDataType.MFDatatypeTimestamp)
							dataType = MFDataType.MFDatatypeInteger;

						// Ensure value has a value.
						if (null == value)
							throw new ArgumentException($"value cannot be null.", nameof(value));

						// If it's a year then it should be four-digits.
						if (dataFunctionCall.DataFunction == MFDataFunction.MFDataFunctionYear
							&& (value < 1000 || value > 9999))
						{
							throw new ArgumentException($"The year must be four digits.", nameof(value));
						}

						break;
					}
				}
			}
			
			// If it is not the right data type then throw.
			if (dataType != MFDataType.MFDatatypeInteger
				&& dataType != MFDataType.MFDatatypeInteger64
				&& dataType != MFDataType.MFDatatypeFloating
				&& dataType != MFDataType.MFDatatypeLookup
				&& dataType != MFDataType.MFDatatypeMultiSelectLookup)
				throw new ArgumentException($"Property {propertyDef} is not an integer, long, real, lookup or multi-select lookup property.", nameof(propertyDef));

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

		#endregion

		#region Long overloads

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeInteger"/>,
		/// <see cref="MFDataType.MFDatatypeInteger64"/> or <see cref="MFDataType.MFDatatypeFloating"/> property definition.
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
			long? value,
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

			// If it is not valid then throw.
			if (dataType != MFDataType.MFDatatypeInteger
				&& dataType != MFDataType.MFDatatypeInteger64
				&& dataType != MFDataType.MFDatatypeFloating
				&& dataType != MFDataType.MFDatatypeLookup
				&& dataType != MFDataType.MFDatatypeMultiSelectLookup)
				throw new ArgumentException($"Property {propertyDef} is not an integer, long, real, lookup or multi-select lookup property.", nameof(propertyDef));

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

		#endregion

		#region Double overloads

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeInteger"/>,
		/// <see cref="MFDataType.MFDatatypeInteger64"/> or <see cref="MFDataType.MFDatatypeFloating"/> property definition.
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
			double? value,
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

			// If it is not valid then throw.
			if (dataType != MFDataType.MFDatatypeFloating)
				throw new ArgumentException($"Property {propertyDef} is not a floating-point property.", nameof(propertyDef));

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

		#endregion

		#region Year

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeDate"/>
		/// or <see cref="MFDataType.MFDatatypeTimestamp"/> property definition.
		/// This method searches solely by the year component in the property value , equivalent to using
		/// a <see cref="DataFunctionCall" /> set to <see cref="DataFunctionCall.SetDataYear"/>.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="year">The four-digit year to search by.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Year
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int year,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), "Property Ids must be greater than -1; ensure that your property alias was resolved.");
			if(year < 1000 || year > 9999)
				throw new ArgumentOutOfRangeException(nameof(year), "The year must be four digits.");

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// What is the data type of the property?
			DataFunctionCall dataFunctionCall = null;
			switch (dataType)
			{
				case MFDataType.MFDatatypeTimestamp:
				case MFDataType.MFDatatypeDate:

					// Timestamps and dates should be converted to integer using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataYear();

					break;
				default:
					throw new ArgumentException($"Property {propertyDef} is not a date or timestamp property.", nameof(propertyDef));
			}

			// Use the property method.
			return searchBuilder.Property
			(
				propertyDef,
				year,
				conditionType,
				parentChildBehavior,
				dataFunctionCall
			);

		}

		#endregion

		#region Days to

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeDate"/>
		/// or <see cref="MFDataType.MFDatatypeTimestamp"/> property definition.
		/// This method searches by how many days there are to the property, equivalent to using
		/// a <see cref="DataFunctionCall" /> set to <see cref="DataFunctionCall.SetDataDaysTo"/>.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="value">The number of days to search by.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder DaysTo
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int value,
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

					// Timestamps and dates should be converted to integer using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataDaysTo();

					break;
				default:
					throw new ArgumentException($"Property {propertyDef} is not a date or timestamp property.", nameof(propertyDef));
			}

			// Use the property method.
			return searchBuilder.Property
			(
				propertyDef,
				value,
				conditionType,
				parentChildBehavior,
				dataFunctionCall
			);

		}

		#endregion

		#region Days from

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeDate"/>
		/// or <see cref="MFDataType.MFDatatypeTimestamp"/> property definition.
		/// This method searches by how many days there are from the property, equivalent to using
		/// a <see cref="DataFunctionCall" /> set to <see cref="DataFunctionCall.SetDataDaysFrom"/>.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="value">The number of days to search by.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder DaysFrom
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int value,
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

					// Timestamps and dates should be converted to integer using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataDaysFrom();

					break;
				default:
					throw new ArgumentException($"Property {propertyDef} is not a date or timestamp property.", nameof(propertyDef));
			}

			// Use the property method.
			return searchBuilder.Property
			(
				propertyDef,
				value,
				conditionType,
				parentChildBehavior,
				dataFunctionCall
			);

		}

		#endregion
	}
}
