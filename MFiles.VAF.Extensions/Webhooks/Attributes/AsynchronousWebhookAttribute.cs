using System;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class AsynchronousWebhookAttribute
		: WebhookAttribute
	{
		public AsynchronousWebhookAttribute(string webhookName, bool supportsNoAuthentication = false, string httpMethod = "GET") 
			: base(webhookName, supportsNoAuthentication, httpMethod)
		{
		}
	}
}
