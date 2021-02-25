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
		/// Executes a method for each <see cref="ObjectVersion"/> using a segmented search.
		/// </summary>
		/// <param name="builder">Search Builder to use for queries.</param>
		/// <param name="objectVersionDelegate">Method to execute for each object.</param>
		/// <param name="startSegment">The (zero-based) index of the segment to start at.</param>
		/// <param name="segmentLimit">The number of total segments to process. See <see cref="DefaultMaximumSegmentIndex"/>.</param>
		/// <param name="segmentSize">The number of items to include in each segment. See <see cref="DefaultNumberOfItemsInSegment"/>.</param>
		/// <param name="searchTimeoutInSeconds">The timeout for each search. See <see cref="DefaultSearchTimeoutInSeconds"/>. Zero indicates indefinite timeout.</param>
		/// <returns>Total count of objects matching conditions.</returns>
		/// <remarks>Note that <paramref name="searchTimeoutInSeconds"/> applies to the timeout on each segment search; if multiple segments are needed then the maximum time that this method takes to return will exceed the provided value.</remarks>
		public static long ForEach(
			this MFSearchBuilder builder,
			Action<ObjectVersion> objectVersionDelegate,
			int startSegment = 0,
			int segmentLimit = MFSearchBuilderExtensionMethods.DefaultMaximumSegmentIndex,
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment,
			int searchTimeoutInSeconds = MFSearchBuilderExtensionMethods.DefaultSearchTimeoutInSeconds )
		{
			// Sanity checks
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));
			if (objectVersionDelegate == null)
				throw new ArgumentNullException(nameof(objectVersionDelegate));
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
				var searchResults = newBuilder.Find(searchTimeoutInSeconds: searchTimeoutInSeconds);

				foreach (ObjectVersion searchResult in searchResults)
				{
					objectVersionDelegate(searchResult);
				}

				return searchResults.Count;
			}

			var resultCount = ForEachSegment(builder, DefaultSearchHandler, startSegment, segmentLimit, segmentSize, searchTimeoutInSeconds);
			return resultCount;
		}

		/// <summary>
		/// Executes a method for each <see cref="ObjVerEx"/> using a segmented search.
		/// </summary>
		/// <param name="builder">Search Builder to use for queries.</param>
		/// <param name="objVerExDelegate">Method to execute for each object.</param>
		/// <param name="startSegment">The (zero-based) index of the segment to start at.</param>
		/// <param name="segmentLimit">The number of total segments to process. See <see cref="DefaultMaximumSegmentIndex"/>.</param>
		/// <param name="segmentSize">The number of items to include in each segment. See <see cref="DefaultNumberOfItemsInSegment"/>.</param>
		/// <param name="searchTimeoutInSeconds">The timeout for each search. See <see cref="DefaultSearchTimeoutInSeconds"/>. Zero indicates indefinite timeout.</param>
		/// <returns>Total count of objects matching conditions.</returns>
		/// <remarks>Note that <paramref name="searchTimeoutInSeconds"/> applies to the timeout on each segment search; if multiple segments are needed then the maximum time that this method takes to return will exceed the provided value.</remarks>
		public static long ForEachEx(
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

			var resultCount = ForEachSegment(builder, DefaultSearchHandler, startSegment, segmentLimit, segmentSize, searchTimeoutInSeconds);
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
		/// <param name="searchTimeoutInSeconds">The timeout for each search. See <see cref="DefaultSearchTimeoutInSeconds"/>. Zero indicates indefinite timeout.</param>
		/// <returns>Total count of objects across vault.</returns>
		/// <remarks>Note that <paramref name="searchTimeoutInSeconds"/> applies to the timeout on each segment search; if multiple segments are needed then the maximum time that this method takes to return will exceed the provided value.</remarks>
		internal static long ForEachSegment(
			this MFSearchBuilder builder,
			Func<Vault, SearchConditions, int> func,
			int startSegment = 0,
			int segmentLimit = MFSearchBuilderExtensionMethods.DefaultMaximumSegmentIndex,
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment,
			int searchTimeoutInSeconds = MFSearchBuilderExtensionMethods.DefaultSearchTimeoutInSeconds)
		{
			// Sanity.
			if (null == func)
				throw new ArgumentNullException(nameof(func));
			if (startSegment < 0)
				throw new ArgumentOutOfRangeException(nameof(startSegment), "The start segment must be greater than or equal to zero.");
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentSize), "The segment size must be greater than zero.");
			if (searchTimeoutInSeconds < 0)
				throw new ArgumentOutOfRangeException(nameof(searchTimeoutInSeconds), "The search timeout must be greater than zero, or zero to indicate an indefinite timeout");

			// Set our start values.
			var segment = startSegment;
			long resultCount = 0; // The total number of matched items in all segments.

			// Iterate over segments until we hit the sanity limit,
			// or until there are no items left to find.
			while (segment < segmentLimit)
			{
				// Make a copy of the builder that we can interact with and not
				// affect the one passed in.
				var internalBuilder = new MFSearchBuilder(builder.Vault, builder.Conditions);

				// Add a condition for the current segment that we want.
				internalBuilder.ObjectIdSegment(segment, segmentSize);

				// Execute the provided function.
				// This must return a count of items that were in the current segment,
				// but it may also execute other code against the items, depending on
				// what the calling function needs.
				var searchResultsCount = func(internalBuilder.Vault, internalBuilder.Conditions);

				// If we got no items back then we need to check whether a higher segment has items.
				if (searchResultsCount == 0)
				{
					// Recreate the internal builder.
					internalBuilder = new MFSearchBuilder(builder.Vault, builder.Conditions);

					// Add a condition to see whether there are any items that have an ID in a higher segment.
					internalBuilder.MinObjId(segment + 1, segmentSize);

					// Find any matching items that exist in a higher segment.
					var resultsTopId = internalBuilder
						.Vault
						.ObjectSearchOperations
						.SearchForObjectsByConditionsEx
						(
							internalBuilder.Conditions,
							MFSearchFlags.MFSearchFlagDisableRelevancyRanking,
							SortResults: false,
							MaxResultCount: 1,
							SearchTimeoutInSeconds: searchTimeoutInSeconds
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
		/// Creates a search condition using the minimum object id for use in segmented search.
		/// </summary>
		/// <param name="searchBuilder">Search Builder to add condition to.</param>
		/// <param name="segment">The segment (starting at one) to retrieve.</param>
		/// <param name="segmentSize">The number of items in the segment.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		/// <remarks>Note that <paramref name="segment"/>  has a minimum of 1 because it is used to find any items in the next segment.</remarks>
		internal static MFSearchBuilder MinObjId(this MFSearchBuilder searchBuilder, int segment, int segmentSize)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (segment <= 0)
				throw new ArgumentOutOfRangeException(nameof(segment), "The segment must be greater than zero");
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(segmentSize), "The segmentSize must be greater than zero");

			// Add a minimum object id condition
			searchBuilder.ObjectId(segmentSize * segment, MFConditionType.MFConditionTypeGreaterThanOrEqual);

			return searchBuilder;
		}
	}
}
