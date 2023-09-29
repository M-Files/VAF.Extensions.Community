using MFiles.VAF.Common;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
	public class AnonymousWebhookAuthenticator
		: WebhookAuthenticatorBase
	{
		public AnonymousWebhookAuthenticator() :
			base(WebhookAuthenticationType.None)
		{
		}

		public override bool IsRequestAuthenticated(EventHandlerEnvironment env, out AnonymousExtensionMethodResult output)
		{
			output = null;
			return true;
		}

		protected override bool ContainsCredentials(EventHandlerEnvironment env)
			=> true;

		protected override bool AreCredentialsValid(EventHandlerEnvironment env)
			=> true;
	}
}
