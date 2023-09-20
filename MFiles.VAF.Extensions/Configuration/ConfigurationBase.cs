using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.NLog;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration
{

	/// <summary>
	/// A base class for configuration that implements <see cref="IConfigurationWithLoggingConfiguration"/>.
	/// </summary>
	[DataContract]
	[UsesConfigurationResources]
	[UsesLoggingResources]
	public abstract class ConfigurationBase
		: VersionedConfigurationBase,
			IConfigurationWithLoggingConfiguration,
			IConfigurationWithWebhookConfiguration
	{

		[DataMember]
		[Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
		public WebhookConfigurationEditor WebhookConfiguration { get; set; }
			= new WebhookConfigurationEditor();

		[DataMember(EmitDefaultValue = false)]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.LoggingConfiguration_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.LoggingConfiguration_HelpText)
		)]
		[Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
		public NLogLoggingConfiguration Logging { get; set; }

		/// <inheritdoc />
		public ILoggingConfiguration GetLoggingConfiguration()
			=> this.Logging ?? new NLogLoggingConfiguration();
	}

	[DataContract]
	public class NLogLoggingConfiguration
		: MFiles.VAF.Configuration.Logging.NLog.Configuration.NLogLoggingConfiguration
	{
		/// <inheritdoc />
		public override IEnumerable<NLogLoggingExclusionRule> GetAllLoggingExclusionRules()
		{
			// Include any other exclusion rules.
			foreach (var r in base.GetAllLoggingExclusionRules() ?? Enumerable.Empty<NLogLoggingExclusionRule>())
				yield return r;

			// If we're set to exclude internal messages then also exclude the task manager ex (spammy).
			if (false == (this.Advanced?.RenderInternalLogMessages ?? false))
			{
				yield return new NLogLoggingExclusionRule()
				{
					LoggerName = "MFiles.VAF.Extensions.TaskManagerEx*",
					MinimumLogLevelOverride = LogLevel.Fatal
				};
			}
		}
	}
}
