using System;
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
		/// <summary>
		/// Adds a property value search condition to the collection.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The property to search by.</param>
		/// <param name="dataType">The data type of the property definition (not checked).</param>
		/// <param name="value">The value to search for.</param>
		/// <param name="conditionType">What type of search to execute.</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well.</param>
		/// <param name="dataFunctionCall">An expression for modifying how the results of matches are evaluated.</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns></returns>
		private static MFSearchBuilder AddPropertyValueSearchCondition
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			MFDataType dataType,
			object value,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null,
			DataFunctionCall dataFunctionCall = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));

			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = conditionType
			};

			// Set up the property value expression.
			searchCondition.Expression.SetPropertyValueExpression
			(
				propertyDef,
				parentChildBehavior,
				dataFunctionCall
			);

			// If we have any indirection levels then use them.
			if (null != indirectionLevels)
			{
				// If any indirection level points at a value list then it will except.
				// Show a nicer error message here.
				foreach (PropertyDefOrObjectType indirectionLevel in indirectionLevels)
				{
					var objectTypeId = indirectionLevel.ID;
					if (indirectionLevel.PropertyDef)
					{
						// If it's a property def then find the object type.
						PropertyDef indirectionLevelPropertyDef;
						try
						{
							indirectionLevelPropertyDef = searchBuilder
								.Vault
								.PropertyDefOperations
								.GetPropertyDef(indirectionLevel.ID);
						}
						catch
						{
							indirectionLevelPropertyDef = null;
						}

						// Does it exist?
						if (null == indirectionLevelPropertyDef)
							throw new ArgumentException
							(
								String.Format
								(
									Resources.Exceptions.MFSearchBuilderExtensionMethods.IndirectionLevelPropertyNotFound,
									indirectionLevel.ID
								),
								nameof(indirectionLevel)
							);

						// Is it a list-based one?
						if (false == indirectionLevelPropertyDef.BasedOnValueList)
							throw new ArgumentException
							(
								String.Format
								(
									Resources.Exceptions.MFSearchBuilderExtensionMethods.IndirectionLevelPropertyNotOfExpectedType,
									indirectionLevel.ID,
									indirectionLevelPropertyDef.DataType
								),
								nameof(indirectionLevel)
							);

						// Record the object type id.
						objectTypeId = indirectionLevelPropertyDef.ValueList;
					}

					// Is it an object type (fine) or a value list (not fine)?
					{
						ObjType indirectionLevelObjectType;
						try
						{
							indirectionLevelObjectType = searchBuilder
								.Vault
								.ValueListOperations
								.GetValueList(objectTypeId);
						}
						catch
						{
							indirectionLevelObjectType = null;
						}

						// Does it exist?
						if (null == indirectionLevelObjectType)
							throw new ArgumentException
							(
								String.Format
								(
									Resources.Exceptions.MFSearchBuilderExtensionMethods.IndirectionLevelPointsToInvalidObjectType,
									objectTypeId
								),
								nameof(indirectionLevel)
							);

						// If it's not a real object type then throw.
						if (false == indirectionLevelObjectType.RealObjectType)
							throw new ArgumentException
							(
								String.Format
								(
									Resources.Exceptions.MFSearchBuilderExtensionMethods.IndirectionLevelPointsToValueList,
									objectTypeId
								),
								nameof(indirectionLevel)
							);
					}

				}

				// Set the indirection levels.
				searchCondition.Expression.IndirectionLevels
					= indirectionLevels;
			}

			// Was the value null?
			if (null == value)
				searchCondition.TypedValue.SetValueToNULL(dataType);
			else
			{
				// For conditions for timestamps, be as precise as possible.
				if (value is DateTime d && dataType == MFDataType.MFDatatypeTimestamp)
					value = d.ToPreciseTimestamp();

				// Set the value.
				searchCondition.TypedValue.SetValue(dataType, value);
			}

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}
	}
}
