using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Return list of lookup ids for a property, works on lookup and lookups.
		/// Returns empty list, if property is missing from the object or no valid lookups were found.
		/// </summary>
		/// <param name="objVerEx"> Object to get lookups </param>
		/// <param name="property"> property to get lookups for </param>
		/// <param name="includeDeleted"> Are deleted lookups included or not. Default is that deleted lookups are not included.</param>
		/// <returns> list of ids, empty list if no lookups found or property is missing </returns>
		public static List<int> GetLookupIDs(this ObjVerEx objVerEx, MFIdentifier property, bool includeDeleted = false)
		{
			var lookupIDs = new List<int>();
			PropertyValue pv = objVerEx.GetProperty(property);
			// Check the property's lookups.
			if (pv != null && !pv.Value.IsNULL() &&
				(pv.Value.DataType == MFDataType.MFDatatypeLookup ||
					pv.Value.DataType == MFDataType.MFDatatypeMultiSelectLookup))
			{
				Lookups lks = pv.Value.GetValueAsLookups();
				foreach (Lookup lookup in lks)
				{
					if (!lookupIDs.Contains(lookup.Item))
					{
						// If the lookup is deleted, then based on the includeDeleted parameter determines is the value added or not
						if (lookup.Deleted && !includeDeleted)
							continue;

						lookupIDs.Add(lookup.Item);
					}
				}
			}

			return lookupIDs;
		}

	}
}
