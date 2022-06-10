using MFiles.VAF.Configuration;
using MFiles.VaultApplications.Logging;
using MFiles.VaultApplications.Logging.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration
{
	/// <summary>
	/// An interface that is used to denote that the configuration class exposes
	/// logging configuration somehow.
	/// </summary>
	public interface IConfigurationWithLoggingConfiguration
	{
		/// <summary>
		/// Returns the logging configuration from whereever it may be configured.
		/// </summary>
		/// <returns>
		/// The logging configuration.
		/// </returns>
		ILoggingConfiguration GetLoggingConfiguration();
	}

	/// <summary>
	/// A base class for configuration that implements <see cref="IConfigurationWithLoggingConfiguration"/>.
	/// </summary>
	[DataContract]
	public abstract class ConfigurationBase
		: IConfigurationWithLoggingConfiguration
	{
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.LoggingConfiguration_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.LoggingConfiguration_HelpText)
		)]
		[Security(ChangeBy = SecurityAttribute.UserLevel.VaultAdmin, ViewBy = SecurityAttribute.UserLevel.VaultAdmin)]
		public NLogLoggingConfiguration Logging { get; set; } = new NLogLoggingConfiguration();

		/// <inheritdoc />
		public ILoggingConfiguration GetLoggingConfiguration()
			=> this.Logging;
	}

	[DataContract]
	public class NLogLoggingConfiguration
		: VaultApplications.Logging.NLog.NLogLoggingConfiguration
	{
		/// <inheritdoc />
		public override IEnumerable<VaultApplications.Logging.NLog.NLogLoggingExclusionRule> GetAllLoggingExclusionRules()
		{
			// Include any other exclusion rules.
			foreach (var r in base.GetAllLoggingExclusionRules() ?? Enumerable.Empty<VaultApplications.Logging.NLog.NLogLoggingExclusionRule>())
				yield return r;

			// If we're set to exclude internal messages then also exclude this library.
			if (false == (this.Advanced?.RenderInternalLogMessages ?? false))
			{
				yield return new VaultApplications.Logging.NLog.NLogLoggingExclusionRule()
				{
					LoggerName = "MFiles.VAF.Extensions.*",
					MinimumLogLevelOverride = LogLevel.Fatal
				};
			}
		}
	}
}
