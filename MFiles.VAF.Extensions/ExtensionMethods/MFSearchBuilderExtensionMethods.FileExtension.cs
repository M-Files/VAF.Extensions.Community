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
		/// Adds a <see cref="SearchCondition"/> to the collection to restrict items just to files of the provided type.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="extension">The file extension to restrict by.  If this does not start with a "." then the method will add it.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder FileExtension
		(
			this MFSearchBuilder searchBuilder,
			string extension
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (null == extension)
				throw new ArgumentNullException(nameof(extension));

			// Ensure it starts with a dot.
			if(false == extension.StartsWith("."))
				extension = "." + extension;

			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeContains
			};

			// Set up the file value expression.
			searchCondition.Expression.SetFileValueExpression
			(
				MFFileValueType.MFFileValueTypeFileName
			);

			// Search for the extension.
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, extension);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

	}
}
