using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Webhooks.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Webhooks.Configuration
{

    [DataContract]
    public class WebhookConfiguration
        : IWebhookAuthenticatorProvider
    {
        [DataMember]
        public bool Enabled { get; set; } = false;

        [DataMember]
        [JsonConfEditor
        (
            Label = "Authentication Type",
            DefaultValue = WebhookAuthenticationType.None,
            Hidden = true,
            ShowWhen = ".parent._children{.key == '" + nameof(Enabled) + "' && .value == true }"
        )]
        public WebhookAuthenticationType AuthenticationType { get; set; } = WebhookAuthenticationType.None;

        [DataMember]
        [JsonConfEditor
        (
            Label = "Basic Authentication Configuration",
            Hidden = true,
            ShowWhen = ".parent._children{.key == '" + nameof(Enabled) + "' && .value == true }.parent._children{.key == '" + nameof(AuthenticationType) + "' && .value == 'Basic' }"
        )]
        public BasicWebhookAuthenticator BasicWebhookAuthenticationConfiguration { get; set; }
            = new BasicWebhookAuthenticator();

        [DataMember]
        [JsonConfEditor
        (
            Label = "Secret Authentication Configuration",
            Hidden = true,
            ShowWhen = ".parent._children{.key == '" + nameof(Enabled) + "' && .value == true }.parent._children{.key == '" + nameof(AuthenticationType) + "' && .value == 'Secret' }"
        )]
        public SecretWebhookAuthenticator SecretWebhookAuthenticationConfiguration { get; set; }
            = new SecretWebhookAuthenticator();

		/// <inheritdoc />
		/// <remarks>
		/// Returns an appropriate authenticator depending upon the configured <see cref="AuthenticationType"/>.
		/// </remarks>
        public virtual IWebhookAuthenticator GetWebhookAuthenticator()
        {
            switch (this.AuthenticationType)
            {
                case WebhookAuthenticationType.None:
                    return new BlockAllRequestsWebhookAuthenticator();
                case WebhookAuthenticationType.Basic:
                    return this.BasicWebhookAuthenticationConfiguration?.GetWebhookAuthenticator();
                case WebhookAuthenticationType.Secret:
                    return this.SecretWebhookAuthenticationConfiguration?.GetWebhookAuthenticator();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
