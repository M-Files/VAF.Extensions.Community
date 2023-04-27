using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution
{
	public class AsynchronousOperationCustomDomainCommandResolver<TSecureConfiguration>
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
		public AsynchronousOperationCustomDomainCommandResolver(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
			: base()
		{
			VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		/// <inheritdoc />
		public override IEnumerable<CustomDomainCommand> GetCustomDomainCommands()
		{
			// Return the command related to the background operation approach.
			foreach (var c in GetTaskQueueBackgroundOperationRunCommands()?.AsNotNull())
				yield return c;

			// Return the commands related to the VAF 2.3+ attribute-based approach.
			foreach (var c in GetTaskQueueRunCommands()?.AsNotNull())
				yield return c;
		}

		/// <summary>
		/// Returns the commands associated with manually running task queue background operations.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<CustomDomainCommand> GetTaskQueueBackgroundOperationRunCommands()
		{
			// Sanity.
			if (null == VaultApplication)
				yield break;

			// Get the background operations that have a run command.
			// Note: this should be all of them.
			foreach (var c in
				GetType()
				.GetPropertiesAndFieldsOfType<TaskQueueBackgroundOperationManager<TSecureConfiguration>>(VaultApplication)
				.SelectMany(tqbom => tqbom.BackgroundOperations)
				.AsEnumerable()
				.Select(bo => bo.Value?.DashboardRunCommand)
				.Where(c => null != c))
				yield return c;
		}

		/// <summary>
		/// Returns the commands associated with manually running task queue background operations.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<CustomDomainCommand> GetTaskQueueRunCommands()
		{
			// Sanity.
			if (null == VaultApplication?.TaskManager)
				yield break;
			if (null == VaultApplication?.TaskQueueResolver)
				yield break;

			// Return the commands related to the VAF 2.3+ attribute-based approach.
			foreach (var c in VaultApplication?.TaskManager?.GetTaskQueueRunCommands(VaultApplication.TaskQueueResolver)?.AsNotNull())
				yield return c;
		}
	}
}
