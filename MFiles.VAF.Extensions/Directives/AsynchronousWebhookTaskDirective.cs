using MFiles.VAF.Common;
using MFilesAPI;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	public class AsynchronousWebhookTaskDirective
		: TaskDirectiveWithDisplayName
	{
		[DataMember]
		/// <summary>
		/// Input HTTP query string for anonymous extension method.
		/// </summary>
		public string InputQueryString { get; set; }

		[DataMember]
		/// <summary>
		/// Input HTTP headers for anonymous extension method.
		/// </summary>
		public Dictionary<string, string> InputHttpHeaders { get; set; }
			= new Dictionary<string, string>();

		[DataMember]
		/// <summary>
		/// HTTP request body input bytes for anonymous extension method.
		/// </summary>
		public byte[] InputBytes { get; set; }

		[DataMember]
		/// <summary>
		/// Input HTTP method for anoymous extension method.
		/// </summary>
		public string InputHttpMethod { get; set; }

		public AsynchronousWebhookTaskDirective()
		{

		}
		public AsynchronousWebhookTaskDirective(EventHandlerEnvironment env)
		{
			this.InputQueryString = env?.InputQueryString;
			this.InputBytes = env?.InputBytes;
			this.InputHttpMethod = env?.InputHttpMethod;
			this.InputHttpHeaders = env?.InputHttpHeaders?
				.Names?
				.Cast<string>()?
				.ToDictionary
				(
					n => n,
					n => env?.InputHttpHeaders[n]?.ToString()
				);
		}

		public EventHandlerEnvironment AsEnvironment(Vault vault)
		{
			// Populate the headers.
			var namedValues = new NamedValues();
			if(null != this.InputHttpHeaders)
				foreach(var k in this.InputHttpHeaders.Keys)
					namedValues[k] = this.InputHttpHeaders[k];

			// Create the environment.
			return new EventHandlerEnvironment()
			{
				Vault = vault,
				InputQueryString = this.InputQueryString,
				InputBytes = this.InputBytes,
				InputHttpMethod = this.InputHttpMethod,
				InputHttpHeaders = namedValues
			};
		}
	}
}
