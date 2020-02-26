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
		/// Adds a <see cref="SearchCondition"/> to the collection to restrict files by their size.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="hasFiles">Whether to include items with files (true) or include items without files (false).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder HasFiles
		(
			this MFSearchBuilder searchBuilder,
			bool hasFiles
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

			// Set up the file value expression.
			searchCondition.Expression.SetFileValueExpression
			(
				MFFileValueType.MFFileValueTypeHasFiles
			);

			// Search by the size
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, hasFiles);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

	}
}
