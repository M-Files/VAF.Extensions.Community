using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
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
        public MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; }
        protected Dictionary<string, IWebhookAuthenticator> Authenticators { get; } 
            = new Dictionary<string, IWebhookAuthenticator>();
        protected IWebhookAuthenticator FallbackAuthenticator { get; set; }
            = new NoAuthenticationWebhookAuthenticator();
        public WebhookAuthenticationConfigurationManager(MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
        {
            this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
        }
        public virtual void PopulateFromConfiguration(TSecureConfiguration configuration)
        {
            this.Authenticators.Clear();

            // Attempt to find any web hook configuration.
            var config = new List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>>();
            this.GetWebhookAuthenticationConfiguration(configuration, out config);
            foreach(var pair in config)
            {
                // Sanity.
                if (string.IsNullOrWhiteSpace(pair.Item1.WebHookName))
                    continue;
                var authenticator = pair.Item2?.GetWebhookAuthenticator();
                if (null == authenticator)
                    continue;

                // We're good.
                this.Logger.Debug($"Enabling webhook {pair.Item1.WebHookName} with authentication type {authenticator.GetType().Name}");
                this.Authenticators.Add(pair.Item1.WebHookName, authenticator);
            }
        }
        public IWebhookAuthenticator GetAuthenticator(string webhook)
            => this.Authenticators.ContainsKey(webhook) ? this.Authenticators[webhook] : this.FallbackAuthenticator;


        /// <summary>
        /// Retrieves any task processor scheduling/recurring configuration
        /// that is exposed via VAF configuration.
        /// </summary>
        /// <param name="input">The object containing the <paramref name="fieldInfo"/>.</param>
        /// <param name="fieldInfo">The field to retrieve the configuration from.</param>
        /// <param name="config">All configuration found relating to scheduled execution.</param>
        /// <param name="recurse">Whether to recurse down the object structure exposed.</param>
        protected virtual void GetWebhookAuthenticationConfiguration
        (
            object input,
            FieldInfo fieldInfo,
            out List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>> config,
            bool recurse = true
        )
        {
            config = new List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>>();
            if (null == input || null == fieldInfo)
                return;

            // Get the basic value.
            var value = fieldInfo.GetValue(input);
            if (null == value)
                return;

            // If it is enumerable then iterate over the contents and add.
            if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
            {
                foreach (var item in (IEnumerable)value)
                {
                    {
                        this.GetWebhookAuthenticationConfiguration(item, out List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>> a);
                        config.AddRange(a);
                    }
                }
                return;
            }

            // Otherwise just add.
            this.GetWebhookAuthenticationConfiguration(value, out config);

        }

        /// <summary>
        /// Retrieves any task processor scheduling/recurring configuration
        /// that is exposed via VAF configuration.
        /// </summary>
        /// <param name="input">The object containing the <paramref name="propertyInfo"/>.</param>
        /// <param name="propertyInfo">The property to retrieve the configuration from.</param>
        /// <param name="schedules">All configuration found relating to scheduled execution.</param>
        /// <param name="recurse">Whether to recurse down the object structure exposed.</param>
        protected virtual void GetWebhookAuthenticationConfiguration
        (
            object input,
            PropertyInfo propertyInfo,
            out List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>> config,
            bool recurse = true
        )
        {
            config = new List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>>();
            if (null == input || null == propertyInfo)
                return;

            // Get the basic value.
            var value = propertyInfo.GetValue(input);
            if (value == null)
                return;

            // If it is enumerable then iterate over the contents and add.
            if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
            {
                foreach (var item in (IEnumerable)value)
                {
                    {
                        this.GetWebhookAuthenticationConfiguration(item, out List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>> a);
                        config.AddRange(a);
                    }
                }
                return;
            }

            // Otherwise just add.
            this.GetWebhookAuthenticationConfiguration(value, out config);

        }

        /// <summary>
        /// Retrieves any task processor scheduling/recurring configuration
        /// that is exposed via VAF configuration.
        /// </summary>
        /// <param name="input">The object containing the configuration.</param>
        /// <param name="config">All configuration found relating to scheduled execution.</param>
        /// <param name="recurse">Whether to recurse down the object structure exposed.</param>
        protected virtual void GetWebhookAuthenticationConfiguration
        (
            object input,
            out List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>> config,
            bool recurse = true
        )
        {
            config = new List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>>();
            if (null == input)
                return;

            // Get all the fields marked with [DataMember].
            foreach (var f in input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                // No data member attribute then die.
                if (0 == f.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false).Length)
                    continue;

                // If it has the web hook configuration then we want it.
                var webhookConfigurationAttributes = f
                    .GetCustomAttributes(false)
                    .Where(a => a is WebhookConfigurationAttribute)
                    .Cast<WebhookConfigurationAttribute>();
                foreach (var attribute in webhookConfigurationAttributes)
                {
                    if (null == attribute)
                        continue;
                    if (!typeof(IWebhookAuthenticatorProvider).IsAssignableFrom(f.FieldType))
                    {
                        this.Logger?.Warn
                        (
                            $"Found [{attribute.GetType().Name}] but field was not a usable type (actual: {f.FieldType.FullName})"
                        );
                        continue;
                    }

                    // Add the configuration to the collection.
                    var provider = f.GetValue(input) as IWebhookAuthenticatorProvider;
                    if (null == provider || false == provider.Enabled)
                        continue;
                    this.Logger?.Trace($"{f.DeclaringType.FullName}.{f.Name} defines the web hook configuration web hook {attribute.WebHookName}.  Authentication provider is {provider.GetType().FullName}.");
                    config
                        .Add
                        (
                            new Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>
                            (
                                attribute,
                                provider
                            )
                        );
                }

                // Can we recurse?
                if (recurse)
                {
                    var a = new List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>>();
                    this.GetWebhookAuthenticationConfiguration(input, f, out a);
                    config.AddRange(a);
                }
            }

            // Now do the same for properties.
            foreach (var p in input.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // No data member attribute then die.
                if (0 == p.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false).Length)
                    continue;

                // If it has the web hook configuration then we want it.
                var webhookConfigurationAttributes = p
                    .GetCustomAttributes(false)
                    .Where(a => a is WebhookConfigurationAttribute)
                    .Cast<WebhookConfigurationAttribute>();
                foreach (var attribute in webhookConfigurationAttributes)
                {
                    if (null == attribute)
                        continue;
                    if (!typeof(IWebhookAuthenticatorProvider).IsAssignableFrom(p.PropertyType))
                    {
                        this.Logger?.Warn
                        (
                            $"Found [{attribute.GetType().Name}] but property was not a usable type (actual: {p.PropertyType.FullName})"
                        );
                        continue;
                    }

                    // Add the configuration to the collection.
                    var provider = p.GetValue(input) as IWebhookAuthenticatorProvider;
                    if (null == provider || false == provider.Enabled)
                        continue;
                    this.Logger?.Trace($"{p.DeclaringType.FullName}.{p.Name} defines the web hook configuration web hook {attribute.WebHookName}.  Authentication provider is {provider.GetType().FullName}.");
                    config
                        .Add
                        (
                            new Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>
                            (
                                attribute,
                                provider
                            )
                        );
                }

                // Can we recurse?
                if (recurse)
                {
                    var a = new List<Tuple<WebhookConfigurationAttribute, IWebhookAuthenticatorProvider>>();
                    this.GetWebhookAuthenticationConfiguration(input, p, out a);
                    config.AddRange(a);
                }
            }
        }
    }
}
