using MFiles.VAF.Common;
using System;

namespace MFiles.VAF.Extensions.ExtensionMethods
{
	public static class EnvironmentBaseExtensionMethods
	{
		/// <summary>
		/// Returns true if <see cref="EnvironmentBase.CurrentUserID"/>
		/// is set to a system process (i.e. an ID less than or equal to zero).
		/// </summary>
		/// <param name="env">The environment to check.</param>
		/// <returns>true if <see cref="EnvironmentBase.CurrentUserID"/> is less than or equal to zero</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="env"/> is null.</exception>
		public static bool CurrentUserIsSystemProcess(this EnvironmentBase env)
			// IDs under zero are system processes.
			=> (env ?? throw new ArgumentNullException(nameof(env))).CurrentUserID  <= 0;
	}
}
