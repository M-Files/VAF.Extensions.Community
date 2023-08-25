using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;

namespace MFiles.VAF.Extensions.Webhooks
{
    public class NewtonsoftJsonSerializer
        : ISerializer
    {

        public Encoding Encoding { get; set; }
            = Encoding.UTF8;
        public Formatting Formatting { get; set; }
            = Formatting.Indented;
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
            = new JsonSerializerSettings();

        public T Deserialize<T>(byte[] input)
            => (T)this.Deserialize(input, typeof(T));

        public byte[] Serialize<T>(T input)
            => this.Serialize(input as object);

        public virtual object Deserialize(byte[] input, Type t)
            => input == null
            ? t.IsValueType ? Activator.CreateInstance(t) : null
            : JsonConvert.DeserializeObject(this.Encoding.GetString(input), t, this.JsonSerializerSettings);

        public virtual byte[] Serialize(object input)
            => this.Encoding.GetBytes
            (
                input == default
                ? typeof(IEnumerable).IsAssignableFrom(input.GetType()) ? "[]" : "{}"
                : JsonConvert.SerializeObject(input, this.Formatting, this.JsonSerializerSettings)
            );
    }
}
