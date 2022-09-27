using MFiles.VAF.Extensions.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{

		protected IConfigurationUpgradeManager ConfigurationUpgradeManager { get; private set; }
		public virtual IConfigurationUpgradeManager GetConfigurationUpgradeManager()
		{
			return new ConfigurationUpgradeManager<TSecureConfiguration>(this);
		}

		/// <inheritdoc />
		/// <remarks>Will call <see cref="UpgradeConfiguration"/> then call the base implementation.</remarks>
		protected override void PopulateConfigurationObjects(Vault vault)
		{
			// Create the configuration upgrade manager if needed.
			this.ConfigurationUpgradeManager = this.ConfigurationUpgradeManager
				?? this.GetConfigurationUpgradeManager();

			// Run any configuration upgrade rules.
			this.ConfigurationUpgradeManager?.UpgradeConfiguration(vault);

			// Use the base implementation.
			base.PopulateConfigurationObjects(vault);
		}
	}
}
