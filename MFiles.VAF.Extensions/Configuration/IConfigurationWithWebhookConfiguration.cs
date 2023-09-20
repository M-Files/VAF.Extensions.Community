using MFiles.VAF.Extensions.Webhooks.Configuration;

namespace MFiles.VAF.Extensions.Configuration
{
	public interface IConfigurationWithWebhookConfiguration
	{
		WebhookConfigurationEditor WebhookConfiguration { get; set; }
	}
}
