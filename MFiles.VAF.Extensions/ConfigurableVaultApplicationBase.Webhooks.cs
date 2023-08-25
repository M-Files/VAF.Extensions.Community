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

		protected List<WebhookMethodInfo<TSecureConfiguration>> Webhooks { get; set; }
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
					this.WebhookAuthenticationConfigurationManager,
					a,
					methodInfo,
					instance
				);
				this.Webhooks.Add(webhookMethodInfo);
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
