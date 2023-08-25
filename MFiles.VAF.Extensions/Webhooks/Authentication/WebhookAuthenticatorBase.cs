using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
    [DataContract]
    public abstract class WebhookAuthenticatorBase
        : IWebhookAuthenticator, IWebhookAuthenticatorProvider
    {
		/// <inheritdoc />
        [DataMember]
        public bool Enabled { get; set; } = false;

        private ILogger Logger { get; } = LogManager.GetLogger(typeof(WebhookAuthenticatorBase));

		/// <summary>
		/// The type of authentication this authenticator provides.
		/// </summary>
        public WebhookAuthenticationType AuthenticationType { get; }

        public WebhookAuthenticatorBase(WebhookAuthenticationType authenticationType)
        {
            this.AuthenticationType = authenticationType;
        }

		/// <inheritdoc />
		public virtual IWebhookAuthenticator GetWebhookAuthenticator()
            => this;

		/// <summary>
		/// Returns true if the request is considered authenticated 
		/// (i.e. the request passes both <see cref="ContainsCredentials(EventHandlerEnvironment)"/>
		/// and <see cref="AreCredentialsValid(EventHandlerEnvironment)"/>).
		/// </summary>
		/// <param name="env">The environment representing the request.</param>
		/// <param name="output">The output from the authentication, if provided.</param>
		/// <returns><see langword="true"/> if the request is considered authenticated.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual bool IsRequestAuthenticated(EventHandlerEnvironment env, out AnonymousExtensionMethodResult output)
        {
            output = null;
            if (null == env)
                throw new ArgumentNullException(nameof(env));

            // If we don't have auth details then it's unauthorised.
            if (!this.ContainsCredentials(env))
            {
                this.Logger.Debug($"Web hook {env.VaultExtensionMethodName} requires authentication but the request does not contain credentials.  Request is being denied.");
                output = this.CreateResult(System.Net.HttpStatusCode.Unauthorized, env);
                return false;
            }
            else
            {
                // If the credentials are not valid then it's forbidden.
                if (!this.AreCredentialsValid(env))
                {
                    this.Logger.Debug($"Web hook {env.VaultExtensionMethodName} requires authentication but the credentials provided were not valid.  Request is being denied.");
                    output = this.CreateResult(System.Net.HttpStatusCode.Forbidden, env);
                    return false;
                }
            }

            return true;
        }

		/// <summary>
		/// Checks whether the credentials in <paramref name="env"/> are valid.
		/// </summary>
		/// <param name="env">The environment representing the request.</param>
		/// <returns></returns>
		protected abstract bool AreCredentialsValid(EventHandlerEnvironment env);

		/// <summary>
		/// Checks whether the credentials in <paramref name="env"/> are valid.
		/// </summary>
		/// <param name="env">The environment representing the request.</param>
		/// <returns></returns>
		protected abstract bool ContainsCredentials(EventHandlerEnvironment env);

        protected virtual AnonymousExtensionMethodResult CreateResult
        (
            HttpStatusCode statusCode,
            EventHandlerEnvironment env
        )
            => this.CreateResult(HttpStatusCode.Unauthorized, new NamedValues());

        protected virtual AnonymousExtensionMethodResult CreateForbiddenResult(EventHandlerEnvironment env)
            => this.CreateResult(HttpStatusCode.Forbidden, new NamedValues());

        protected virtual AnonymousExtensionMethodResult CreateResult
        (
            HttpStatusCode statusCode, 
            NamedValues headers
        )
            => this.CreateResult(statusCode, headers, (byte[])null);

        protected virtual AnonymousExtensionMethodResult CreateResult
        (
            HttpStatusCode statusCode,
            NamedValues headers,
            Encoding encoding,
            string content
        )
            => this.CreateResult
            (
                statusCode,
                headers, 
                (encoding ?? throw new ArgumentNullException(nameof(encoding))).GetBytes(content)
            );

        protected virtual AnonymousExtensionMethodResult CreateResult
        (
            HttpStatusCode statusCode,
            NamedValues headers,
            System.IO.Stream content
        )
        {
            byte[] data;
            if (content is System.IO.MemoryStream ms)
                data = ms.ToArray();
            else
            {
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    content?.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
            }
            return this.CreateResult(statusCode, headers, data);
        }

        protected virtual AnonymousExtensionMethodResult CreateResult
        (
            HttpStatusCode statusCode,
            NamedValues headers,
            byte[] content
        )
            // TODO: Can we set the HTTP status?!
            // I assume currently exceptions return 500 and everything else 200?
            // We need to be able to set the status code to correctly handle authentication issues.
            => new AnonymousExtensionMethodResult()
            {
                OutputHttpHeadersValue = headers,
                OutputBytesValue = content
            };

    }
}
