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
		/// Adds a <see cref="SearchCondition"/> to the collection for a full-text search on the value given.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="value">The value to search for.</param>
		/// <param name="searchFlags">The type of full text search to execute (defaults to <see cref="MFFullTextSearchFlags.MFFullTextSearchFlagsLookInFileData"/> | <see cref="MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData"/> ).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder FullText
		(
			this MFSearchBuilder searchBuilder,
			string value,
			MFFullTextSearchFlags searchFlags = MFFullTextSearchFlags.MFFullTextSearchFlagsLookInFileData
												// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
												| MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (null == value)
				throw new ArgumentNullException(nameof(value));

			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeContains
			};

			// Set up the any-field (full-text) expression.
			searchCondition.Expression.SetAnyFieldExpression
			(
				searchFlags
			);

			// Search for the given term.
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, value);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

	}
}
