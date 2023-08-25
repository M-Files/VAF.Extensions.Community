using MFiles.VAF.Common;
using System;

namespace MFiles.VAF.Extensions
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class WebhookAttribute
        : VaultAnonymousExtensionMethodAttribute
    {
        public string Name => base.Filter;
        public string HttpMethod { get; private set; }
        public bool SupportsNoAuthentication { get; private set; }
        public Type SerializerType { get; set; }

        public WebhookAttribute(string webhookName, bool supportsNoAuthentication, string httpMethod = "GET")
            : base(webhookName)
        {
            this.HttpMethod = httpMethod;
            this.SupportsNoAuthentication = supportsNoAuthentication;
        }

        public virtual bool SupportsAuthenticator(Type type)
            => true;

        // Supports all authenticator types.
        public bool SupportsAuthenticator<TAuthenticatorType>()
            where TAuthenticatorType : IWebhookAuthenticator
            => this.SupportsAuthenticator(typeof(TAuthenticatorType));
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class AnonymousWebhookAttribute
        : WebhookAttribute
    {
        public AnonymousWebhookAttribute(string webhookName, string httpMethod = "GET")
            : base(webhookName, true, httpMethod)
        {
        }
    }

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
