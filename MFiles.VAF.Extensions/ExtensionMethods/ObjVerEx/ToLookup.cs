using MFiles.VAF.Common;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
        /// <summary>
        /// Function to return MFilesAPI.Lookup representation of this MFiles.VAF.Common.ObjVerEx with a specific lookup version or the always latest
        /// </summary>
        /// <param name="objVerEx">The object version to check.</param>
        /// <param name="latestVersion">false return the specific latest version</param>
        /// <returns>MFilesAPI.Lookup representation of this MFiles.VAF.Common.ObjVerEx</returns>
        public static Lookup ToLookup(this ObjVerEx objVerEx, bool latestVersion)
        {
            // Sanity.
            if (null == objVerEx)
                throw new ArgumentNullException(nameof(objVerEx));

            // Get the standard implementation with version data.
            var lookup = objVerEx.ToLookup();

            // If we want the version data then we're all good.
            if (!latestVersion)
                return lookup;

            // Latest version: remove the version data and return.
            lookup.Version = -1;

            return lookup;
        }
    }
}
