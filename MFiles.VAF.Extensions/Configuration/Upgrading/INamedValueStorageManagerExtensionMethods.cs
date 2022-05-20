using MFilesAPI;
using System;
using MFiles.VAF.Extensions.Configuration.Upgrading;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
	public static class INamedValueStorageManagerExtensionMethods
	{
		public static string GetValue(this INamedValueStorageManager namedValueStorageManager, Vault vault, MFNamedValueType namedValueType, string @namespace, string @name, string defaultValue = null)
		{
			if (null == namedValueStorageManager)
				throw new ArgumentNullException(nameof(namedValueStorageManager));
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (string.IsNullOrWhiteSpace(@namespace))
				throw new ArgumentException(nameof(@namespace));
			if (string.IsNullOrWhiteSpace(@name))
				throw new ArgumentException(nameof(@name));

			var namedValues = namedValueStorageManager.GetNamedValues(vault, namedValueType, @namespace);
			if (null == namedValues)
				return defaultValue;
			return namedValues.Contains(name) ? namedValues[name]?.ToString() : defaultValue;
		}
	}
}
