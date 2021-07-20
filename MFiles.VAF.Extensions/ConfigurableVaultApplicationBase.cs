﻿// ReSharper disable once CheckNamespace
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

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A base class that automatically implements the pattern required for broadcasting
	/// configuration changes to other servers.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	/// <remarks>See https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes for further details.</remarks>
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>, IUsesTaskQueue
	where TSecureConfiguration : class, new()
	{
		private TaskQueueBackgroundOperationManager taskQueueBackgroundOperationManager;

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
		protected TaskQueueBackgroundOperationManager TaskQueueBackgroundOperationManager
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
							taskQueueBackgroundOperationManager ?? new TaskQueueBackgroundOperationManager(this);
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

		/// <inheritdoc />
		public override string GetRebroadcastQueueId()
		{
			// If we do not have a rebroadcast queue for the configuration data
			// then create one.
			if (null == this.ConfigurationRebroadcastTaskProcessor)
			{
				// Enable the configuration rebroadcasting.
				this.EnableConfigurationRebroadcasting
					(
					out AppTaskBatchProcessor processor,
					out string queueId
					);

				// Populate references to the task processor and queue Id.
				this.ConfigurationRebroadcastQueueId = queueId;
				this.ConfigurationRebroadcastTaskProcessor = processor;
			}

			// Return the broadcast queue Id.
			return this.ConfigurationRebroadcastQueueId;
		}

		#region Implementation of IUsesTaskQueue

		/// <inheritdoc />
		public virtual void RegisterTaskQueues()
		{
		}

		#endregion
	}
}
