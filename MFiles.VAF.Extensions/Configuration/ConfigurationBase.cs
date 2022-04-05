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
		: VersionedConfigurationBase, IConfigurationWithLoggingConfiguration
	{
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
}
