using MFiles.VAF.Extensions.Webhooks.Authentication;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Defines a webhook which is expected to be called using some pre-shared key
	/// in either a HTTP header or querystring value.
	/// </summary
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class SecretWebhookAuthenticationAttribute
		: WebhookAuthenticationAttribute
	{
		public SecretWebhookAuthenticationAttribute()
			: base(typeof(SecretWebhookAuthenticator))
		{
		}
		public override bool SupportsAuthenticator(Type type)
            => typeof(SecretWebhookAuthenticator).IsAssignableFrom(type);
    }
}
