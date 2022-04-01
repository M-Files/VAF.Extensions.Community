using MFilesAPI;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// Defines a rule that can be run (<see cref="Execute(Vault)"/>)
	/// to somehow upgrade the configuration.
	/// </summary>
	public interface IUpgradeRule
	{
		/// <summary>
		/// Runs this rule against the provided <paramref name="vault"/>.
		/// </summary>
		/// <param name="vault">The vault to run the code against.</param>
		/// <returns><see langword="true"/> if the rule ran successfully, <see langword="false"/> if the rule chose not to run (e.g. there was nothing to migrate).</returns>
		/// <remarks>May throw exceptions.</remarks>
		bool Execute(Vault vault);
	}
}
