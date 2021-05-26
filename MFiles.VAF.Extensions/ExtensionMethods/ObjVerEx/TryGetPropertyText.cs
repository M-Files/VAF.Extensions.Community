using System;

using MFiles.VAF.Common;
using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// <para>
	/// Extension method for the <see cref="ObjVerEx"/> object for providing
	/// a safety <see cref="ObjVerEx.GetPropertyText(MFIdentifier)"/> method
	/// </para>
	/// <para>
	/// This can be used to get the property text as an <see cref="string.Empty"/>  -
	/// or <see langword="null"/> if the <see cref="MFIdentifier"/> was not set correctly in configuration
	/// or if it could not be resolved via <see cref="MFIdentifier.IsResolved"/>.
	/// </para>
	/// <para>
	/// The check if the property was set correctly can be done as comfort function via return value
	/// instead of catching exceptions or implementing own checks before execution.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Contributed by Falk Huth, EWERK Group, Leipzig (Germany)<br/>
	/// <a href="https://linkedin.com/in/falk-huth" target="_new">https://linkedin.com/in/falk-huth</a> | 
	/// <a href="https://github.com/falk-huth-ewerk" target="_new">https://github.com/falk-huth-ewerk</a> | 
	/// <a href="https://ewerk.com/innovation-hub/ecm" target="_new">https://ewerk.com/innovation-hub/ecm</a> (German)
	/// </remarks>
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Enhanced method for <see cref="ObjVerEx.GetPropertyText(MFIdentifier)"/>
		/// to return a <see langword="null"/> value even if the property was not set or could not be resolved,
		/// otherwise the value which was set or <see cref="string.Empty"/>.
		/// </summary>
		/// 
		/// <param name="objVerEx">
		/// The <see cref="ObjVerEx"/> object to be used as base for calling <see cref="ObjVerEx.GetPropertyText(MFIdentifier)"/>.
		/// </param>
		/// <param name="result">
		/// The <see cref="string"/> value to be returned as output parameter.
		/// </param>
		/// <param name="prop">
		/// The <see cref="MFIdentifier"/> object for the property which can be <see langword="null"/> or not resolved.
		/// </param>
		/// 
		/// <returns>
		/// <list type="table">
		/// <item>
		/// <term><see cref="true"/></term>
		/// <description>if <paramref name="prop"/> is not <see langword="null"/> and can be resolved</description>
		/// </item>
		/// <item>
		/// <term><see cref="false"/></term>
		/// <description>if <paramref name="prop"/> is <see langword="null"/> or cannot be resolved</description>
		/// </item>
		/// </list>
		/// </returns>
		public static bool TryGetPropertyText(
			this ObjVerEx objVerEx,
			out string result,
			MFIdentifier prop)
		{
			// Sanity
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));

			// return the nullValue specified if not set or not resolved as result and false as return value
			if (null == prop || !prop.IsResolved)
			{
				result = null;
				return false;
			}

			// return value and return that all is ok
			result = objVerEx.GetPropertyText(prop);
			return true;
		}
	}
}
