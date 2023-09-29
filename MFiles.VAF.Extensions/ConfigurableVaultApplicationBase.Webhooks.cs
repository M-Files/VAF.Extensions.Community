using MFiles.VAF.Common;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using MFiles.VAF.Extensions.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.AppTasks;
using System.Diagnostics;

namespace MFiles.VAF.Extensions
{
	public partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{

		public List<WebhookMethodInfo<TSecureConfiguration>> Webhooks { get; set; }
			= new List<WebhookMethodInfo<TSecureConfiguration>>();

		/// <summary>
		/// The processor that will process items in the task queue.
		/// </summary>
		protected internal TaskProcessor<AsynchronousWebhookTaskDirective> AsynchronousWebhookTaskProcessor { get; set; }

		private TaskQueueConfiguration asynchronousWebhookTaskQueueConfiguration { get; } = new TaskQueueConfiguration();

		/// <summary>
		/// Returns information about the task queue used by this remote service.
		/// </summary>
		/// <returns></returns>
		protected internal virtual TaskQueueConfiguration GetAsynchronousWebhookTaskQueueConfiguration()
			=> this.asynchronousWebhookTaskQueueConfiguration;

		protected ITaskQueueProcessor AsynchronousWebhookTaskQueueProcessor { get; set; }

		/// <summary>
		/// Uploads items.
		/// </summary>
		protected virtual void ExecuteAsynchronousWebhook(ITaskProcessingJob<AsynchronousWebhookTaskDirective> job)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(job?.Directive?.WebhookName))
				return;

			// Get the associated webhook.
			var webhook = this.Webhooks?.FirstOrDefault(wh => wh.WebhookName == job?.Directive?.WebhookName);
			if (null == webhook)
				return;

			// Go!
			webhook.Execute(job.Directive.AsEnvironment(job.Vault));
		}

		/// <summary>
		/// Registers the task queue that the remote service will use.
		/// </summary>
		protected virtual void RegisterAsynchronousWebhookTaskQueue()
		{
			// For the moment only register the webhook stuff if there are webhooks.
			if (!this.Webhooks.Any())
				return;

			var taskQueueConfiguration = this.GetAsynchronousWebhookTaskQueueConfiguration();
			if (null == taskQueueConfiguration)
				return;

			// Set up the processor.
			this.AsynchronousWebhookTaskProcessor = new TaskProcessor<AsynchronousWebhookTaskDirective>
			(
				taskQueueConfiguration.TaskType,
				this.ExecuteAsynchronousWebhook,
				taskQueueConfiguration.TransactionMode,
				taskQueueConfiguration.ProcessorSettings
			);

			// Set up the queue.
			this.AsynchronousWebhookTaskQueueProcessor = this
				.TaskManager
				.RegisterQueue
				(
					taskQueueConfiguration.QueueID,
					new[] { this.AsynchronousWebhookTaskProcessor },
					taskQueueConfiguration.TaskQueueProcessingBehavior,
					taskQueueConfiguration.QueueSettings
				);
		}

		/// <summary>
		/// <returns>The created method info object.</returns>
		protected override IVaultExtensionMethodInfo CreateVaultAnonymousExtensionMethodInfo(
			MethodInfo methodInfo,
			object instance,
			VaultAnonymousExtensionMethodAttribute attribute
		)
		{
			// Web hooks are first-party.
			if (attribute is WebhookAttribute a)
			{
				var webhookMethodInfo = new WebhookMethodInfo<TSecureConfiguration>
				(
					this,
					a,
					methodInfo,
					instance
				);
				this.Webhooks.Add(webhookMethodInfo);

				// Add the webhook to the configuration editor.
				var authenticationAttribute = methodInfo.GetCustomAttribute<WebhookAuthenticationAttribute>();
				if (authenticationAttribute != null)
				{
					if (authenticationAttribute is AnonymousWebhookAuthenticationAttribute)
					{
						// No config.
					}
					else
					{
						WebhookConfigurationEditor.Instance.Add(a.Name, authenticationAttribute.ConfigurationType);
					}
				}
				else
				{
					WebhookConfigurationEditor.Instance.Add(a.Name, typeof(WebhookConfiguration));
				}
				
				return webhookMethodInfo;
			}

			// Use the base implementation.
			return base.CreateVaultAnonymousExtensionMethodInfo(methodInfo, instance, attribute);
		}

		/// <summary>
		/// Contains information about VAF configuration that
		/// control when task processing repeats.
		/// </summary>
		public WebhookAuthenticationConfigurationManager<TSecureConfiguration> WebhookAuthenticationConfigurationManager { get; private set; }

		protected virtual WebhookAuthenticationConfigurationManager<TSecureConfiguration> GetWebhookAuthenticationConfigurationManager()
			=> new WebhookAuthenticationConfigurationManager<TSecureConfiguration>(this);
	}
}
