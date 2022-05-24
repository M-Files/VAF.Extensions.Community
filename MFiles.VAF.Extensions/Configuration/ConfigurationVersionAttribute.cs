using MFilesAPI;
using System;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions.Configuration
{
	/// <summary>
	/// Allows definition of the version of this configuration structure.
	/// Expected to be used on a class that implemented <see cref="IVersionedConfiguration"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ConfigurationVersionAttribute : Attribute
	{
		public Version Version { get; set; }
		public bool UsesCustomNVSLocation { get; set; }
		public string Namespace { get; set; }
		public string Key { get; set; }
		public MFNamedValueType NamedValueType { get; set; } = MFNamedValueType.MFSystemAdminConfiguration;
		public Type PreviousVersionType { get; set; } 

		public ConfigurationVersionAttribute(string version)
		{
			this.Version = Version.Parse(version);
		}
	}

	/// <summary>
	/// Allows a declarative approach to configuration upgrading.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ConfigurationUpgradeMethodAttribute: Attribute
	{
		public ConfigurationUpgradeMethodAttribute()
		{
		}
	}
}
