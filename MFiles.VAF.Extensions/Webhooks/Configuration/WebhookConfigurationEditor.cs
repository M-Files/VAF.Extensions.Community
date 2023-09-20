using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.JsonEditor;
using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Resources;

namespace MFiles.VAF.Extensions.Webhooks.Configuration
{
	[ObjectMembers(typeof(WebhookConfigurationEditor))]
	public class WebhookConfigurationEditor
		: Dictionary<string, object>, IObjectEditorMembersProvider
	{
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
	}
}
