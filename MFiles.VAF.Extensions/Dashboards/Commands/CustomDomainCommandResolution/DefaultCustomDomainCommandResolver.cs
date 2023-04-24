using MFiles.VAF.AppTasks;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution
{
	public class DefaultCustomDomainCommandResolver<TSecureConfiguration>
		: AggregatedCustomDomainCommandResolver
		where TSecureConfiguration : class, new()
	{

		/// <summary>
		/// The vault application that this resolver is running within.
		/// </summary>
		protected ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; }

		/// <summary>
		/// Creates an instance of <see cref="DefaultCustomDomainCommandResolver{TSecureConfiguration}"/>
		/// and includes the provided <paramref name="vaultApplication"/> in the list of things to resolve against.
		/// </summary>
		/// <param name="vaultApplication">The vault application this is running within.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="vaultApplication"/> is <see langword="null"/>.</exception>
		public DefaultCustomDomainCommandResolver(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
			: base()
		{
			this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));

			foreach(var r in this.GetDefaultCustomDomainCommandResolvers()?.AsNotNull())
				if (null != r)
					this.CustomDomainCommandResolvers.Add(r);
		}

		/// <summary>
		/// Retrieves the custom domain resolvers that should be used by default.
		/// </summary>
		/// <returns>The custom domain command resolvers.</returns>
		public virtual IEnumerable<ICustomDomainCommandResolver> GetDefaultCustomDomainCommandResolvers()
		{
			yield return new AsynchronousOperationCustomDomainCommandResolver<TSecureConfiguration>(this.VaultApplication);
			yield return new LogCustomDomainCommandResolver<TSecureConfiguration>(this.VaultApplication);
			yield return new AttributeCustomDomainCommandResolver(this.VaultApplication);
			yield return new ImportPackageCustomDomainCommandResolver<TSecureConfiguration>(this.VaultApplication);
		}

	}
}
