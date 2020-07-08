// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using System;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public abstract class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{
		private string rebroadcastQueueId = null;

		/// <inheritdoc />
		public override string GetRebroadcastQueueId()
		{
			if (string.IsNullOrWhiteSpace(this.rebroadcastQueueId))
				this.rebroadcastQueueId = this.EnableConfigurationRebroadcasting();
			return this.rebroadcastQueueId;
		}

	}
}
