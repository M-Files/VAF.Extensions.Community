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
	/// A base class for configuration that implements <see cref="IConfigurationWithLoggingConfiguration"/>.
	/// </summary>
	[DataContract]
	public abstract class ConfigurationBase
		: IConfigurationWithLoggingConfiguration, IVersionedConfiguration
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
		[DataMember(EmitDefaultValue = true)]
		public Version Version { get; set; }

		/// <inheritdoc />
		public ILoggingConfiguration GetLoggingConfiguration()
			=> this.Logging;
	}
}
