using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
	/// <summary>
	/// Implements basic (base64-encoded credentials in a HTTP header)
	/// authentication.  Also exposes appropriate configuration values.
	/// </summary>
    [DataContract]
    public class BasicWebhookAuthenticator
        : WebhookAuthenticatorBase
    {

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        [Security(IsPassword = true)]
        public string Password { get; set; }

        [DataMember]
        public string NewUsername { get; set; }

        [DataMember]
        [Security(IsPassword = true)]
        public string NewPassword { get; set; }

        public BasicWebhookAuthenticator()
            : base(WebhookAuthenticationType.Basic)
        {
        }

		/// <inheritdoc />
        protected override bool ContainsCredentials(EventHandlerEnvironment env)
            => !string.IsNullOrWhiteSpace(env?.InputHttpHeaders?.GetValueOrEmpty("Authorization"));

		/// <inheritdoc />
		/// <remarks>Adds the WWW-Authenticate header.</remarks>
		protected override AnonymousExtensionMethodResult CreateResult
        (
            HttpStatusCode statusCode,
            NamedValues headers,
            byte[] content
        )
        {
            // If it's unauthorised then add a challenge header.
            if(statusCode == HttpStatusCode.Unauthorized)
            {
                headers = headers ?? new NamedValues();
                headers["WWW-Authenticate"] = "Basic";
            }

            // Use the base implementation.
            return base.CreateResult(statusCode, headers, content);
        }

		/// <inheritdoc />
		protected override bool AreCredentialsValid(EventHandlerEnvironment env)
        {
            // Sanity.
            if (null == env)
                throw new ArgumentNullException(nameof(env));
            var httpHeaders = env.InputHttpHeaders;

            try
            {
                // Get the header and decode the values.
                var authorizationHeaderValue = httpHeaders.GetValueOrEmpty("Authorization");
                if (string.IsNullOrEmpty(authorizationHeaderValue))
                    return false; // No value.)
                if (authorizationHeaderValue.StartsWith("Basic "))
                    authorizationHeaderValue = authorizationHeaderValue.Substring(6);

                string username = "";
                string password = "";
                {
                    var pair = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeaderValue));
                    if (string.IsNullOrEmpty(pair))
                        return false; // No value.

                    if (!pair.Contains(":"))
                        username = pair;
                    else
                    {
                        username = pair.Substring(0, pair.IndexOf(":"));
                        password = pair.Substring(pair.IndexOf(":") + 1);
                    }
                }

                // Check that they are the same.
                return (this.Username == username && this.Password == password)
                    || (this.NewUsername == username && this.NewPassword == password);
            }
            catch (Exception e)
            {
                // TODO: LOG!
                return false;
            }
        }
    }
}
