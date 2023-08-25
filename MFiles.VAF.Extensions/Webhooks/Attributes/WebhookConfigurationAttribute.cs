using System;

namespace MFiles.VAF.Extensions
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class WebhookConfigurationAttribute
        : Attribute
    {
        public string WebHookName { get; }
        public WebhookConfigurationAttribute(string webhookName)
        {
            this.WebHookName = webhookName ?? throw new ArgumentNullException(nameof(webhookName));
        }
    }
}
