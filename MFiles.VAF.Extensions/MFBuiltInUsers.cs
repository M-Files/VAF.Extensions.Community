namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Internal class for enum-style access to "built-in" users.
	/// </summary>
	internal static class MFBuiltInUsers
	{
		/// <summary>
		/// The user ID reported when code is run as the M-Files user
		/// (e.g. automatic state transitions, task processing).
		/// </summary>
		internal const int MFilesServerUserID = -102;
	}
}
