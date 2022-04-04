using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration
{
	public interface IVersionedConfiguration
	{
		/// <summary>
		/// The current version of the configuration.
		/// Used for managing upgrading of configuration structures.
		/// </summary>
		Version Version { get; set; }
	}
	internal class VersionedConfiguration
		: IVersionedConfiguration
	{
		/// <inheritdoc />
		[DataMember(EmitDefaultValue = true)]
		public Version Version { get; set; }
	}
}
