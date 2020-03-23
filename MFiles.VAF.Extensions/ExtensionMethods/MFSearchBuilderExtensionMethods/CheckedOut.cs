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
		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to find items by their checkout status.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="isCheckedOut">Whether to include items that are checked out (true) or include items that are not checked out (false).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder IsCheckedOut
		(
			this MFSearchBuilder searchBuilder,
			bool isCheckedOut = false
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));

			
			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeEqual
			};

			// Set up the status value expression.
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeCheckedOut);

			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, isCheckedOut);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

	}
}
