using System;

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
		public ConfigurationVersionAttribute(string version)
		{
			this.Version = Version.Parse(version);
		}
	}
}
