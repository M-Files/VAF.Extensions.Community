using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public static partial class MFSearchBuilderExtensionMethods
	{

		/// <summary>
		/// Finds the number of items that are returned by this search.
		/// Wraps https://www.m-files.com/api/documentation/#MFilesAPI~VaultObjectSearchOperations~GetObjectCountInSearch.html.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="searchFlags">Any search flags to use.</param>
		/// <returns>The count.</returns>
		public static int FindCount
		(
			this MFSearchBuilder searchBuilder,
			MFSearchFlags searchFlags = MFSearchFlags.MFSearchFlagNone
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (null == searchBuilder.Vault)
				throw new ArgumentException(Resources.Exceptions.MFSearchBuilderExtensionMethods.VaultReferenceNull, nameof(searchBuilder));
			if (null == searchBuilder.Vault.ObjectSearchOperations)
				throw new ArgumentException(Resources.Exceptions.MFSearchBuilderExtensionMethods.VaultObjectSearchOperationsReferenceNull, nameof(searchBuilder));
			if (null == searchBuilder.Conditions)
				throw new ArgumentException(Resources.Exceptions.MFSearchBuilderExtensionMethods.SearchConditionsNull, nameof(searchBuilder));

			// Use the GetObjectCountInSearch API method.
			return searchBuilder
				.Vault
				.ObjectSearchOperations
				.GetObjectCountInSearch
				(
					searchBuilder.Conditions, 
					searchFlags
				);
		}
	}
}
