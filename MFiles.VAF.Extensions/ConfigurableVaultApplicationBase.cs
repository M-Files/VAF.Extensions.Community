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
		public RecurringOperationConfigurationManager<TSecureConfiguration> RecurringOperationConfigurationManager { get; }

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
		}

		public ConfigurableVaultApplicationBase()
			: base()
		{
			this.RecurringOperationConfigurationManager = new RecurringOperationConfigurationManager<TSecureConfiguration>(this);
		}
		private TaskQueueBackgroundOperationManager<TSecureConfiguration> taskQueueBackgroundOperationManager;

		private object _lock = new object();

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
	}
}
