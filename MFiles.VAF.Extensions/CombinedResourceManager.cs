// ReSharper disable once CheckNamespace
using System.Collections.Generic;
using System.Resources;
using System.Globalization;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A resource manager that can retrieve resources from a set of files.
	/// </summary>
	public class CombinedResourceManager
		: ResourceManager
	{
		/// <summary>
		/// The resource managers that provide resources for this application.
		/// </summary>
		public List<ResourceManager> ResourceManagers { get; } = new List<ResourceManager>();
		public CombinedResourceManager() { }
		public CombinedResourceManager(params ResourceManager[] resourceManagers)
			: this()
		{
			if (null == resourceManagers)
				return;

			// Add the resource managers.
			foreach(var resourceManager in resourceManagers)
				if(null != resourceManager)
					this.ResourceManagers.Add(resourceManager);
		}

		/// <inheritdoc />
		/// <remarks>Iterates through <see cref="ResourceManagers"/> in order to find one that contains a value for the supplied <paramref name="name"/>.</remarks>
		public override string GetString(string name, CultureInfo culture)
		{
			culture = culture ?? System.Globalization.CultureInfo.CurrentUICulture;
			foreach (var manager in this.ResourceManagers)
			{
				if (null == manager)
					continue;
				try
				{
					var value = manager.GetString(name, culture);
					if (null != value)
						return value;
				}
				catch { }
			}
			return null;
		}

		/// <inheritdoc />
		/// <remarks>Iterates through <see cref="ResourceManagers"/> in order to find one that contains a value for the supplied <paramref name="name"/>.</remarks>
		public override string GetString(string name) => this.GetString(name, null);

		/// <inheritdoc />
		/// <remarks>Iterates through <see cref="ResourceManagers"/> in order to find one that contains a value for the supplied <paramref name="name"/>.</remarks>
		public override object GetObject(string name, CultureInfo culture)
		{
			culture = culture ?? System.Globalization.CultureInfo.CurrentUICulture;
			foreach (var manager in this.ResourceManagers)
			{
				if (null == manager)
					continue;
				try
				{
					var value = manager.GetObject(name, culture);
					if (null != value)
						return value;
				}
				catch { }
			}
			return null;
		}

		/// <inheritdoc />
		/// <remarks>Iterates through <see cref="ResourceManagers"/> in order to find one that contains a value for the supplied <paramref name="name"/>.</remarks>
		public override object GetObject(string name) => this.GetObject(name, null);
	}
}
