using MFiles.VAF.Extensions.Webhooks.Authentication;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Defines a web hook that is explicitly designed to be called with no authentication.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class AnonymousWebhookAuthenticationAttribute
        : WebhookAuthenticationAttribute
	{
		public AnonymousWebhookAuthenticationAttribute()
			: base(typeof(WebhookConfiguration))
		{
		}

		public override bool SupportsAuthenticator(Type type)
			=> true;

	}
}
