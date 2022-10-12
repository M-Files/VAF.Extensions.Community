// ReSharper disable once CheckNamespace
using System.Collections.Generic;
using System.Resources;
using System.Globalization;
using System.Linq;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A resource manager that can retrieve resources from a set of files.
	/// </summary>
	public class CombinedResourceManager
		: MFiles.VaultApplications.Logging.Resources.CombinedResourceManager
	{
		/// <summary>
		/// Creates a <see cref="CombinedResourceManager"/>
		/// that wraps the given resource managers.
		/// </summary>
		/// <param name="resourceManagers"></param>
		public CombinedResourceManager(params ResourceManager[] resourceManagers)
			: base(true, resourceManagers)
		{
		}

		/// <inheritdoc />
		protected override IEnumerable<ResourceManager> GetDefaultResourceManagers()
		{
			// Get the ones exposed by the base library.
			foreach(var resourceManager in base.GetDefaultResourceManagers() ?? new ResourceManager[0])
				yield return resourceManager;

			// Return ones from this library.
			yield return Resources.AsynchronousOperations.ResourceManager;
			yield return Resources.Configuration.ResourceManager;
			yield return Resources.Dashboard.ResourceManager;
			yield return Resources.Schedule.ResourceManager;
			yield return Resources.Time.ResourceManager;
			yield return Resources.Licensing.ResourceManager;
			yield return Resources.Logging.ResourceManager;
			yield return Resources.Exceptions.Configuration.ResourceManager;
			yield return Resources.Exceptions.Dashboard.ResourceManager;
			yield return Resources.Exceptions.InternalOperations.ResourceManager;
			yield return Resources.Exceptions.MFSearchBuilderExtensionMethods.ResourceManager;
			yield return Resources.Exceptions.ObjVerExExtensionMethods.ResourceManager;
			yield return Resources.Exceptions.TaskQueueBackgroundOperations.ResourceManager;
			yield return Resources.Exceptions.VaultInteraction.ResourceManager;

		}
	}
}
