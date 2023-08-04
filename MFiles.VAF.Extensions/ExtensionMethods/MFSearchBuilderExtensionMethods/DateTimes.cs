﻿using System;
using MFiles.VAF.Common;
using MFiles.VAF.Extensions.ExtensionMethods;
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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Property
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			DateTime? value,
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
				throw new ArgumentException
				(
					String.Format
					(
						Resources.Exceptions.VaultInteraction.PropertyDefinition_NotOfExpectedType,
						propertyDef,
						string.Join(", ", new[] { MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp })
					),
					nameof(propertyDef)
				);

			// If it's a date and the value has a time component then strip it.
			if (dataType == MFDataType.MFDatatypeDate)
				value = value?.Date;

			// If it's a timestamp then ensure we are searching by local time.
			if(
				dataType == MFDataType.MFDatatypeTimestamp
				&& value.HasValue
			)
			{
				value = searchBuilder?.
					Vault?
					.SessionInfo?
					.TimeZoneInfo?
					.EnsureLocalTime(value.Value);
			}

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
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Date
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			DateTime value,
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

					// Timestamps should be converted to dates using a data function call.
					dataFunctionCall = new DataFunctionCall();
					dataFunctionCall.SetDataDate();

					break;

				default:
					throw new ArgumentException
					(
						String.Format
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
				value.Date,
				conditionType,
				parentChildBehavior,
				indirectionLevels,
				dataFunctionCall
			);

		}

		#endregion
	}
}
