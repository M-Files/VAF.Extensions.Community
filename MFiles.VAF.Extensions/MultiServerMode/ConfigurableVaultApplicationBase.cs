// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using System;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// A base class that automatically implements the pattern required for broadcasting
	/// configuration changes to other servers.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	/// <remarks>See https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes for further details.</remarks>
	public abstract class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>, IUsesTaskQueue
	where TSecureConfiguration : class, new()
	{
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
