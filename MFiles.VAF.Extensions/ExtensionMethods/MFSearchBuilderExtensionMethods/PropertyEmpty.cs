using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{/// <summary>
 /// Extension methods for the <see cref="MFSearchBuilder"/> class.
 /// </summary>
	// ReSharper disable once InconsistentNaming
	public static partial class MFSearchBuilderExtensionMethods
	{
		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to check whether a property is empty
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		/// <remarks>This will not find objects that lack the property entirely.</remarks>
		public static MFSearchBuilder PropertyEmpty
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// Add the search condition.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				propertyDef,
				dataType,
				null,
				MFConditionType.MFConditionTypeEqual,
				parentChildBehavior,
				indirectionLevels,
				null
			);
		}

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection to check whether a property is not empty
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="propertyDef">The ID of the property to search by.</param>
		/// <param name="parentChildBehavior">Whether to accept matches to parent/child values as well (defaults to <see cref="MFParentChildBehavior.MFParentChildBehaviorNone"/>).</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder PropertyNotEmpty
		(
			this MFSearchBuilder searchBuilder,
			int propertyDef,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			PropertyDefOrObjectTypes indirectionLevels = null
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

			// What is the type of this property?
			var dataType = searchBuilder.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

			// Add the search condition.
			return searchBuilder.AddPropertyValueSearchCondition
			(
				propertyDef,
				dataType,
				null,
				MFConditionType.MFConditionTypeNotEqual,
				parentChildBehavior,
				indirectionLevels,
				null
			);
		}
	}
}
