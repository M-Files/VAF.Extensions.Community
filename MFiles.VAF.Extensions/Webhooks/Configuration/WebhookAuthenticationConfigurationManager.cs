using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Webhooks.Authentication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions.Webhooks.Configuration
{
    public class WebhookAuthenticationConfigurationManager<TSecureConfiguration>
        where TSecureConfiguration : class, new()
    {
        private ILogger Logger { get; } = LogManager.GetLogger<WebhookAuthenticationConfigurationManager<TSecureConfiguration>>();
        public ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; }
        protected Dictionary<string, IWebhookAuthenticator> Authenticators { get; } 
            = new Dictionary<string, IWebhookAuthenticator>();
        protected IWebhookAuthenticator FallbackAuthenticator { get; set; }
            = new BlockAllRequestsWebhookAuthenticator();
        public WebhookAuthenticationConfigurationManager(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
        {
            this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
        }

		/// <summary>
		/// Ensures that this instance represents data in <paramref name="configuration"/>.
		/// </summary>
		/// <param name="configuration"></param>
        public virtual void PopulateFromConfiguration(TSecureConfiguration configuration)
        {
            this.Authenticators.Clear();

			// Sanity.
			if (null == this.VaultApplication?.Webhooks)
				return;

			// If we can parse the config then use it.
			if(configuration is IConfigurationWithWebhookConfiguration c
				&& null != c.WebhookConfiguration)
			{
				this.Logger?.Trace($"Parsing webhook configuration...");
				foreach(var webhook in this.VaultApplication.Webhooks)
				{
					if (string.IsNullOrWhiteSpace(webhook?.WebhookName))
						return;
					if (!c.WebhookConfiguration.ContainsKey(webhook.WebhookName))
					{
						this.Logger?.Warn($"Webhook with name {webhook.WebhookName} found, but configuration is not available.");
						continue;
					}

					if(c.WebhookConfiguration.TryGetWebhookAuthenticator(webhook.WebhookName, out IWebhookAuthenticator authenticator)
						&& null != authenticator)
					{
						this.Logger?.Info($"Webhook with name {webhook.WebhookName} found, configured via {authenticator.GetType().FullName}.");
						this.Authenticators.Add(webhook.WebhookName, authenticator);
					}
					else
					{
						this.Logger?.Warn($"Webhook with name {webhook.WebhookName} found, but configuration could not be loaded.");

					}
				}
			}
			else
			{
				this.Logger?.Trace($"The configuration does not inherit from IConfigurationWithWebhookConfiguration so cannot parse webhook config.");
			}
        }

		/// <summary>
		/// Returns the instance of <see cref="IWebhookAuthenticator"/> associated with the
		/// webhook with name <paramref name="webhook"/>.
		/// </summary>
		/// <param name="webhook">The webhook name.</param>
		/// <returns>The authenticator, or <see cref="FallbackAuthenticator"/> if none is registered.</returns>
        public IWebhookAuthenticator GetAuthenticator(string webhook)
            => this.Authenticators.ContainsKey(webhook) ? this.Authenticators[webhook] : this.FallbackAuthenticator;

    }
}
