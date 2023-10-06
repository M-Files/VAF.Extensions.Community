using MFiles.VAF.Extensions.Webhooks.Authentication;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MFiles.VAF.Extensions.Webhooks
{
	public interface IAsynchronousWebhook
		: IWebhook
	{
		string GetResponseText();
		string GetContentType();
		string TaskQueueId { get; set; }
		string TaskQueueTaskType { get; set; }
    }

	public interface IWebhook
	{
		bool Enabled { get; }
		string Name { get; }
		string HttpMethod { get; }
		bool SupportsNoAuthentication { get; }

		/// <summary>
		/// Gets the serializer to use for incoming requests.
		/// </summary>
		/// <returns></returns>
		ISerializer GetIncomingSerializer();

		/// <summary>
		/// Gets the serializer to use for outgoing responses.
		/// </summary>
		/// <returns></returns>
		ISerializer GetOutgoingSerializer();
	}
}
