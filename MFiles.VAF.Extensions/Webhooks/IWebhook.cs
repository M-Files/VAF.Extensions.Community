using MFiles.VAF.Extensions.Webhooks.Authentication;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MFiles.VAF.Extensions.Webhooks
{
	public interface IWebhook
	{
		bool Enabled { get; }
		string Name { get; }
		string HttpMethod { get; }
		bool SupportsNoAuthentication { get; }
		Type SerializerType { get; }
		bool SupportsAuthenticator(Type type);
		bool SupportsAuthenticator<TAuthenticatorType>()
			where TAuthenticatorType : IWebhookAuthenticator;

		bool TryGetHandlerMethodInfo(out MethodInfo methodInfo, out object instance);

	}
	public abstract class Webhook
		: IWebhook
	{
		public bool Enabled { get; set; } = true;

		public string Name { get; set; }

		public string HttpMethod { get; set; }

		public bool SupportsNoAuthentication { get; set; }

		public Type SerializerType { get; set; } = typeof(NewtonsoftJsonSerializer);

		public virtual bool SupportsAuthenticator(Type type)
			=> true;

		public abstract bool TryGetHandlerMethodInfo(out MethodInfo methodInfo, out object instance);

		public bool SupportsAuthenticator<TAuthenticatorType>() where TAuthenticatorType : IWebhookAuthenticator
			=> this.SupportsAuthenticator(typeof(TAuthenticatorType));

		public Webhook(string name)
		{
			this.Name = name;
		}
		public Webhook(string name, string httpMethod = "GET", bool supportsNoAuthentication = false)
			: this(name)
		{
			this.HttpMethod = httpMethod;
			this.SupportsNoAuthentication = supportsNoAuthentication;
		}
	}
	public class SimpleWebhook
		: Webhook
	{
		protected MethodInfo MethodInfo { get; set; }
		protected object Instance { get; set; }

		public SimpleWebhook(string name, MethodInfo methodInfo, object instance)
			: base(name)
		{
			this.Name = name;
			this.MethodInfo = methodInfo;
			this.Instance = instance;
		}
		public SimpleWebhook(string name, MethodInfo methodInfo, object instance, string httpMethod = "GET", bool supportsNoAuthentication = false)
			: this(name, methodInfo, instance)
		{
			this.HttpMethod = httpMethod;
			this.SupportsNoAuthentication = supportsNoAuthentication;
		}

		public override bool TryGetHandlerMethodInfo(out MethodInfo methodInfo, out object instance)
		{
			methodInfo = this.MethodInfo;
			instance = this.Instance;
			return true;
		}
	}
}
