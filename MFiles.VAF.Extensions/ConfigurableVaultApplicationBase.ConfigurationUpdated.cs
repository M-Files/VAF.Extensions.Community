using MFilesAPI;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A base class for vault applications that use the VAF Extensions library.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	/// <remarks>See https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes for further details.</remarks>
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{
		/// <inheritdoc />
		protected override void OnConfigurationUpdated(TSecureConfiguration oldConfiguration, bool updateExternals)
		{
			// Base implementation is empty, but good practice to call it.
			base.OnConfigurationUpdated(oldConfiguration, updateExternals);

			// Populate the task processing schedule configuration.
			this.RecurringOperationConfigurationManager?.PopulateFromConfiguration(this.Configuration);
		}

		/// <inheritdoc />
		public override void StartOperations(Vault vaultPersistent)
		{
			// Initialise the application.
			base.StartOperations(vaultPersistent);

			// Ensure that our recurring configuration is updated.
			this.RecurringOperationConfigurationManager?.PopulateFromConfiguration(this.Configuration);
		}
	}
}
