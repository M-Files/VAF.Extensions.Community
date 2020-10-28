using MFiles.VAF.Common;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions
{
	public static partial class MFSearchBuilderExtensionMethods
	{

		/// <summary>
		/// The number of items to match in one segment.
		/// </summary>
		/// <remarks>Making it larger will reduce queries to M-Files, but higher chance of timeout.</remarks>
		public const int DefaultNumberOfItemsInSegment = 1000;

		/// <summary>
		/// The maximum index segment (must be greater than zero).
		/// </summary>
		public const int DefaultMaximumSegmentIndex = 100001;

		/// <summary>
		/// The default search timeout in seconds.
		/// </summary>
		public const int DefaultSearchTimeoutInSeconds = 10;

		/// <summary>
		/// Executes a method for each <see cref="ObjVerEx"/> using a segmented search.
		/// </summary>
		/// <param name="builder">Search Builder to use for queries.</param>
		/// <param name="objVerExDelegate">Method to execute for each object.</param>
		/// <param name="startSegment">The (zero-based) index of the segment to start at.</param>
		/// <param name="segmentLimit">The number of total segments to process. See <see cref="DefaultMaximumSegmentIndex"/>.</param>
		/// <param name="segmentSize">The number of items to include in each segment. See <see cref="DefaultNumberOfItemsInSegment"/>.</param>
		/// <param name="searchTimeoutInSeconds">The timeout for each search. See <see cref="DefaultSearchTimeoutInSeconds"/>.</param>
		/// <returns>Total count of objects matching conditions.</returns>
		/// <remarks>Note that <paramref name="searchTimeoutInSeconds"/> applies to the timeout on each segment search; if multiple segments are needed then the maximum time that this method takes to return will exceed the provided value.</remarks>
		public static long ForEach(
			this MFSearchBuilder builder,
			Action<ObjVerEx> objVerExDelegate,
			int startSegment = 0,
			int segmentLimit = MFSearchBuilderExtensionMethods.DefaultMaximumSegmentIndex,
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment,
			int searchTimeoutInSeconds = MFSearchBuilderExtensionMethods.DefaultSearchTimeoutInSeconds)
		{
			// Sanity checks
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));
			if (objVerExDelegate == null)
				throw new ArgumentNullException(nameof(objVerExDelegate));
			if (startSegment < 0)
				throw new ArgumentOutOfRangeException(nameof(startSegment), "value must be greater than or equal to 0");
			if (segmentLimit <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentLimit), "value must be greater 0");
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentSize), "value must be greater than 0");
			if (searchTimeoutInSeconds < 0)
				throw new ArgumentOutOfRangeException(nameof(searchTimeoutInSeconds), "The search timeout must be greater than zero, or zero to indicate an indefinite timeout");

			int DefaultSearchHandler(Vault vault, SearchConditions searchConditions)
			{
				var newBuilder = new MFSearchBuilder(vault, searchConditions);
				var searchResults = newBuilder.FindEx(searchTimeoutInSeconds: searchTimeoutInSeconds);

				foreach (var searchResult in searchResults)
				{
					objVerExDelegate(searchResult);
				}

				return searchResults.Count;
			}

			var resultCount = ForEachSegment(builder, DefaultSearchHandler, startSegment, segmentLimit, segmentSize);
			return resultCount;
		}

		/// <summary>
		/// Executes a segmented search in order to count all matching items in vault.
		/// </summary>
		/// <param name="builder">Search Builder to use for queries.</param>
		/// <param name="startSegment">The (zero-based) index of the segment to start at.</param>
		/// <param name="segmentLimit">The number of total segments to process. See <see cref="DefaultMaximumSegmentIndex"/>.</param>
		/// <param name="segmentSize">The number of items to include in each segment. See <see cref="DefaultNumberOfItemsInSegment"/>.</param>
		/// <returns>Total count of objects matching conditions.</returns>
		public static long SegmentedCount(
			this MFSearchBuilder builder,
			int startSegment = 0,
			int segmentLimit = MFSearchBuilderExtensionMethods.DefaultMaximumSegmentIndex,
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment)
		{
			// Sanity checks
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));
			if (startSegment < 0)
				throw new ArgumentOutOfRangeException(nameof(startSegment), "value must be greater than or equal to 0");
			if (segmentLimit <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentLimit), "value must be greater 0");
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentSize), "value must be greater than 0");

			return ForEachSegment(builder,
				// Note: this method is required because func needs to return the count to be summed by ForEachSegment
				(vault, conditions) => vault.ObjectSearchOperations.GetObjectCountInSearch(conditions, MFSearchFlags.MFSearchFlagDisableRelevancyRanking),
				startSegment,
				segmentLimit,
				segmentSize);
		}

		/// <summary>
		/// Runs a method for each segment in the vault.
		/// </summary>
		/// <param name="builder">Search Builder to use for queries.</param>
		/// <param name="func">Method to be executed for each segment, takes a vault and search conditions and returns count.</param>
		/// <param name="startSegment">The (zero-based) index of the segment to start at.</param>
		/// <param name="segmentLimit">The number of total segments to process. See <see cref="DefaultMaximumSegmentIndex"/>.</param>
		/// <param name="segmentSize">The number of items to include in each segment. See <see cref="DefaultNumberOfItemsInSegment"/>.</param>
		/// <returns>Total count of objects across vault.</returns>
		internal static long ForEachSegment(
			this MFSearchBuilder builder,
			Func<Vault, SearchConditions, int> func,
			int startSegment = 0,
			int segmentLimit = MFSearchBuilderExtensionMethods.DefaultMaximumSegmentIndex,
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment)
		{
			// Sanity.
			if (null == func)
				throw new ArgumentNullException(nameof(func));
			if (startSegment < 0)
				throw new ArgumentOutOfRangeException(nameof(startSegment), "The start segment must be greater than or equal to zero.");
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentSize), "The segment size must be greater than zero.");

			// Set our start values.
			var segment = startSegment;
			long resultCount = 0; // The total number of matched items in all segments.

			// Iterate over segments until we hit the sanity limit,
			// or until there are no items left to find.
			while (segment < segmentLimit)
			{
				// Clone the search conditions from the supplied builder.
				var searchConditions = builder.Conditions.Clone();

				// Add a condition for the current segment that we want.
				searchConditions.Add(-1, SearchConditionSegment(segment, segmentSize));

				// Execute the provided function.
				// This must return a count of items that were in the current segment,
				// but it may also execute other code against the items, depending on
				// what the calling function needs.
				var searchResultsCount = func(builder.Vault, searchConditions);

				// Remove the condition for the segment.
				searchConditions.Remove(searchConditions.Count);

				// If we got no items back then we need to check whether a higher segment has items.
				if (searchResultsCount == 0)
				{
					// Clone the original builder conditions.
					var searchConditionsTopId = builder.Conditions.Clone();

					// Add a condition to see whether there are any items that have an ID in a higher segment.
					searchConditionsTopId.Add(-1, SearchConditionMinObjId(segment, segmentSize));

					// Find any matching items that exist in a higher segment.
					var resultsTopId = builder
						.Vault
						.ObjectSearchOperations
						.SearchForObjectsByConditionsEx
						(
							searchConditionsTopId,
							MFSearchFlags.MFSearchFlagDisableRelevancyRanking,
							SortResults: false,
							MaxResultCount: 1
						);

					// If there are none then break out of the while loop
					// as there is no point checking further segments.
					if (resultsTopId.Count == 0)
					{
						break;
					}
				}

				// Increment the total count by the count in this segment.
				resultCount += searchResultsCount;

				// Advance to the next segment.
				segment += 1;
			}

			// Return the total number of matched items across all segments.
			return resultCount;
		}

		/// <summary>
		/// Creates a search condition for a segment to use in segmented search.
		/// </summary>
		/// <param name="segment">The segment (starting at zero) to retrieve.</param>
		/// <param name="segmentSize">The number of items in the segment.</param>
		/// <returns>A <see cref="SearchCondition"/> that represents finding items that have object IDs in the provided segment.</returns>
		/// <remarks>
		/// A <paramref name="segment"/> of zero and <paramref name="segmentSize"/> of 1000 will return items with IDs between 1 and 1000.
		/// A <paramref name="segment"/> of one and <paramref name="segmentSize"/> of 1000 will return items with IDs between 1001 and 2000.
		/// </remarks>
		internal static SearchCondition SearchConditionSegment(int segment, int segmentSize)
		{
			// Sanity.
			if (segment < 0)
				throw new ArgumentOutOfRangeException(nameof(segment), "The segment must be greater than or equal to zero");
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentSize), "The segmentSize must be greater than zero");

			// Create and return the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeEqual
			};
			searchCondition.Expression.SetObjectIDSegmentExpression(segmentSize);
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeInteger, segment);
			return searchCondition;
		}

		/// <summary>
		/// Creates a search condition using the minimum object id for use in segmented search.
		/// </summary>
		/// <param name="segment">The segment (starting at zero) to retrieve.</param>
		/// <param name="segmentSize">The number of items in the segment.</param>
		/// <returns>A <see cref="SearchCondition"/> that represents finding items that have the correct minimum object ID.</returns>
		internal static SearchCondition SearchConditionMinObjId(int segment, int segmentSize)
		{
			// Sanity.
			if (segment < 0)
				throw new ArgumentOutOfRangeException(nameof(segment), "The segment must be greater than or equal to zero");
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentSize), "The segmentSize must be greater than zero");

			// Create and return the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeGreaterThanOrEqual
			};
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectID);
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeInteger, segmentSize * segment);
			return searchCondition;
		}

	}
}
