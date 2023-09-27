using MFiles.VAF.Common;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using MFiles.VAF.Extensions.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{

		public List<WebhookMethodInfo<TSecureConfiguration>> Webhooks { get; set; }
			= new List<WebhookMethodInfo<TSecureConfiguration>>();

		/// <summary>
		/// <returns>The created method info object.</returns>
		protected override IVaultExtensionMethodInfo CreateVaultAnonymousExtensionMethodInfo(
			MethodInfo methodInfo,
			object instance,
			VaultAnonymousExtensionMethodAttribute attribute
		)
		{
			// Web hooks are first-party.
			if (attribute is WebhookAttribute a)
			{
				var webhookMethodInfo = new WebhookMethodInfo<TSecureConfiguration>
				(
					this,
					a,
					methodInfo,
					instance
				);
				this.Webhooks.Add(webhookMethodInfo);

				// Add the webhook to the configuration editor.
				var authenticationAttribute = methodInfo.GetCustomAttribute<WebhookAuthenticationAttribute>();
				if (authenticationAttribute != null)
				{
					if (authenticationAttribute is AnonymousWebhookAuthenticationAttribute)
					{
						// No config.
					}
					else
					{
						WebhookConfigurationEditor.Instance.Add(a.Name, authenticationAttribute.ConfigurationType);
					}
				}
				else
				{
					WebhookConfigurationEditor.Instance.Add(a.Name, typeof(WebhookConfiguration));
				}
				
				return webhookMethodInfo;
			}

			// Use the base implementation.
			return base.CreateVaultAnonymousExtensionMethodInfo(methodInfo, instance, attribute);
		}

		/// <summary>
		/// Contains information about VAF configuration that
		/// control when task processing repeats.
		/// </summary>
		public WebhookAuthenticationConfigurationManager<TSecureConfiguration> WebhookAuthenticationConfigurationManager { get; private set; }

		protected virtual WebhookAuthenticationConfigurationManager<TSecureConfiguration> GetWebhookAuthenticationConfigurationManager()
			=> new WebhookAuthenticationConfigurationManager<TSecureConfiguration>(this);
	}
}
