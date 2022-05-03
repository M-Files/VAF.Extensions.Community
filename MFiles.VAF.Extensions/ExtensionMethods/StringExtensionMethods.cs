using System;

namespace MFiles.VAF.Extensions
{
	public static class StringExtensionMethods
	{
		/// <summary>
		/// Escapes <paramref name="input"/> so that it can be used in XML.
		/// </summary>
		/// <param name="input">The string to escape.</param>
		/// <returns>The escaped string, or null if <paramref name="input"/> is null.</returns>
		public static string EscapeXmlForDashboard(this string input)
		{
			return input.EscapeXmlForDashboard(null);
		}

		/// <summary>
		/// Runs <paramref name="format"/> through <see cref="string.Format(string, object[])"/>,
		/// then escapes it so that it can be used in XML.
		/// </summary>
		/// <param name="format">The string to escape.</param>
		/// <param name="args">The arguments to use in the <see cref="string.Format(string, object[])"/> call.</param>
		/// <returns>The escaped string, or null if <paramref name="format"/> is null.</returns>
		public static string EscapeXmlForDashboard(this string format, params object[] args)
		{
			if (null == format)
				return null;
			return args == null || args.Length == 0
				? System.Security.SecurityElement.Escape(format)
				: System.Security.SecurityElement.Escape(String.Format(format, args));
		}
	}
}
