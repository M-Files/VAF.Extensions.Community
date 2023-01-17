using MFiles.VAF.Configuration.JsonAdaptor;
using MFiles.VAF.Configuration.JsonEditor;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using System;
using System.CodeDom;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MFiles.VAF.Extensions
{
	public interface IJsonConvert
	{
		/// <summary>
		/// Deserializes <paramref name="input"/> into an instance of <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialize to.</typeparam>
		/// <param name="input">The serialized version.</param>
		/// <returns>The instance.</returns>
		T Deserialize<T>(string input);

		/// <summary>
		/// Deserializes <paramref name="input"/> to an instance of <paramref name="type"/>.
		/// </summary>
		/// <param name="input">The serialized version.</param>
		/// <param name="type">The type to deserialize to.</param>
		/// <returns>The instance.</returns>
		object Deserialize(string input, Type type);

		/// <summary>
		/// Serializes <paramref name="input"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialize from.</typeparam>
		/// <param name="input">The object to deserialize.</param>
		/// <returns>The instance.</returns>
		string Serialize<T>(T input);

		/// <summary>
		/// Serializes <paramref name="input"/>.
		/// </summary>
		/// <param name="t">The type to serialize from.</typeparam>
		/// <param name="input">The object to deserialize.</param>
		/// <returns>The instance.</returns>
		string Serialize(object input, Type t);
	}
}
