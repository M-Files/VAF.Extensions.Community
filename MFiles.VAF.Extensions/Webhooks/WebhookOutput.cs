using MFiles.VAF;
using MFilesAPI;
using System;
using static MFiles.VAF.Configuration.ValidationResultForValidation;

namespace MFiles.VAF.Extensions.Webhooks
{
	public class WebhookOutput
        : IConvertableToAnonymousExtensionMethodResult
    {
        public NamedValues ResponseHeaders { get; set; }
        public byte[] ResponseBody { get; set; }

        public WebhookOutput
        (
            AnonymousExtensionMethodResult result = null
        )
            : this(result?.OutputHttpHeadersValue, result?.OutputBytesValue)
        {
        }

        public WebhookOutput
        (
            NamedValues responseHeaders,
            byte[] responseBody
        )
        {
            this.ResponseHeaders = responseHeaders;
            this.ResponseBody = responseBody;
        }

        public virtual AnonymousExtensionMethodResult AsAnonymousExtensionMethodResult()
        {
            return new AnonymousExtensionMethodResult()
            {
                OutputBytesValue = this.ResponseBody,
                OutputHttpHeadersValue = this.ResponseHeaders
            };
        }
    }
    public class WebhookOutput<TOutputType>
        : WebhookOutput
    {
        public ISerializer Serializer { get; set; }
        private TOutputType responseBody { get; set; }
        public new TOutputType ResponseBody
        {
            get => this.responseBody;
            set
            {
                this.responseBody = value;
                base.ResponseBody = this.Serializer.Serialize(value);
            }
        }

        public WebhookOutput
        (
            ISerializer serializer,
            AnonymousExtensionMethodResult result = null
        ) : base(result?.OutputHttpHeadersValue, null)
        {
            this.Serializer = serializer
                ?? throw new ArgumentNullException(nameof(serializer));
            if (null != result?.OutputBytesValue)
                this.ResponseBody = this.Serializer.Deserialize<TOutputType>(result?.OutputBytesValue);
        }

        public WebhookOutput
        (
            ISerializer serializer,
            NamedValues responseHeaders,
            TOutputType responseBody
        ) : base(responseHeaders, null)
        {
            this.Serializer = serializer
                ?? throw new ArgumentNullException(nameof(serializer));
            if (responseBody != null)
                this.ResponseBody = responseBody;
        }
    }
}
