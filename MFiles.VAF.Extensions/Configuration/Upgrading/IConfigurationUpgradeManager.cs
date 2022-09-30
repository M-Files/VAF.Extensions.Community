using MFilesAPI;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
	public interface IConfigurationUpgradeManager
	{
		/// <summary>
		/// Upgrades the configuration in the vault.
		/// </summary>
		/// <param name="vault">The vault reference to use to access named-value storage.</param>
		void UpgradeConfiguration<TSecureConfiguration>(Vault vault)
            where TSecureConfiguration : class, new();

	}
}
