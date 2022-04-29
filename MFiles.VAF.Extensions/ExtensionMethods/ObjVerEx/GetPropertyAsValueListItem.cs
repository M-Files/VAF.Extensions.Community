using System;

using MFiles.VAF.Common;
using MFiles.VAF.Configuration;

using MFilesAPI;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Get the value list item for the specified property def id.
		/// </summary>
		/// <param name="objVerEx">The child/owned object.</param>
		/// <param name="propDefId">The property definition id.</param>
		/// <returns>Value list item object if available</returns>
		/// <remarks>Can return null if the value list item is deleted or not set.</remarks>
		/// <exception cref="ArgumentException">Thrown if <paramref name="propDefId"/> does not point to a suitable property definition.</exception>
		public static ValueListItem GetPropertyAsValueListItem(
			this ObjVerEx objVerEx,
			int propDefId
		)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));

			// Validity of the property def id
			if (0 > propDefId)
			{
				throw new ArgumentOutOfRangeException
				(
					nameof(propDefId),
					Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved
				);
			}

			// Get the value list id of the property def
			PropertyDef propDef = objVerEx
				.Vault
				.PropertyDefOperations
				.GetPropertyDef(propDefId);

			// Exception if property was not found
			if (null == propDef)
			{
				throw new ArgumentException
				(
					String.Format(Resources.Exceptions.VaultInteraction.PropertyDefinition_NotFound, propDefId),
					nameof(propDefId)
				);
			}

			// Does this have an owning type?
			if (false == propDef.BasedOnValueList || 0 > propDef.ValueList)
			{
				throw new ArgumentException
				(
					String.Format
					(
						Resources.Exceptions.VaultInteraction.PropertyDefinition_NotBasedOnValueList,
						propDefId,
						propDef.Name
					),
					nameof(propDefId)
				);
			}

			// Get the lookup id of the property
			int lookupId = objVerEx.GetLookupID(propDefId);

			// Return null if lookup was not found
			if (lookupId < 0)
				return null;

			// return the value list item for the lookup id
			return objVerEx
				.Vault
				.ValueListItemOperations
				.GetValueListItemByID(propDef.ValueList, lookupId);
		}
	}
}
