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
		/// Adds a <see cref="SearchCondition"/> to the collection to restrict objects by their ID.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="segmentIndex">The index of the segment to retrieve (starting at zero).</param>
		/// <param name="segmentSize">The size of the segment to retrieve (i.e. a size of 1000 would return items 0-999 in segment index zero).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		/// <remarks>
		/// The <paramref name="segmentIndex"/> and <paramref name="segmentSize"/> are used to calculate a range of object IDs,
		/// and objects with an ID in this range (that also match the other search conditions) will be returned.
		/// For example, a <paramref name="segmentIndex"/> of zero and a <paramref name="segmentSize"/> of 1000 will return objects with IDs between 0 and 999.
		/// For example, a <paramref name="segmentIndex"/> of one and a <paramref name="segmentSize"/> of 1000 will return objects with IDs between 1000 and 1999.
		/// </remarks>
		public static MFSearchBuilder ObjectIdSegment
		(
			this MFSearchBuilder searchBuilder,
			int segmentIndex,
			int segmentSize
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (segmentIndex < 0)
				throw new ArgumentOutOfRangeException(Resources.Exceptions.MFSearchBuilderExtensionMethods.ObjectIDs_SegmentIndexMustBeZeroOrLarger, nameof(segmentIndex));
			if (segmentSize <= 0)
				throw new ArgumentOutOfRangeException(Resources.Exceptions.MFSearchBuilderExtensionMethods.ObjectIDs_SegmentSizeMustBeOneOrLarger, nameof(segmentSize));

			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = MFConditionType.MFConditionTypeEqual
			};

			// Set up the segment size data.
			searchCondition.Expression.SetObjectIDSegmentExpression
			(
				segmentSize
			);

			// Search by the segment index.
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeInteger, segmentIndex);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to find items by <see cref="ObjID.ID"/>.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="objectId">The object ID to search for.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder ObjectId
		(
			this MFSearchBuilder searchBuilder,
			int objectId,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 >= objectId)
				throw new ArgumentOutOfRangeException(nameof(objectId), Resources.Exceptions.MFSearchBuilderExtensionMethods.ObjectIDs_ObjectIDMustBeGreaterThanZero);

			// We can only handle certain condition types; throw for others.
			if (conditionType != MFConditionType.MFConditionTypeEqual
				&&conditionType != MFConditionType.MFConditionTypeNotEqual
				&&conditionType != MFConditionType.MFConditionTypeLessThan
				&&conditionType != MFConditionType.MFConditionTypeLessThanOrEqual
				&&conditionType != MFConditionType.MFConditionTypeGreaterThan
				&&conditionType != MFConditionType.MFConditionTypeGreaterThanOrEqual)
			{
				throw new ArgumentException
				(
					String.Format
					(
						Resources.Exceptions.MFSearchBuilderExtensionMethods.ObjectIDs_ConditionTypeInvalid,
						conditionType
					),
					nameof(conditionType)
				);
			}

			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = conditionType
			};

			// Set up the expression.
			searchCondition.Expression.SetStatusValueExpression
			(
				MFStatusType.MFStatusTypeObjectID
			);

			// Set the object id.
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeInteger, objectId);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

	}
}
