using System;
using System.Collections.Generic;

using MFiles.VAF.Common;

using MFilesAPI;

namespace MFiles.VAF.Extensions
{
    public static partial class ObjVerExExtensionMethods
    {
        /// <summary>
        /// Get the list of value list items for the specified property def id. (for multiselect lookups)
        /// </summary>
        /// <param name="objVerEx">The child/owned object.</param>
        /// <param name="propDefId">The property definition id.</param>
        /// <returns>List of value list item objects if available</returns>
        /// <remarks>Can return null entries within the list if the value list item is deleted or not set.</remarks>
        /// <exception cref="ArgumentException">Thrown if <paramref name="propDefId"/> does not point to a suitable property definition.</exception>
        public static List<ValueListItem> GetPropertyAsValueListItems(
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
                throw new ArgumentOutOfRangeException(
                    nameof(propDefId),
                    "Property Ids must be greater than -1; ensure that your property alias was resolved."
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
                throw new ArgumentException(
                    $"The property could not be found.",
                    nameof(propDefId)
                );
            }

            // Does this have an owning type?
            if (false == propDef.BasedOnValueList || 0 > propDef.ValueList)
            {
                throw new ArgumentException(
                    $"The property \"{propDef.Name}\" is not based on a value list.",
                    nameof(propDefId)
                );
            }

            // Does this have an owning type?
            if ((int)MFDataType.MFDatatypeMultiSelectLookup != (int)propDef.DataType)
            {
                throw new ArgumentException(
                    $"The property \"{propDef.Name}\" is no multi select lookup property.",
                    nameof(propDefId)
                );
            }

            // Get the lookup elements and initialize result list
            IEnumerable<Lookup> lookups = objVerEx.GetLookupsFromProperty(propDefId);
            List<ValueListItem> listResults = new List<ValueListItem>();
            VaultValueListItemOperations vliOps = objVerEx.Vault.ValueListItemOperations;

            // Loop through the lookup elements
            foreach (Lookup lookup in lookups)
            {
                listResults.Add(
                    vliOps.GetValueListItemByID(propDef.ValueList, lookup.Item)
                );
            }

            // return the list with the value list items for the multiselect lookup entries
            return listResults;
        }
    }
}
