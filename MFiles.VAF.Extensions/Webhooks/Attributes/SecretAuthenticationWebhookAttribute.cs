using MFiles.VAF.Extensions.Webhooks.Authentication;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Defines a webhook which is expected to be called using some pre-shared key
	/// in either a HTTP header or querystring value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SecretAuthenticationWebhookAttribute
        : WebhookAttribute
    {
        public SecretAuthenticationWebhookAttribute(string webhookName, string httpMethod = "GET")
            : base(webhookName, false, httpMethod)
        {
        }

        public override bool SupportsAuthenticator(Type type)
            => typeof(SecretWebhookAuthenticator).IsAssignableFrom(type);
    }
}
