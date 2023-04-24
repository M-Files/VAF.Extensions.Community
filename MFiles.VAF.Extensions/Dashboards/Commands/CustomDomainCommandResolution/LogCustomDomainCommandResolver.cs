using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution
{
	public class LogCustomDomainCommandResolver<TSecureConfiguration>
		: CustomDomainCommandResolverBase
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
		public LogCustomDomainCommandResolver(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
			: base()
		{
			VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		public override IEnumerable<CustomDomainCommand> GetCustomDomainCommands()
		{

			// Return the commands associated with downloading logs from the default file target.
			foreach (var c in GetDefaultLogTargetDownloadCommands()?.AsNotNull())
				yield return c;
		}

		/// <summary>
		/// Returns the commands associated with downloading logs from the default file target.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<CustomDomainCommand> GetDefaultLogTargetDownloadCommands()
		{
			// One to allow them to select which logs...
			yield return ShowSelectLogDownloadDashboardCommand.Create();

			// ...and one that actually does the collation/download.
			yield return DownloadSelectedLogsDashboardCommand.Create();

			// Allow the user to see the latest log entries.
			yield return ShowLatestLogEntriesDashboardCommand.Create();
			yield return RetrieveLatestLogEntriesCommand.Create();
		}
	}
}
