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
		/// <param name="size">The file size to restrict by (in bytes).</param>
		/// <param name="conditionType">What type of search to execute.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder FileSize
		(
			this MFSearchBuilder searchBuilder,
			long size,
			MFConditionType conditionType
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (size < 0)
				throw new ArgumentOutOfRangeException($"The size parameter must be zero or larger.", nameof(size));

			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = conditionType
			};

			// Set up the file value expression.
			searchCondition.Expression.SetFileValueExpression
			(
				MFFileValueType.MFFileValueTypeFileSize
			);

			// Search by the size
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeInteger64, size);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

	}
}
