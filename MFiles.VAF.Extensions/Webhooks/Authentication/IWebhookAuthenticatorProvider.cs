
namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
    public interface IWebhookAuthenticatorProvider
    {
        bool Enabled { get; }
        IWebhookAuthenticator GetWebhookAuthenticator();
    }
}
