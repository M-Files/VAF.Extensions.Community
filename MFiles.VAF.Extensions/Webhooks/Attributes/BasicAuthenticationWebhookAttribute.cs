using MFiles.VAF.Extensions.Webhooks.Authentication;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Defines a web hook that expects to be called using Basic authentication
	/// (i.e. base64-encoded credentials in a HTTP header).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class BasicAuthenticationWebhookAttribute
        : WebhookAttribute
    {
        public BasicAuthenticationWebhookAttribute(string webhookName, string httpMethod = "GET")
            : base(webhookName, false, httpMethod)
        {
        }

        public override bool SupportsAuthenticator(Type type)
            => typeof(BasicWebhookAuthenticator).IsAssignableFrom(type);
    }
}
