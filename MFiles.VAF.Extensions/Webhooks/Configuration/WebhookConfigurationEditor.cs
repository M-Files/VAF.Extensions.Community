using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.JsonEditor;
using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Resources;
using MFiles.VAF.Extensions.Webhooks.Authentication;
using Newtonsoft.Json.Linq;
using MFiles.VAF.Configuration.Logging;

namespace MFiles.VAF.Extensions.Webhooks.Configuration
{
	[ObjectMembers(typeof(WebhookConfigurationEditor))]
	public class WebhookConfigurationEditor
		: Dictionary<string, object>, IObjectEditorMembersProvider
	{
		private ILogger Logger { get; } = LogManager.GetLogger<WebhookConfigurationEditor>();
		public static WebhookConfigurationEditor Instance { get; }
			= new WebhookConfigurationEditor();
		public IEnumerable<ObjectEditorMember> GetMembers
		(
			Type memberType,
			IConfigurationRequestContext context,
			ResourceManager resourceManager
		)
		{
			foreach (var p in Instance)
			{
				// Editor type?
				var editorType = typeof(WebhookConfiguration);
				if (p.Value is Type t)
					editorType = t;
				else if (p.Value != null)
					editorType = p.Value.GetType();
				yield return new ObjectEditorMember()
				{
					Key = p.Key,
					Attributes = new List<Attribute>()
					{
						new JsonConfEditorAttribute()
						{
							TypeEditor = editorType.FullName
						}
					}
				};
			}
		}
		public bool TryGetWebhookAuthenticator
		(
			string webhookName, 
			out IWebhookAuthenticator authenticator
		)
		{
			authenticator = null;
			if (string.IsNullOrWhiteSpace(webhookName))
				return false;
			if (false == this.ContainsKey(webhookName))
				return false;

			var value = this[webhookName] as JObject;
			if (value == null)
				return false;

			var type = Instance[webhookName] as Type;
			if (null == type)
				return false;

			try
			{
				authenticator = value.ToObject(type) as IWebhookAuthenticator;
			}
			catch(Exception e)
			{
				this.Logger?.Warn(e, $"Could not load webhook configuration for {webhookName}.");
			}
			return (authenticator != null);

		}
	}
}
