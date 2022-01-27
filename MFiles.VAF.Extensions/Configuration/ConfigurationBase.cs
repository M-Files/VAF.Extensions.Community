using MFiles.VAF.Configuration;
using MFiles.VaultApplications.Logging.Configuration;
using MFiles.VaultApplications.Logging.NLog;
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
		public NLogLoggingConfiguration Logging { get; set; } = new NLogLoggingConfiguration();

		/// <inheritdoc />
		public ILoggingConfiguration GetLoggingConfiguration()
			=> this.Logging;
	}
}
