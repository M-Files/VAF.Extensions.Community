using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
	public interface INamedValueStorageManager
	{
		NamedValues GetNamedValues(Vault vault, MFNamedValueType namedValueType, string @namespace);
		void SetNamedValues(Vault vault, MFNamedValueType namedValueType, string @namespace, NamedValues namedValues);
		void RemoveNamedValues(Vault vault, MFNamedValueType namedValueType, string @namespace, params string[] namedValues);
	}
	internal class VaultNamedValueStorageManager
		: INamedValueStorageManager
	{
		public NamedValues GetNamedValues(Vault vault, MFNamedValueType namedValueType, string @namespace)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == vault.NamedValueStorageOperations)
				throw new ArgumentException("The NamedValueStorage instance was null.", nameof(vault));

			// Use the API implementation.
			return vault
				.NamedValueStorageOperations
				.GetNamedValues(namedValueType, @namespace);
		}
		public void SetNamedValues(Vault vault, MFNamedValueType namedValueType, string @namespace, NamedValues namedValues)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == vault.NamedValueStorageOperations)
				throw new ArgumentException("The NamedValueStorage instance was null.", nameof(vault));

			// Use the API implementation.
			vault
				.NamedValueStorageOperations
				.SetNamedValues(namedValueType, @namespace, namedValues);
		}
		public void RemoveNamedValues(Vault vault, MFNamedValueType namedValueType, string @namespace, params string[] namedValueNames)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == vault.NamedValueStorageOperations)
				throw new ArgumentException("The NamedValueStorage instance was null.", nameof(vault));
			if (null == namedValueNames || 0 == namedValueNames.Length)
				return;

			// Create the strings collection.
			var strings = new Strings();
			foreach (var name in namedValueNames)
				strings.Add(0, name);

			// Use the API implementation.
			vault
				.NamedValueStorageOperations
				.RemoveNamedValues(namedValueType, @namespace, strings);
		}
	}
}
