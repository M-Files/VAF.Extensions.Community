using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;

namespace MFiles.VAF.Extensions.Webhooks
{

	public class NewtonsoftJsonSerializer
        : ISerializer
	{
		/// <inheritdoc />
		public bool CanSerialize(Type type) => true;

		/// <inheritdoc />
		public bool CanDeserialize(Type type) => true;        
		
		/// <summary>
		/// The type of encoding to use.  Defaults to <see cref="Encoding.UTF8"/>.
		/// </summary>
		public Encoding Encoding { get; set; }
            = Encoding.UTF8;

		/// <summary>
		/// How to format the resulting JSON.  Defaults to <see cref="Formatting.Indented"/>.
		/// </summary>
		public Formatting Formatting { get; set; }
            = Formatting.Indented;

		/// <summary>
		/// The settings to use for the serialization.
		/// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
            = new JsonSerializerSettings();

		/// <inheritdoc />
        public T Deserialize<T>(byte[] input)
            => (T)this.Deserialize(input, typeof(T));

		/// <inheritdoc />
		public byte[] Serialize<T>(T input)
            => this.Serialize(input as object);

		/// <inheritdoc />
		public virtual object Deserialize(byte[] input, Type t)
            => input == null
            ? t.IsValueType ? Activator.CreateInstance(t) : null
            : Newtonsoft.Json.JsonConvert.DeserializeObject(this.Encoding.GetString(input), t, this.JsonSerializerSettings);

		/// <inheritdoc />
		public virtual byte[] Serialize(object input)
            => this.Encoding.GetBytes
            (
                input == default
                ? typeof(IEnumerable).IsAssignableFrom(input.GetType()) ? "[]" : "{}"
                : Newtonsoft.Json.JsonConvert.SerializeObject(input, this.Formatting, this.JsonSerializerSettings)
            );
    }
}
