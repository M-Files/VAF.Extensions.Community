using MFiles.VAF.Common;
using MFiles.VAF.Extensions.Webhooks.Authentication;
using System;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class WebhookAttribute
        : VaultAnonymousExtensionMethodAttribute
    {
		/// <summary>
		/// The web hook name.  Must be unique.
		/// </summary>
        public string Name => base.Filter;

		/// <summary>
		/// The HTTP method that is needed to call this web hook.
		/// TODO: This doesn't work.
		/// </summary>
        public string HttpMethod { get; private set; }

		/// <summary>
		/// If <see langword="true"/> then no authentication is needed to call this webhook.
		/// </summary>
        public bool SupportsNoAuthentication { get; private set; }

		/// <summary>
		/// The type of serializer to use.  The type must implement <see cref="Webhooks.ISerializer"/>.
		/// </summary>
        public Type SerializerType { get; set; }

		/// <summary>
		/// Creates a web hook.
		/// </summary>
		/// <param name="webhookName">The name of the webhook.  Also used as part of the URI (e.g. /webhook/{webhookName}).</param>
		/// <param name="supportsNoAuthentication">Whether this web hook requires authentication or not.</param>
		/// <param name="httpMethod">The HTTP method that should cause this web hook to run.</param>
        public WebhookAttribute(string webhookName, bool supportsNoAuthentication, string httpMethod = "GET")
            : base(webhookName)
        {
            this.HttpMethod = httpMethod;
            this.SupportsNoAuthentication = supportsNoAuthentication;
        }

		/// <summary>
		/// Checks whether authenticators of type <paramref name="type"/> are supported.
		/// e.g. <see cref="BasicAuthenticationWebhookAttribute"/> supports <see cref="Webhooks.Authentication.BasicWebhookAuthenticator"/>,
		/// but <see cref="AnonymousWebhookAttribute"/> supports any/all as no authentication is needed.
		/// </summary>
		/// <param name="type">The authenticator to check.</param>
		/// <returns><see langword="true"/> if supported, false otherwise.</returns>
        public virtual bool SupportsAuthenticator(Type type)
            => true;

        // Supports all authenticator types.
        public bool SupportsAuthenticator<TAuthenticatorType>()
            where TAuthenticatorType : IWebhookAuthenticator
            => this.SupportsAuthenticator(typeof(TAuthenticatorType));
    }
}
