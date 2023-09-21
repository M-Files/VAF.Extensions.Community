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

	}
}
