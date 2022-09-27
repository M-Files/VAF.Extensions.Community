using MFiles.VaultApplications.Logging.Configuration;

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
}
