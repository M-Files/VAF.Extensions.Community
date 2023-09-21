
namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
	/// <summary>
	/// Provides instances of <see cref="IWebhookAuthenticator"/>.
	/// </summary>
    public interface IWebhookAuthenticatorProvider
    {
		/// <summary>
		/// Gets the <see cref="IWebhookAuthenticator"/>
		/// </summary>
		/// <returns></returns>
		IWebhookAuthenticator GetWebhookAuthenticator();
    }
}
