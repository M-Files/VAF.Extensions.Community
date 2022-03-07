using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.ExtensionMethods
{
	public static class DictionaryExtensionMethods
	{
		/// <summary>
		/// Adds or updates an item in a dictionary.
		/// </summary>
		/// <typeparam name="TKey">The type of the key in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the value in the dictionary.</typeparam>
		/// <param name="dictionary">The dictionary to update.</param>
		/// <param name="key">The key of the item.</param>
		/// <param name="value">The value for the item.</param>
		public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			// Sanity.
			if (null == dictionary)
				throw new ArgumentNullException(nameof(dictionary));
			if (null == key)
				throw new ArgumentNullException(nameof(key));
			if (dictionary.ContainsKey(key))
				dictionary[key] = value;
			else
				dictionary.Add(key, value);
		}
	}
}
