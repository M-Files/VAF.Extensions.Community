using MFiles.VAF;
using MFiles.VAF.Common;
using MFilesAPI;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{

    public interface IWebhookAuthenticator
    {
        bool IsRequestAuthenticated(EventHandlerEnvironment env, out AnonymousExtensionMethodResult output);
    }
}
