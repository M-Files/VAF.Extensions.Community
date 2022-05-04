using MFiles.VAF.Configuration;
using MFiles.VaultApplications.Logging;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		/// <summary>
		/// The logger for the vault application class.
		/// </summary>
		public ILogger Logger { get; private set; }

		protected override void InitializeApplication(Vault vault)
		{
			base.InitializeApplication(vault);

			// If we have logging configuration then initialize with that.
			if (this.Configuration is Configuration.IConfigurationWithLoggingConfiguration configurationWithLogging)
			{
				LogManager.Initialize(vault, configurationWithLogging?.GetLoggingConfiguration());
				this.Logger?.Debug("Logging started");
			}
		}

		/// <inheritdoc />
		protected override void StartApplication()
		{
			base.StartApplication();

#if DEBUG
			// If we are debugging then populate the cache of referenced assemblies.

			// Populate the referenced assemblies (do it via a task though to allow application to start).
			Task.Run(this.PopulateReferencedAssemblies);
#endif

		}

		/// <inheritdoc />
		protected override void UninitializeApplication(Vault vault)
		{
			// If we have a logger then write out that we're stopping.
			this.Logger?.Debug("Logging stopping");
			LogManager.Shutdown();
			base.UninitializeApplication(vault);
		}

		/// <inheritdoc />
		protected override IEnumerable<ValidationFinding> CustomValidation(Vault vault, TSecureConfiguration config)
		{
			foreach (var finding in base.CustomValidation(vault, config) ?? new ValidationFinding[0])
				yield return finding;

			// If we have logging configuration then use that.
			if (this.Configuration is Configuration.IConfigurationWithLoggingConfiguration configurationWithLogging)
			{
				foreach (var finding in configurationWithLogging?.GetLoggingConfiguration()?.GetValidationFindings() ?? new ValidationFinding[0])
					yield return finding;
			}
		}
	}
}
