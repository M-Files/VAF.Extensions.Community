using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Extensions;
using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Extension methods for the <see cref="MFSearchBuilder"/> class.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static partial class MFSearchBuilderExtensionMethods
	{
		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to represent a "one of" search condition (https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#executing-a-one-of-search).
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="lookupIds">The ids to search for (the object must have the property set to one of these IDs).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyOneOf
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			IEnumerable<int> lookupIds,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), "Property Ids must be greater than -1; ensure that your property alias was resolved.");
			if (null == lookupIds || false == lookupIds.Any())
				throw new ArgumentException("The lookup collection does not contain any items.", nameof(lookupIds));

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// If it is not valid then throw.
			if (dataType != MFDataType.MFDatatypeLookup && dataType != MFDataType.MFDatatypeMultiSelectLookup)
				throw new ArgumentException($"Property {propertyDef} is not a lookup or multi-select lookup property.", nameof(propertyDef));

			// Add the search condition.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				propertyDef,
				MFDataType.MFDatatypeMultiSelectLookup,
				lookupIds.ToArray(),
				MFConditionType.MFConditionTypeEqual,
				parentChildBehavior,
				indirectionLevels
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to represent a "one of" search condition (https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#executing-a-one-of-search).
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="identifiers">The items to search for (the object must have the property set to one of these).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyOneOf
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			IEnumerable<MFIdentifier> identifiers,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Use the other overload.
			return searchBuilder?.PropertyOneOf
			(
				propertyDef,
				identifiers?
					.Where(i => i != null)
					.Select(identifier =>
					{
						// Ensure that we're resolved.
						try
						{
							return identifier
								.Resolve(searchBuilder?.Vault, typeof(PropertyDef))
								.ID;
						}
						catch(Exception e)
						{
							throw new Exception($"Could not resolve the identifier with alias/GUID {identifier.Alias} in the current vault", e);
						}
					}),
				parentChildBehavior,
				indirectionLevels
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to represent a "one of" search condition (https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#executing-a-one-of-search).
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="objects">The items to search for (the object must have the property set to one of these).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyOneOf
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			IEnumerable<ObjVer> objects,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Use the other overload.
			return searchBuilder?.PropertyOneOf
			(
				propertyDef,
				objects?
					.Where(o => o != null)?
					.Select(objVer => objVer.ID),
				parentChildBehavior,
				indirectionLevels
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to represent a "one of" search condition (https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#executing-a-one-of-search).
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="objects">The items to search for (the object must have the property set to one of these).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyOneOf
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			IEnumerable<ObjID> objects,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Use the other overload.
			return searchBuilder.PropertyOneOf
			(
				propertyDef,
				objects?
					.Where(o => o != null)?
					.Select(objID => objID.ID),
				parentChildBehavior,
				indirectionLevels
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to represent a "one of" search condition (https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#executing-a-one-of-search).
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="objects">The items to search for (the object must have the property set to one of these).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyOneOf
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			IEnumerable<ObjVerEx> objects,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Use the other overload.
			return searchBuilder.PropertyOneOf
			(
				propertyDef,
				objects?.Where(o => o != null)?.Select(objVerEx => objVerEx.ID),
				parentChildBehavior,
				indirectionLevels
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to represent a "one of" search condition (https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#executing-a-one-of-search).
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="lookups">The items to search for (the object must have the property set to one of these).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyOneOf
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			IEnumerable<Lookup> lookups,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Use the other overload.
			return searchBuilder?.PropertyOneOf
			(
				propertyDef,
				lookups?.Select(lookup => lookup.Item),
				parentChildBehavior,
				indirectionLevels
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to represent a "one of" search condition (https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#executing-a-one-of-search).
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="lookups">The items to search for (the object must have the property set to one of these).</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyOneOf
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			Lookups lookups,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Use the other overload.
			return searchBuilder?.PropertyOneOf
			(
				propertyDef,
				lookups?
					.Cast<Lookup>()?
					.Where(l => l != null)?
					.Select(lookup => lookup.Item),
				parentChildBehavior,
				indirectionLevels
			);
		}
	}
}
