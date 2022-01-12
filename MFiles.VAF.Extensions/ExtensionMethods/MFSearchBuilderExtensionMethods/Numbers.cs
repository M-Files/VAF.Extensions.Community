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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Property
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int? value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null,
			DataFunctionCall dataFunctionCall = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

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
							throw new ArgumentException(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallValueNull, nameof(value));

						// If it's a year then it should be four-digits.
						if (dataFunctionCall.DataFunction == MFDataFunction.MFDataFunctionYear
							&& (value < 1000 || value > 9999))
							throw new ArgumentException(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallYearInvalid, nameof(value));

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
				throw new ArgumentException
				(
					string.Format
					(
						Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
						propertyDef,
						string.Join(", ", new[] { MFDataType.MFDatatypeInteger, MFDataType.MFDatatypeInteger64, MFDataType.MFDatatypeFloating, MFDataType.MFDatatypeLookup, MFDataType.MFDatatypeMultiSelectLookup })
					),
					nameof(propertyDef)
				);

			// Add the search condition.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				propertyDef,
				dataType,
				value,
				conditionType,
				parentChildBehavior,
				indirectionLevels,
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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Property
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			long? value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null,
			DataFunctionCall dataFunctionCall = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// If it is not valid then throw.
			if (dataType != MFDataType.MFDatatypeInteger
				&& dataType != MFDataType.MFDatatypeInteger64
				&& dataType != MFDataType.MFDatatypeFloating
				&& dataType != MFDataType.MFDatatypeLookup
				&& dataType != MFDataType.MFDatatypeMultiSelectLookup)
				throw new ArgumentException
				(
					string.Format
					(
						Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
						propertyDef,
						string.Join(", ", new[] { MFDataType.MFDatatypeInteger, MFDataType.MFDatatypeInteger64, MFDataType.MFDatatypeFloating, MFDataType.MFDatatypeLookup, MFDataType.MFDatatypeMultiSelectLookup })
					),
					nameof(propertyDef)
				);

			// Add the search condition.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				propertyDef,
				dataType,
				value,
				conditionType,
				parentChildBehavior,
				indirectionLevels,
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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Property
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			double? value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null,
			DataFunctionCall dataFunctionCall = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// If it is not valid then throw.
			if (dataType != MFDataType.MFDatatypeFloating)
				throw new ArgumentException
				(
					string.Format
					(
						Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
						propertyDef,
						string.Join(", ", new[] { MFDataType.MFDatatypeFloating })
					),
					nameof(propertyDef)
				);

			// Add the search condition.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				propertyDef,
				dataType,
				value,
				conditionType,
				parentChildBehavior,
				indirectionLevels,
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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Year
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int year,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);
			if(year < 1000 || year > 9999)
				throw new ArgumentOutOfRangeException(nameof(year), Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallYearInvalid);

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// What is the data type of the property?
			DataFunctionCall dataFunctionCall;
			switch (dataType)
			{
				case MFDataType.MFDatatypeTimestamp:
				case MFDataType.MFDatatypeDate:

					// Timestamps and dates should be converted to integer using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataYear();

					break;
				default:
					throw new ArgumentException
					(
						string.Format
						(
							Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
							propertyDef,
							string.Join(", ", new[] { MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp })
						),
						nameof(propertyDef)
					);
			}

			// Use the property method.
			return searchBuilder.Property
			(
				propertyDef,
				year,
				conditionType,
				parentChildBehavior,
				indirectionLevels,
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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder DaysTo
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// What is the data type of the property?
			DataFunctionCall dataFunctionCall;
			switch (dataType)
			{
				case MFDataType.MFDatatypeTimestamp:
				case MFDataType.MFDatatypeDate:

					// Timestamps and dates should be converted to integer using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataDaysTo();

					break;
				default:
					throw new ArgumentException
					(
						string.Format
						(
							Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
							propertyDef,
							string.Join(", ", new[] { MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp })
						),
						nameof(propertyDef)
					);
			}

			// Use the property method.
			return searchBuilder.Property
			(
				propertyDef,
				value,
				conditionType,
				parentChildBehavior,
				indirectionLevels,
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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder DaysFrom
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// What is the data type of the property?
			DataFunctionCall dataFunctionCall;
			switch (dataType)
			{
				case MFDataType.MFDatatypeTimestamp:
				case MFDataType.MFDatatypeDate:

					// Timestamps and dates should be converted to integer using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataDaysFrom();

					break;
				default:
					throw new ArgumentException
					(
						string.Format
						(
							Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
							propertyDef,
							string.Join(", ", new[] { MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp })
						),
						nameof(propertyDef)
					);
			}

			// Use the property method.
			return searchBuilder.Property
			(
				propertyDef,
				value,
				conditionType,
				parentChildBehavior,
				indirectionLevels,
				dataFunctionCall
			);

		}

		#endregion
	}
}
