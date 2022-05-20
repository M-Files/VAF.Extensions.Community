using System;

namespace MFiles.VAF.Extensions
{
	public interface IJsonConvert
	{
		T Deserialize<T>(string input);
		object Deserialize(string input, Type type);
		string Serialize<T>(T input);
	}

	internal class NewtonsoftJsonConvert : IJsonConvert
	{
		/// <summary>
		/// The default serialiser settings to use.
		/// </summary>
		public static Newtonsoft.Json.JsonSerializerSettings DefaultJsonSerializerSettings { get; }
			= new Newtonsoft.Json.JsonSerializerSettings()
			{
				DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
				Formatting = Newtonsoft.Json.Formatting.Indented
			};
		public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; }
			= DefaultJsonSerializerSettings;

		public T Deserialize<T>(string input)
			=> Deserialize<T>(input, null);

		public object Deserialize(string input, Type type)
			=> Deserialize(input, type, null);

		public object Deserialize(string input, Type type, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject(input, type, settings ?? this.JsonSerializerSettings ?? DefaultJsonSerializerSettings);

		public T Deserialize<T>(string input, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input, settings ?? this.JsonSerializerSettings ?? DefaultJsonSerializerSettings);

		public string Serialize<T>(T input)
			=> Serialize<T>(input, null);

		public string Serialize<T>(T input, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.SerializeObject(input, settings ?? this.JsonSerializerSettings ?? DefaultJsonSerializerSettings);
	}
}
