using MFiles.VAF.Configuration.Resources;
using System.Resources;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Used to mark a configuration class as using resources from <see cref="Resources.Logging"/>.
	/// </summary>
	public class UsesLoggingResources
		: ResourceManagerProviderAttribute
	{
		public override ResourceManager ResourceManager
			=> Resources.Logging.ResourceManager;
	}
}
