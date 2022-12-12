using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Resources;
using System;
using System.Linq;
using System.Resources;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = true, Inherited = true)]
	public class UsesResourcesAttribute
		: ResourceManagerProviderAttribute
	{
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(UsesResourcesAttribute));

		/// <summary>
		/// The class that defines a resource manager.
		/// </summary>
		protected Type ResourceClass { get; private set; }

		public UsesResourcesAttribute(Type resourceClass)
		{
			this.ResourceClass = resourceClass
				?? throw new ArgumentNullException(nameof(resourceClass));
		}

		/// <summary>
		/// The cached resource manager.
		/// Populated by calling <see cref="ResourceManager"/>.
		/// Populated with the result of <see cref="GetResourceManager"/>
		/// </summary>
		private ResourceManager resourceManager { get; set; }

		/// <summary>
		/// Retrieves the <see cref="System.Resources.ResourceManager"/> reference from
		/// <see cref="ResourceClass"/>.
		/// </summary>
		/// <returns>The resource manager instance.</returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="ResourceClass"/> does not map to a valid resource file, or is null.</exception>
		protected virtual ResourceManager GetResourceManager()
		{
			// Sanity.
			if (null == this.ResourceClass)
				throw new InvalidOperationException("Cannot retrieve resource manager from null type.");

			// Validate that we have a property.
			var property = this.ResourceClass
				.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
				.FirstOrDefault(p => p.Name == "ResourceManager");
			if (null == property)
			{
				this.Logger.Fatal($"The resource manager property could not be found on {this.ResourceClass.FullName}; is it a resource file?");
				throw new InvalidOperationException
				(
					$"The resource manager property could not be found on {this.ResourceClass.FullName}; is it a resource file?"
				);
			}

			// Valdiate that we can read it.
			if (false == property.CanRead)
			{
				this.Logger.Fatal($"The resource manager property on {this.ResourceClass.FullName} cannot be read; is it a resource file?");
				throw new InvalidOperationException
				(
					$"The resource manager property on {this.ResourceClass.FullName} cannot be read; is it a resource file?"
				);
			}

			// Invoke and return.
			try
			{
				var resourceManager = property.GetValue(null) as ResourceManager;
				if (null == resourceManager)
				{
					this.Logger.Fatal($"The resource manager property on {this.ResourceClass.FullName} returned a null value.");
					throw new InvalidOperationException
					(
						$"The resource manager property on {this.ResourceClass.FullName} returned a null value."
					);
				}
				return resourceManager; // Null as static.
			}
			catch (Exception e)
			{
				this.Logger.Fatal(e, $"Could not read the resource manager property on {this.ResourceClass.FullName}; is the property static?");
				throw;
			}
		}

		/// <inheritdoc />
		public override ResourceManager ResourceManager
		{
			get
			{
				// Use the cached one if we have it, otherwise use the method to get it.
				var resourceManager = this.resourceManager
					?? this.GetResourceManager();

				// Update the cache and return.
				this.resourceManager = resourceManager;
				return resourceManager;
			}
		}

	}

}
