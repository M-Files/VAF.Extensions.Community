// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Linq;
using MFiles.VAF.MultiserverMode;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using System.Reflection;
using System.Collections;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent;
using System.Collections.Generic;
using MFiles.VAF.Extensions.Dashboards.LoggingDashboardContent;
using MFiles.VAF.Extensions.Dashboards.DevelopmentDashboardContent;
using System.Threading.Tasks;
using MFiles.VAF.Extensions.Logging;
using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Configuration.JsonEditor;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using System.Dynamic;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A base class providing common functionality for vault applications that use the VAF Extensions library.
	/// If a vault application uses the library, but does not inherit from this class, then various library functionality
	/// may not work.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
		where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// Contains information about VAF configuration that
		/// control when task processing repeats.
		/// </summary>
		public RecurringOperationConfigurationManager<TSecureConfiguration> RecurringOperationConfigurationManager { get; private set; }

		/// <summary>
		/// Expose the task queue resolver (was protected, now internal so that we can use it).
		/// </summary>
		internal new TaskQueueResolver TaskQueueResolver
		{
			get => base.TaskQueueResolver;
			set => base.TaskQueueResolver = value;
		}

		/// <summary>
		/// Expose the running configuration (was protected, now internal so that we can use it).
		/// </summary>
		internal new TSecureConfiguration Configuration
		{
			get => base.Configuration;
			set => base.Configuration = value;
		}

		public ConfigurableVaultApplicationBase()
			: this(new ExtensionsNLogLogManager())
		{
		}
		protected ConfigurableVaultApplicationBase(ILogManager logManager)
			: base()
		{
			LogManager.Current = logManager ?? new ExtensionsNLogLogManager();
			this.Logger = LogManager.GetLogger(this.GetType());
			this.WebhookAuthenticationConfigurationManager = this.GetWebhookAuthenticationConfigurationManager();
		}
		private TaskQueueBackgroundOperationManager<TSecureConfiguration> taskQueueBackgroundOperationManager;

		private readonly object _lock = new object();

		/// <summary>
		/// The task queue background operation manager for this application.
		/// </summary>
		protected TaskQueueBackgroundOperationManager<TSecureConfiguration> TaskQueueBackgroundOperationManager
		{
			get
			{
				if (null != this.taskQueueBackgroundOperationManager)
					return this.taskQueueBackgroundOperationManager;
				lock (this._lock)
				{
					try
					{
						taskQueueBackgroundOperationManager =
							taskQueueBackgroundOperationManager ?? new TaskQueueBackgroundOperationManager<TSecureConfiguration>(this);
					}
					catch
					{
						// This may except if the vault is not yet started.
						// Allow it to return null.
					}
					return taskQueueBackgroundOperationManager;
				}
			}
			private set => taskQueueBackgroundOperationManager = value;
		}

		protected internal new IApplicationLicense License
		{
			get { return base.License; }
			set { base.License = value; }
		}

		private bool startApplicationCalled = false;

		protected override void StartApplication()
		{
			this.startApplicationCalled = true;
			this.RecurringOperationConfigurationManager = new RecurringOperationConfigurationManager<TSecureConfiguration>(this);
			this.ApplicationOverviewDashboardContentRenderer = this.GetApplicationOverviewDashboardContentRenderer();
			this.AsynchronousDashboardContentRenderer = this.GetAsynchronousDashboardContentRenderer();
			this.AsynchronousDashboardContentProviders.AddRange(this.GetAsynchronousDashboardContentProviders());
			this.LoggingDashboardContentRenderer = this.GetLoggingDashboardContentRenderer();

#if DEBUG
			// In debug builds we want to show the referenced assemblies and the like.
			{
				this.DevelopmentDashboardContentRenderer = this.GetDevelopmentDashboardContentRenderer();
				Task.Run(
					() =>
					{
						this.DevelopmentDashboardContentRenderer?.PopulateReferencedAssemblies<TSecureConfiguration>();
					}
				);
			}
#endif

			base.StartApplication();
		}

		/// <inheritdoc />
		protected override IMetadataStructureValidator CreateMetadataStructureValidator()
		{
			return new MFiles.VAF.Extensions.Configuration.MetadataStructureValidator();
		}

		public override Schema GetConfigurationSchema(IConfigurationRequestContext context)
		{
			var s = base.GetConfigurationSchema(context);

			// Create a schema for the different types of webhook auth.
			// We can use the WebhookConfiguration class as an entry point.
			var authSchema = new SchemaGenerator()
			{
				ConfigurationRequestContext = context,
				ResourceManager = this.ConfManager.ResourceManager
			}.GenerateSchema(typeof(WebhookConfiguration));
			foreach (var kvp in authSchema.Editors)
			{
				if(!s.Editors.ContainsKey(kvp.Key))
					s.Editors.Add(kvp.Key, kvp.Value);
			}

			// Fix the schema.
			{
				// Find the schema editor entry for the current config type.
				var configurationElement = s
					.Editors
					.FirstOrDefault(e => e.Key == typeof(TSecureConfiguration).FullName)
					.Value as IDictionary<string, object>;
				if (!(configurationElement == default))
				{
					// Get the members (configurable properties) for this type.
					var members = ((IDictionary<string, object>)configurationElement["members"]);
					if(!(members == default) && members.ContainsKey(nameof(ConfigurationBase.WebhookConfiguration)))
					{
						var webhookConfigurationMember = members[nameof(ConfigurationBase.WebhookConfiguration)] as IDictionary<string, object>;

						// Update the editor type name so that it's unique...
						var typeName = (webhookConfigurationMember != null && webhookConfigurationMember.ContainsKey("type")) ? webhookConfigurationMember["type"].ToString() : (string)null;
						if (!string.IsNullOrWhiteSpace(typeName) && s.Editors.ContainsKey(typeName))
						{
							// What should the new type name be?
							var newType = $"MFiles.VAF.Extensions.MemberProxy`3[[{this.GetType().FullName}],[MFiles.VAF.Extensions.Webhooks.Configuration.WebhookConfigurationEditor],[System.Object]]";

							// Replace the type in the editors.
							var existingType = s.Editors[typeName];
							if ((existingType as IDictionary<string, object>)?["type"]?.ToString() == typeName)
							{
								s.Editors.Remove(typeName);
								(existingType as IDictionary<string, object>)["type"] = newType;
								s.Editors.Add(newType, existingType);
								webhookConfigurationMember["type"] = newType;
							}
						}

						// If there are no webhooks then remove the webhook config member entirely.
						if (!WebhookConfigurationEditor.Instance.Any())
						{
							members.Remove(nameof(ConfigurationBase.WebhookConfiguration));
						}
					}

				}
			}

			return s;
		}
	}
}
