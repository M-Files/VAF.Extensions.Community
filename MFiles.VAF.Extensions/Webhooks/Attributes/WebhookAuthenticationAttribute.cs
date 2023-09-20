using MFiles.VAF.Extensions.Webhooks.Authentication;
using System;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public abstract class WebhookAuthenticationAttribute
		: Attribute
	{
		public Type ConfigurationType { get; protected set; }

		protected WebhookAuthenticationAttribute(Type configurationType)
		{
			this.ConfigurationType = configurationType 
				?? throw new ArgumentNullException(nameof(configurationType));
		}

		/// <summary>
		/// Checks whether authenticators of type <paramref name="type"/> are supported.
		/// e.g. <see cref="BasicAuthenticationWebhookAttribute"/> supports <see cref="Webhooks.Authentication.BasicWebhookAuthenticator"/>,
		/// but <see cref="AnonymousWebhookAttribute"/> supports any/all as no authentication is needed.
		/// </summary>
		/// <param name="type">The authenticator to check.</param>
		/// <returns><see langword="true"/> if supported, false otherwise.</returns>
		public virtual bool SupportsAuthenticator(Type type)
			=> false;

		// Supports all authenticator types.
		public bool SupportsAuthenticator<TAuthenticatorType>()
			where TAuthenticatorType : IWebhookAuthenticator
			=> this.SupportsAuthenticator(typeof(TAuthenticatorType));

	}
}
