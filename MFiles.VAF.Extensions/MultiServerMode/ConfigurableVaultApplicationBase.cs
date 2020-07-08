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
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// The rebroadcast queue.  Populated during the first call to
		/// <see cref="GetRebroadcastQueueId"/>.
		/// </summary>
		private string configurationRebroadcastQueueId = null;

		/// <inheritdoc />
		public override string GetRebroadcastQueueId()
		{
			// If we do not have a rebroadcast queue for the configuration data
			// then create one.
			if (string.IsNullOrWhiteSpace(this.configurationRebroadcastQueueId))
				this.configurationRebroadcastQueueId = this.EnableConfigurationRebroadcasting();

			// Return the broadcast queue.
			return this.configurationRebroadcastQueueId;
		}

	}
}
