using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace MFiles.VAF.Extensions.Webhooks
{
	// Not yet tested.
	internal class MultiPartFormDataSerializer
		: ISerializer
	{
		public class MultipartFormData
		{
			public List<FileData> Files { get; set; } = new List<FileData>();
		}
		public class FileData
		{
			public string Name { get; set; }
			public Stream Contents { get; set; }
		}
		public bool CanDeserialize(Type type)
			=> typeof(MultipartFormData).IsAssignableFrom(type);

		public bool CanSerialize(Type type) => false;

		public object Deserialize(byte[] input, Type t)
		{
			var data = new MultipartFormData();
			using (var memoryStream = new MemoryStream())
			{
				memoryStream.Write(input, 0, input.Length);
				memoryStream.Position = 0;

				var streamContent = new StreamContent(memoryStream);
				var provider = streamContent.ReadAsMultipartAsync().Result;
				foreach(var httpContent in provider.Contents)
				{
					data.Files.Add
					(
						new FileData()
						{
							Name = httpContent.Headers.ContentDisposition.FileName,
							Contents = httpContent.ReadAsStreamAsync().Result
						}
					);
				}
			}
			return data;
		}

		public T Deserialize<T>(byte[] input)
			=> (T)this.Deserialize(input, typeof(T));

		public byte[] Serialize(object input)
		{
			throw new NotImplementedException();
		}

		public byte[] Serialize<T>(T input)
		{
			throw new NotImplementedException();
		}
	}
}
