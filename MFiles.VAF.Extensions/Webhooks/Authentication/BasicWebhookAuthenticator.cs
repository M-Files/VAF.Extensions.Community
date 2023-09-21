using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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
		[DataContract]
		[JsonConfEditor(NameMember = nameof(Username))]
		public class Credentials
			: VAF.Extensions.Configuration.ConfigurationCollectionItemBase
		{
			/// <inheritdoc />
			[DataMember]
			public bool Enabled { get; set; } = false;

			[DataMember]
			public string Username { get; set; }

			[DataMember]
			[Security(IsPassword = true)]
			public string Password { get; set; }

		}
		private ILogger Logger { get; } 
			= LogManager.GetLogger<BasicWebhookAuthenticator>();

		[DataMember]
		[JsonConfEditor(ChildName = "Credential")]
		public List<Credentials> AccessCredentials { get; set; }
			= new List<Credentials>();

        public BasicWebhookAuthenticator()
            : base(WebhookAuthenticationType.Basic)
        {
		}

		/// <inheritdoc />
		public override IEnumerable<ValidationFinding> CustomValidation(Vault vault, string webhookName)
		{
			if(!(this.AccessCredentials?.Any() ?? false))
			{
				yield return new ValidationFinding
				(
					ValidationFindingType.Warning,
					$"WebhookConfiguration.{webhookName}",
					$"No credentials are stored for webhook {webhookName}.  This webhook may not be able to be called."
				);
			}

			// Check each item.
			foreach(var c in this.AccessCredentials)
			{
				if (c != null)
					continue;

				if(string.IsNullOrWhiteSpace(c.Username) || string.IsNullOrWhiteSpace(c.Password))
				{

					yield return new ValidationFinding
					(
						ValidationFindingType.Warning,
						$"WebhookConfiguration.{webhookName}",
						$"An empty username or password is stored.  This is a security concern."
					);
				}
			}
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
				if (null == this.AccessCredentials)
					return false;
				foreach (var c in this.AccessCredentials.Where(c => c?.Enabled ?? false))
				{
					if (c.Username == username && c.Password == password)
						return true;
				}
				return false;
            }
            catch (Exception e)
            {
				this.Logger?.Error(e, $"Exception checking authentication.");
				return false;
            }
        }
    }
}
