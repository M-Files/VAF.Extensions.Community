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
		/// <returns></returns>
		private static MFSearchBuilder AddPropertyValueSearchCondition
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			MFDataType dataType,
			object value,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
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
