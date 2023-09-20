using MFiles.VAF.Extensions.Webhooks.Authentication;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Defines a web hook that expects to be called using Basic authentication
	/// (i.e. base64-encoded credentials in a HTTP header).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class BasicWebhookAuthenticationAttribute
        : WebhookAuthenticationAttribute
	{
		public BasicWebhookAuthenticationAttribute()
			: base(typeof(BasicWebhookAuthenticator))
		{
		}
		
		public override bool SupportsAuthenticator(Type type)
            => typeof(BasicWebhookAuthenticator).IsAssignableFrom(type);
    }
}
