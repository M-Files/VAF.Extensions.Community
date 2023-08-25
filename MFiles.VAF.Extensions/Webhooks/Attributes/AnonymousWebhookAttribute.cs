using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Defines a web hook that is explicitly designed to be called with no authentication.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class AnonymousWebhookAttribute
        : WebhookAttribute
    {
        public AnonymousWebhookAttribute(string webhookName, string httpMethod = "GET")
            : base(webhookName, true, httpMethod)
        {
        }
    }
}
