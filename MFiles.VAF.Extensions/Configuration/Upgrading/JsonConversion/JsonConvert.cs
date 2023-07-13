using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
{
	public abstract class JsonConvert
		: IJsonConvert
	{
		/// <inheritdoc />
		public abstract T Deserialize<T>(string input);

		/// <inheritdoc />
		public abstract object Deserialize(string input, Type type);

		/// <inheritdoc />
		public abstract string Serialize<T>(T input);

		/// <inheritdoc />
		public abstract string Serialize(object input, Type t);

		/// <summary>
		/// If these types are found then their default values are output when
		/// serializing instances.
		/// </summary>
		/// <remarks></remarks>
		public static List<string> DefaultValueSkippedTypes { get; } = new List<string>();

		/// <summary>
		/// If these types are found in the JSON then the raw JSON will be maintained.
		/// Useful for types that are not expected to go through .NET serialization
		/// such as <see cref="VAF.Configuration.JsonAdaptor.SearchConditionsJA"/>.
		/// </summary>
		public static List<string> LeaveJsonAloneTypes { get; } = new List<string>()
		{
			"MFiles.VAF.Configuration.JsonAdaptor.SearchConditionsJA"
		};
	}
}
