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
		#region Segmented Search

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
		public static long ForEach(
			this MFSearchBuilder builder,
			Action<ObjVerEx> objVerExDelegate,
			int startSegment = 0,
			int segmentLimit = MFSearchBuilderExtensionMethods.DefaultMaximumSegmentIndex,
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment,
			int searchTimeoutInSeconds = MFSearchBuilderExtensionMethods.DefaultSearchTimeoutInSeconds )
		{
			// Sanity checks
			if( builder == null )
				throw new ArgumentNullException( nameof(builder) );
			if( objVerExDelegate == null )
				throw new ArgumentNullException( nameof(objVerExDelegate) );
			if( startSegment < 0 )
				throw new ArgumentOutOfRangeException( nameof(startSegment), "value must be greater than or equal to 0" );
			if( segmentLimit <= 0 )
				throw new ArgumentOutOfRangeException( nameof(segmentLimit), "value must be greater 0" );
			if( segmentSize <= 0 )
				throw new ArgumentOutOfRangeException( nameof(segmentSize), "value must be greater than 0" );
			if( searchTimeoutInSeconds <= 0 )
				throw new ArgumentOutOfRangeException( nameof(searchTimeoutInSeconds), "value must be greater than 0" );

			int DefaultSearchHandler( Vault vault, SearchConditions searchConditions )
			{
				var newBuilder = new MFSearchBuilder( vault, searchConditions );
				var searchResults = newBuilder.FindEx( searchTimeoutInSeconds: searchTimeoutInSeconds );

				searchConditions.Remove( searchConditions.Count );

				if( searchConditions.Count <= 0 )
					return searchResults.Count;

				foreach( var searchResult in searchResults )
				{
					objVerExDelegate( searchResult );
				}

				return searchResults.Count;
			}

			var resultCount = ForEachSegment( builder, DefaultSearchHandler, startSegment, segmentLimit, segmentSize );
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
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment )
		{
			// Sanity checks
			if( builder == null )
				throw new ArgumentNullException( nameof(builder) );
			if( startSegment < 0 )
				throw new ArgumentOutOfRangeException( nameof(startSegment), "value must be greater than or equal to 0" );
			if( segmentLimit <= 0 )
				throw new ArgumentOutOfRangeException( nameof(segmentLimit), "value must be greater 0" );
			if( segmentSize <= 0 )
				throw new ArgumentOutOfRangeException( nameof(segmentSize), "value must be greater than 0" );

			return ForEachSegment( builder,
				// Note: this method is required because func needs to return the count to be summed by ForEachSegment
				( vault, conditions ) => vault.ObjectSearchOperations.GetObjectCountInSearch( conditions, MFSearchFlags.MFSearchFlagDisableRelevancyRanking ),
				startSegment,
				segmentLimit,
				segmentSize );
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
		private static long ForEachSegment(
			this MFSearchBuilder builder,
			Func<Vault, SearchConditions, int> func,
			int startSegment = 0,
			int segmentLimit = MFSearchBuilderExtensionMethods.DefaultMaximumSegmentIndex,
			int segmentSize = MFSearchBuilderExtensionMethods.DefaultNumberOfItemsInSegment )
		{
			var segment = startSegment;
			long resultCount = 0;

			while( segment < segmentLimit )
			{
				var searchConditions = builder.Conditions.Clone();

				searchConditions.Add( -1, SearchConditionSegment( segment, segmentSize ) );
				var searchResultsCount = func( builder.Vault, searchConditions );

				searchConditions.Remove( searchConditions.Count );

				if( searchResultsCount == 0 )
				{
					var searchConditionsTopId = builder.Conditions.Clone();

					searchConditionsTopId.Add( -1, SearchConditionMinObjId( segment, segmentSize ) );
					var resultsTopId = builder.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx( searchConditionsTopId, MFSearchFlags.MFSearchFlagDisableRelevancyRanking, false, 1 );
					if( resultsTopId.Count == 0 )
					{
						break;
					}
				}

				resultCount += searchResultsCount;

				segment += 1;
			}

			return resultCount;
		}

		/// <summary>
		/// Creates a search condition for a segment to use in segmented search.
		/// </summary>
		/// <param name="segment">Used to calculate the segment expression.</param>
		/// <param name="range">Used to calculate the segment expression.</param>
		/// <returns></returns>
		private static SearchCondition SearchConditionSegment( int segment, int range )
		{
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeEqual
			};
			searchCondition.Expression.SetObjectIDSegmentExpression( range );
			searchCondition.TypedValue.SetValue( MFDataType.MFDatatypeInteger, segment );
			return searchCondition;
		}

		/// <summary>
		/// Creates a search condition using the minimum object id for use in segmented search.
		/// </summary>
		/// <param name="segment">Used to calculate the minimum Id.</param>
		/// <param name="range">Used to calculate the minimum Id.</param>
		/// <returns></returns>
		private static SearchCondition SearchConditionMinObjId( int segment, int range )
		{
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeGreaterThanOrEqual
			};
			searchCondition.Expression.SetStatusValueExpression( MFStatusType.MFStatusTypeObjectID );
			searchCondition.TypedValue.SetValue( MFDataType.MFDatatypeInteger, range * segment );
			return searchCondition;
		}

		#endregion Segmented Search

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
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null,
			DataFunctionCall dataFunctionCall = null
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
