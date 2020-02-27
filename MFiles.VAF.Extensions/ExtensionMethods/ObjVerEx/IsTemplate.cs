using MFiles.VAF.Common;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Returns whether this <see cref="ObjVerEx"/> has the
		/// <see cref="MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate"/>
		/// property set, and whether this property is set to true.
		/// </summary>
		/// <param name="objVerEx">The object version to check.</param>
		/// <returns>
		/// True if it both has the
		/// <see cref="MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate"/> property
		/// and that property's value is true.</returns>
		public static bool IsTemplate(this ObjVerEx objVerEx)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if(null == objVerEx.Properties
				|| 0 == objVerEx.Properties.Count)
				return false;

			// Use the HasPropertyFlag method.
			return objVerEx.HasPropertyFlag
			(
				(int) MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate,
				defaultValue: false
			);

		}
	}
}
