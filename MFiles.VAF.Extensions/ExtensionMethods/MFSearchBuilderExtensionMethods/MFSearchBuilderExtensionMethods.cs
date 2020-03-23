using System;
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
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels,
			DataFunctionCall dataFunctionCall
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
						{
							throw new ArgumentException($"An indirection level references a property definition with ID {indirectionLevel.ID}, but this property definition could not be found.", nameof(indirectionLevel));
						}

						// Is it a list-based one?
						if (false == indirectionLevelPropertyDef.BasedOnValueList)
						{
							throw new ArgumentException($"The indirection level for property {indirectionLevel.ID} does not reference a lookup-style property definition.", nameof(indirectionLevel));
						}

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
						{
							throw new ArgumentException($"An indirection level references a value list with ID {objectTypeId}, but this value list could not be found.", nameof(indirectionLevel));
						}

						// If it's not a real object type then throw.
						if (false == indirectionLevelObjectType.RealObjectType)
						{
							throw new ArgumentException($"An indirection level references an value list with ID {objectTypeId}, but this list does not refer to an object type (cannot be used with value lists).", nameof(indirectionLevel));
						}
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
				searchCondition.TypedValue.SetValue(dataType, value);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}
	}
}
