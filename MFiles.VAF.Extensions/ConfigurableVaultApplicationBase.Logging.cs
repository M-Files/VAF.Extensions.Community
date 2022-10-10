using MFiles.VAF.Configuration;
using MFiles.VaultApplications.Logging;
using MFiles.VaultApplications.Logging.Configuration;
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
			var loggingConfiguration = this.GetLoggingConfiguration();
			if (loggingConfiguration != null)
			{
				LogManager.Initialize(vault, loggingConfiguration);
				this.Logger?.Debug("Logging started");
			}
		}

		protected virtual ILoggingConfiguration GetLoggingConfiguration()
		{
			if (this.Configuration is Configuration.IConfigurationWithLoggingConfiguration configurationWithLogging)
			{
				return configurationWithLogging?.GetLoggingConfiguration();
			}

			return null;
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
			var loggingConfiguration = this.GetLoggingConfiguration();
			if (loggingConfiguration != null)
			{
				foreach (var finding in loggingConfiguration.GetValidationFindings() ?? new ValidationFinding[0])
					yield return finding;
			}
		}
	}
}
