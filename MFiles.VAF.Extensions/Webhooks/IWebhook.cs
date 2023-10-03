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
        /// The type of serializer to use.  The type must implement <see cref="Webhooks.ISerializer"/>.
        /// </summary>
        /// <remarks>If null, consumer is expected to default to <see cref="NewtonsoftJsonSerializer"/>.</remarks>
        Type DefaultSerializerType { get; set; }

        /// <summary>
        /// The type of serializer to use for incoming requests.  The type must implement <see cref="Webhooks.ISerializer"/>.
        /// </summary>
        /// <remarks> If null, uses <see cref="DefaultSerializerType"/></remarks>
        Type IncomingSerializerType { get; set; }

        /// <summary>
        /// The type of serializer to use for outgoing responses.  The type must implement <see cref="Webhooks.ISerializer"/>.
        /// </summary>
        /// <remarks> If null, uses <see cref="DefaultSerializerType"/></remarks>
        Type OutgoingSerializerType { get; set; }
    }
}
