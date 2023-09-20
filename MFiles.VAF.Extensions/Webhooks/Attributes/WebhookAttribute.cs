using MFiles.VAF.Common;
using MFiles.VAF.Extensions.Webhooks;
using System;
using System.Reflection;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class WebhookAttribute
        : VaultAnonymousExtensionMethodAttribute, IWebhook
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

		public bool Enabled => throw new NotImplementedException();

		/// <summary>
		/// Creates a web hook.
		/// </summary>
		/// <param name="webhookName">The name of the webhook.  Also used as part of the URI (e.g. /webhook/{webhookName}).</param>
		/// <param name="supportsNoAuthentication">Whether this web hook requires authentication or not.</param>
		/// <param name="httpMethod">The HTTP method that should cause this web hook to run.</param>
		public WebhookAttribute(string webhookName, bool supportsNoAuthentication = false, string httpMethod = "GET")
            : base(webhookName)
        {
            this.HttpMethod = httpMethod;
            this.SupportsNoAuthentication = supportsNoAuthentication;
        }

		public bool TryGetHandlerMethodInfo(out MethodInfo methodInfo, out object instance)
		{
			methodInfo = null;
			instance = null;
			return false;
		}
	}
}
