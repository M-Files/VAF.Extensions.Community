// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.MultiserverMode;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A base class that automatically implements the pattern required for broadcasting
	/// configuration changes to other servers.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	/// <remarks>See https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes for further details.</remarks>
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{
		private TaskQueueBackgroundOperationManager<TSecureConfiguration> taskQueueBackgroundOperationManager;

		/// <summary>
		/// The rebroadcast queue Id.
		/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
		/// </summary>
		protected string ConfigurationRebroadcastQueueId { get; private set; }

		/// <summary>
		/// The rebroadcast queue processor.
		/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
		/// </summary>
		protected AppTaskBatchProcessor ConfigurationRebroadcastTaskProcessor { get; private set; }

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


		/// <summary>
		/// Task manager.
		/// </summary>
		public new TaskManagerEx TaskManager
		{
			get => base.TaskManager as TaskManagerEx;
			protected set => base.TaskManager = value;
		}

		/// <summary>
		/// Initializes the newer task manager, and regsiters all queues and processors
		/// that have been discovered by the <see cref="TaskQueueResolver"/>.
		/// Should be called after StartOperations(), before StartApplication().
		/// </summary>
		protected override void InitializeTaskManager()
		{
			// Report an error to the event log if queues were declared, but tasks aren't supported.
			if (AppTasks.TaskManager.IsSupported(PermanentVault))
			{
				// Initialize the new task manager, and register the queues from the resolver.
				this.TaskManager = new TaskManagerEx(
						this.GetType().Namespace, this.PermanentVault, GetTransactionRunner());
				this.TaskQueueResolver?.RegisterAll(this.TaskManager);
				TaskStatusHelper.Attach(this.TaskManager);
			}
			else if (this.TaskQueueResolver != null && this.TaskQueueResolver.GetQueues().Length > 0)
			{
				// Report an error if task manager is not supported, but queues have been resolved.
				// If someone tries to declare queues programmatically, the TaskManager should be null,
				// and therefore throw its own errors.
				SysUtils.ReportErrorToEventLog(
						"The application requires support for tasks. Please upgrade to M-Files 20.4 or later.");
			}
		}
	}
}
