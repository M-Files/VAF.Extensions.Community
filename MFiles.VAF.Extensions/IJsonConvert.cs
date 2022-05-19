using System;

namespace MFiles.VAF.Extensions
{
	public interface IJsonConvert
	{
		T Deserialize<T>(string input);
		object Deserialize(string input);
		string Serialize<T>(T input);
	}

	internal class NewtonsoftJsonConvert : IJsonConvert
	{
		/// <summary>
		/// The default serialiser settings to use.
		/// </summary>
		public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; }
			= new Newtonsoft.Json.JsonSerializerSettings()
			{
				DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
				Formatting = Newtonsoft.Json.Formatting.Indented
			};

		public T Deserialize<T>(string input)
			=> Deserialize<T>(input, null);

		public object Deserialize(string input)
			=> Deserialize<object>(input, null);

		public T Deserialize<T>(string input, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input, settings ?? this.JsonSerializerSettings ?? new Newtonsoft.Json.JsonSerializerSettings());

		public string Serialize<T>(T input)
			=> Serialize<T>(input, null);

		public string Serialize<T>(T input, Newtonsoft.Json.JsonSerializerSettings settings)
			=> Newtonsoft.Json.JsonConvert.SerializeObject(input, settings ?? this.JsonSerializerSettings ?? new Newtonsoft.Json.JsonSerializerSettings());
	}
}
