using MFiles.VAF.Extensions.Webhooks;
using System;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class AsynchronousWebhookAttribute
		: WebhookAttribute, IAsynchronousWebhook
	{
		public const string DefaultResponseText = "{ \"status\": \"queued\" }";
		public const string DefaultContentType = "application/json";

		protected string ResponseText { get; set; }
		protected string ContentType { get; set; }
		public string TaskQueueId { get; set; }
		public string TaskQueueTaskType { get; set; }

		public virtual string GetResponseText() => this.ResponseText;
		public virtual string GetContentType() => this.ContentType;

		public AsynchronousWebhookAttribute
		(
			string webhookName,
			bool supportsNoAuthentication = false,
			string httpMethod = "GET"
		)
			: this
			(
				webhookName, 
				DefaultResponseText,
				DefaultContentType, 
				supportsNoAuthentication, 
				httpMethod
			)
		{
		}
		public AsynchronousWebhookAttribute
		(
			string webhookName,
			string responseText,
			string contentType,
			bool supportsNoAuthentication = false,
			string httpMethod = "GET"
		)
			: base(webhookName, supportsNoAuthentication, httpMethod)
		{
			this.ResponseText = responseText;
			this.ContentType = contentType;
		}
	}
}
