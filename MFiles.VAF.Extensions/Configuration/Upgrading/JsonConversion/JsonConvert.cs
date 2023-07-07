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
		/// If these types are found then their default values are left intact
		/// when converting the JSON.
		/// </summary>
		public static List<string> DefaultValueSkippedTypes { get; } = new List<string>()
		{
			"MFiles.VAF.Configuration.JsonAdaptor.SearchConditionsJA"
		};
	}
}
