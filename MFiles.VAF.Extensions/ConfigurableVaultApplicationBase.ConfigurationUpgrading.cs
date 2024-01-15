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
		/// <summary>
		/// The configuration upgrade manager.
		/// May be null before <see cref="PopulateConfigurationObjects(Vault)"/> is called.
		/// </summary>
		/// <remarks>Populated with the result of calling <see cref="GetConfigurationUpgradeManager"/>.</remarks>
		protected IConfigurationUpgradeManager ConfigurationUpgradeManager { get; private set; }

		/// <summary>
		/// Returns the instance of <see cref="IConfigurationUpgradeManager"/> that will be used
		/// to upgrade any configuration found.
		/// </summary>
		/// <returns>
		/// <see langword="null"/> by default.  
		/// Return an instance of something that inherits <see cref="IConfigurationUpgradeManager"/>,
		/// for example <see cref="DefaultConfigurationUpgradeManager"/>, to control configuration upgrading.
		/// </returns>
		public virtual IConfigurationUpgradeManager GetConfigurationUpgradeManager()
			=> null;

		/// <inheritdoc />
		/// <remarks>Will call <see cref="UpgradeConfiguration"/> then call the base implementation.</remarks>
		protected override void PopulateConfigurationObjects(Vault vault)
		{
			// Create the configuration upgrade manager if needed.
			this.ConfigurationUpgradeManager = this.ConfigurationUpgradeManager
				?? this.GetConfigurationUpgradeManager();

			// Run any configuration upgrade rules.
			this.ConfigurationUpgradeManager?.UpgradeConfiguration<TSecureConfiguration>(vault);

			// Use the base implementation.
			base.PopulateConfigurationObjects(vault);
		}
	}
}
