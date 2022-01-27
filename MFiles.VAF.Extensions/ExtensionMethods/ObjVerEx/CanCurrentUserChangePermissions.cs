using System;
using MFiles.VAF.Common;

using MFilesAPI;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Indicates whether the passed user can change permissions this object version.
		/// </summary>
		/// <param name="objVerEx">The ObjVerEx object.</param>
		/// <param name="sessionInfo">The session to check access for.</param>
		/// <returns>True if the user can change permissions this version, otherwise False.</returns>
		public static bool CanCurrentUserChangePermissions
		(
			this ObjVerEx objVerEx,
			SessionInfo sessionInfo
		)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (sessionInfo is null)
				throw new ArgumentNullException(nameof(sessionInfo));

			return sessionInfo.CheckObjectAccess(objVerEx.ACL, MFObjectAccess.MFObjectAccessChangePermissions);
		}
	}
}
