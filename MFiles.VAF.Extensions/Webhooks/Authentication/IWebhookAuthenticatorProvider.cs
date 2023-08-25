
namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
	/// <summary>
	/// Provides instances of <see cref="IWebhookAuthenticator"/>.
	/// </summary>
    public interface IWebhookAuthenticatorProvider
    {
		/// <summary>
		/// Whether this provider is enabled or not.
		/// </summary>
        bool Enabled { get; }

		/// <summary>
		/// Gets the <see cref="IWebhookAuthenticator"/>
		/// </summary>
		/// <returns></returns>
		IWebhookAuthenticator GetWebhookAuthenticator();
    }
}
