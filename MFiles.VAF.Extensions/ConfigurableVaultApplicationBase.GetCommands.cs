using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		/// <inheritdoc />
		public override IEnumerable<CustomDomainCommand> GetCommands(IConfigurationRequestContext context)
		{
			// Return the base commands, if any.
			foreach (var c in base.GetCommands(context)?.AsNotNull())
				yield return c;

			// Return any commands that the resolver provides.
			{
				var resolver = this.GetCustomDomainCommandResolver();
				if (resolver != null)
				{
					foreach (var c in resolver.GetCustomDomainCommands()?.AsNotNull())
						yield return c;
				}
			}
		}

		/// <summary>
		/// Returns an object - or <see langword="null"/> - that searches known object types
		/// to find methods decorated with <see cref="CustomCommandAttribute"/>.
		/// </summary>
		/// <returns>The resolver, or <see langword="null"/> if none is configured.</returns>
		/// <remarks>Returns <see cref="DefaultCustomDomainCommandResolver"/> by default.</remarks>
		public virtual ICustomDomainCommandResolver GetCustomDomainCommandResolver()
		{
			return new DefaultCustomDomainCommandResolver<TSecureConfiguration>(this);
		}
	}
}
