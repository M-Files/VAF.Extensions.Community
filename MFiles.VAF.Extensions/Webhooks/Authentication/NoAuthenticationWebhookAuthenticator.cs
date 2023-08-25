using extensionstest2.Webhooks.Configuration;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFilesAPI;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
    public class NoAuthenticationWebhookAuthenticator
        : WebhookAuthenticatorBase
    {
        public NoAuthenticationWebhookAuthenticator() :
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
