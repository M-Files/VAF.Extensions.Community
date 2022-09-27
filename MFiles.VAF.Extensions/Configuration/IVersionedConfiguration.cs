using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration
{
	/// <summary>
	/// A root configuration structure with version data.
	/// </summary>
	public interface IVersionedConfiguration
	{
		/// <summary>
		/// The current version of the configuration.
		/// Used for managing upgrading of configuration structures.
		/// </summary>
		Version Version { get; set; }
	}

	/// <summary>
	/// A base class for configuration that implements <see cref="IVersionedConfiguration"/>.
	/// When <see cref="ConfigurationVersionAttribute"/> is used on a derived class, the version
	/// data will be loaded from the attribute and <see cref="System.Version"/> will be populated.
	/// </summary>
	[DataContract]
	public class VersionedConfigurationBase
		: IVersionedConfiguration
	{
		public VersionedConfigurationBase()
		{
			// Set the version from the attribute.
			this.Version = this
				.GetType()
				.GetCustomAttributes(false)?
				.Where(a => a is ConfigurationVersionAttribute)
				.Cast<ConfigurationVersionAttribute>()
				.FirstOrDefault()?
				.Version ?? new Version("0.0");
		}

		/// <inheritdoc />
		[IgnoreDataMember]
		public Version Version { get;set; }

		/// <summary>
		/// The current version of the configuration.
		/// Used for managing upgrading of configuration structures.
		/// </summary>
		[DataMember(EmitDefaultValue = true, Name="Version")]
		[JsonConfEditor(Hidden = true)]
		public string VersionString
		{
			get => this.Version?.ToString();
			set => this.Version = value == null ? new Version("0.0") : Version.Parse(value);
		}

		public bool ShouldSerializeVersionString()
		{
			return this.Version != null
				&& this.Version.ToString() != "0.0";
		}
	}
}
