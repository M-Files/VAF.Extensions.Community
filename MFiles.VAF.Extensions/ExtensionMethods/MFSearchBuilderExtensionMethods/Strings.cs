﻿using System;
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
		#region String overloads

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeText"/>
		/// or <see cref="MFDataType.MFDatatypeMultiLineText"/> property definition.
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
			string value,
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
					case MFDataFunction.MFDataFunctionMonth:
					{
						// If it's a timestamp or date then convert to text.
						if (dataType == MFDataType.MFDatatypeTimestamp
							|| dataType == MFDataType.MFDatatypeDate)
							dataType = MFDataType.MFDatatypeText;

						// Validate the value.
						if (value?.Length != 2)
						{
							throw new ArgumentException
							(
								string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallMonthInvalid, value),
								nameof(value)
							);
						}

						// Is it an integer?
						if (false == Int32.TryParse(value, out int month))
						{
							throw new ArgumentException
							(
								string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallMonthInvalid, value),
								nameof(value)
							);
						}

						// Validate the month.
						if (month <= 0 || month > 12)
						{
							throw new ArgumentException
							(
								string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallMonthOutOfRange, value),
								nameof(value)
							);
						}
					}
						break;
					case MFDataFunction.MFDataFunctionYearAndMonth:
					{
						// If it's a timestamp or date then convert to text.
						if (dataType == MFDataType.MFDatatypeTimestamp
							|| dataType == MFDataType.MFDatatypeDate)
							dataType = MFDataType.MFDatatypeText;

						// Also, validate the value.
						var splitValue = (value ?? "").Split("-".ToCharArray());
						if (splitValue.Length != 2
							|| splitValue[0].Length != 4
							|| splitValue[1].Length != 2)
						{
							throw new ArgumentException
							(
								string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallYearAndMonthInvalid, value),
								nameof(value)
							);
						}

						// Is it a valid set of integers?
						{
							if (false == Int32.TryParse(splitValue[0], out _)
								|| false == Int32.TryParse(splitValue[1], out int month))
							{
								throw new ArgumentException
								(
								string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallYearAndMonthInvalid, value),
									nameof(value)
								);
							}

							// Validate the month.
							if (month <= 0 || month > 12)
							{
								throw new ArgumentException
								(
								string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallMonthOutOfRange, value),
									nameof(value)
								);
							}
						}
					}
						break;
				}
			}

			// If it is not okay then throw.
			if (dataType != MFDataType.MFDatatypeText
				&& dataType != MFDataType.MFDatatypeMultiLineText)
				throw new ArgumentException
				(
					string.Format
					(
						Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
						propertyDef,
						string.Join(", ", new[] { MFDataType.MFDatatypeText, MFDataType.MFDatatypeMultiLineText })
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

		#region Month

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeDate"/>
		/// or <see cref="MFDataType.MFDatatypeTimestamp"/> property definition.
		/// This method searches solely by the month component in the property value , equivalent to using
		/// a <see cref="DataFunctionCall" /> set to <see cref="DataFunctionCall.SetDataMonth" />.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="month">The 1-based number of the month to search by (1 = January, 12 = December).</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Month
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int month,
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
			if(month < 1 || month > 12)
				throw new ArgumentOutOfRangeException(nameof(month), string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallMonthOutOfRange, month));

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// What is the data type of the property?
			DataFunctionCall dataFunctionCall;
			switch (dataType)
			{
				case MFDataType.MFDatatypeTimestamp:
				case MFDataType.MFDatatypeDate:

					// Timestamps and dates should be converted to text using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataMonth();

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
				month.ToString("00"),
				conditionType,
				parentChildBehavior,
				indirectionLevels,
				dataFunctionCall
			);

		}

		#endregion

		#region Year and month

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for a <see cref="MFDataType.MFDatatypeDate"/>
		/// or <see cref="MFDataType.MFDatatypeTimestamp"/> property definition.
		/// This method searches solely by the year and month components in the property value , equivalent to using
		/// a <see cref="DataFunctionCall" /> set to <see cref="DataFunctionCall.SetDataYearAndMonth" />.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="year">The four-digit year to search by.</param>
		/// <param name="month">The 1-based number of the month to search by (1 = January, 12 = December).</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder YearAndMonth
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			int year,
			int month,
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
			if(month < 1 || month > 12)
				throw new ArgumentOutOfRangeException(nameof(month), string.Format(Resources.Exceptions.MFSearchBuilderExtensionMethods.DataFunctionCallMonthOutOfRange, month));
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

					// Timestamps and dates should be converted to text using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataYearAndMonth();

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
				$"{year}-{month:00}",
				conditionType,
				parentChildBehavior,
				indirectionLevels,
				dataFunctionCall
			);

		}

		#endregion
	}
}
