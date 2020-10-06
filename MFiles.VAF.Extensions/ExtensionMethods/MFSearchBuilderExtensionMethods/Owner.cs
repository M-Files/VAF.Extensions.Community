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
		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to find items with the given owner.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="owner">The owner item.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Owner
		(
			this MFSearchBuilder searchBuilder,
			ObjVerEx owner
		)
		{
			// Sanity.
			if (null == owner)
				throw new ArgumentNullException(nameof(owner));

			// Use the other overload.
			return searchBuilder.Owner(owner.ObjVer);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to find items with the given owner.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="owner">The owner item.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Owner
		(
			this MFSearchBuilder searchBuilder,
			ObjVer owner
		)
		{
			// Sanity.
			if (null == owner)
				throw new ArgumentNullException(nameof(owner));

			// Use the other overload.
			return searchBuilder.Owner(owner.ObjID);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to find items with the given owner.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="owner">The owner item.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder Owner
		(
			this MFSearchBuilder searchBuilder,
			ObjID objID
		)
		{
			// Sanity.
			if (null == objID)
				throw new ArgumentNullException(nameof(objID));

			// Get the owner object type.
			var ownerObjType = searchBuilder
				.Vault
				.ObjectTypeOperations
				.GetObjectType(objID.Type);

			// Use the other method.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				ownerObjType.OwnerPropertyDef,
				MFDataType.MFDatatypeLookup, // Owner must be single-select
				objID.ID,
				MFConditionType.MFConditionTypeEqual
			);
		}

	}
}
