using MFiles.VAF.Configuration.Resources;
using System.Resources;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Used to mark a configuration class as using resources from <see cref="Resources.Configuration"/>.
	/// </summary>
	public class UsesConfigurationResourcesAttribute
		: UsesResourcesAttribute
	{
		public UsesConfigurationResourcesAttribute()
			: base(typeof(Resources.Configuration)) { }
	}
}
