// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Core;
using MFiles.VAF.MultiserverMode;
using System;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public abstract class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>, IUsesTaskQueue
	where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// The broadcast task processor used to broadcast
		/// application configuration changes to other servers.
		/// </summary>
		/// <remarks>Automatically populated during <see cref="RegisterTaskQueues" />
		/// with the value returned by
		/// <see cref="GetConfigurationRebroadcastTaskProcessor"/>.</remarks>
		protected AppTaskBatchProcessor ConfigurationRebroadcastTaskProcessor { get; private set; }

		/// <inheritdoc />
		public override string GetRebroadcastQueueId()
		{
			return $"{this.GetType().FullName.Replace(".", "-")}-ConfigurationRebroadcastQueue";
		}
		
		/// <summary>
		/// Returns the broadcast task processor that should be used to
		/// rebroadcast application configuration changes to other servers.
		/// </summary>
		/// <returns>The task processor to use.</returns>
		protected virtual AppTaskBatchProcessor GetConfigurationRebroadcastTaskProcessor()
		{
			// Create the broadcast processor.
			return this.CreateBroadcastTaskProcessor
			(
				// Use the rebroadcast queue ID for this.
				this.GetRebroadcastQueueId(),

				// No task handlers required.
				new System.Collections.Generic.Dictionary<string, TaskProcessorJobHandler>(),

				// Use the default vault extension proxy method ID.
				vaultExtensionProxyMethodId: this.GetVaultExtensionMethodEventHandlerProxyName()
			);
		}

		#region Implementation of IUsesTaskQueue

		/// <inheritdoc />
		public virtual void RegisterTaskQueues()
		{
			// Create the configuration rebroadcast task processor.
			this.ConfigurationRebroadcastTaskProcessor
				= this.GetConfigurationRebroadcastTaskProcessor();
		}

		#endregion

	}
}
