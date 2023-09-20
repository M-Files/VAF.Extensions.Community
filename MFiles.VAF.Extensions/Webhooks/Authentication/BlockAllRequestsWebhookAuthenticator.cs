using MFiles.VAF;
using MFiles.VAF.Common;
using MFilesAPI;
using System.Text;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
    public class BlockAllRequestsWebhookAuthenticator
        : WebhookAuthenticatorBase
    {
        public BlockAllRequestsWebhookAuthenticator() :
            base(WebhookAuthenticationType.None)
        {
        }

        public override bool IsRequestAuthenticated(EventHandlerEnvironment env, out AnonymousExtensionMethodResult output)
        {
			output = new WebhookOutput()
			{
				ResponseBody = Encoding.UTF8.GetBytes("Access denied")
			}.AsAnonymousExtensionMethodResult();
            return false;
        }

        protected override bool ContainsCredentials(EventHandlerEnvironment env)
            => false;

        protected override bool AreCredentialsValid(EventHandlerEnvironment env)
            => false;
    }
}
