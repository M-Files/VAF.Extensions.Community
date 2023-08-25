
namespace MFiles.VAF.Extensions.Webhooks
{
    //public class WebhookInput
    //{
    //    public NamedValues RequestHeaders { get; set; }
    //        = new NamedValues();
    //    public byte[] RequestBody { get; set; }
    //    public string HttpMethod { get; set; }

    //    public WebhookInput
    //    (
    //        EventHandlerEnvironment env = null
    //    )
    //        : this(env?.InputHttpHeaders, env?.InputBytes, env?.InputHttpMethod)
    //    {
    //    }

    //    public WebhookInput
    //    (
    //        NamedValues requestHeaders, 
    //        byte[] requestBody,
    //        string httpMethod
    //    )
    //    {
    //        this.RequestHeaders = requestHeaders;
    //        this.RequestBody = requestBody;
    //        this.HttpMethod = httpMethod;
    //    }

    //    public static WebhookInput CreateFromEnvironment(EventHandlerEnvironment env)
    //        => new WebhookInput(env);

    //    public static WebhookInput CreateFromEnvironment(ISerializer serializer, EventHandlerEnvironment env, Type type)
    //    {
    //        return (WebhookInput)Activator.CreateInstance
    //        (
    //            typeof(WebhookInput<>).MakeGenericType(type),
    //            new object[]
    //            {
    //                serializer,
    //                env
    //            }
    //        );
    //    }
    //}
    //public class WebhookInput<TInputType>
    //    : WebhookInput
    //{
    //    public ISerializer Serializer { get; set; }
    //    private TInputType requestBody { get; set; }
    //    public new TInputType RequestBody
    //    {
    //        get => this.requestBody;
    //        set
    //        {
    //            this.requestBody = value;
    //            base.RequestBody = this.Serializer.Serialize(value);
    //        }
    //    }

    //    public WebhookInput
    //    (
    //        ISerializer serializer,
    //        EventHandlerEnvironment env = null
    //    )
    //        : base(env?.InputHttpHeaders, null, env?.InputHttpMethod)
    //    {
    //        this.Serializer = serializer
    //            ?? throw new ArgumentNullException(nameof(serializer));
    //        if (null != env?.InputBytes)
    //            this.RequestBody = this.Serializer.Deserialize<TInputType>(env?.InputBytes);
    //    }

    //    public WebhookInput
    //    (
    //        ISerializer serializer,
    //        NamedValues requestHeaders,
    //        TInputType requestBody,
    //        string httpMethod
    //    )
    //        : base(requestHeaders, null, httpMethod)
    //    {
    //        this.Serializer = serializer
    //            ?? throw new ArgumentNullException(nameof(serializer));
    //        if (requestBody != null)
    //            this.RequestBody = requestBody;
    //    }
    //}
}
