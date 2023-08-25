using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions
{
    internal static class MFNamedValuesExtensionMethods
    {
        /// <summary>
        /// Returns the value corresponding to key, or "" if no such value.
        /// </summary>
        /// <param name="key">Key to use with NamedValues.</param>
        /// <returns></returns>
        public static string GetValueOrEmpty(this NamedValues namedValues, string key)
        {
            try
            {
                return namedValues[key].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
