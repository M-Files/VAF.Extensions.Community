using MFiles.VAF.Configuration;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// Defines a rule that can be run (<see cref="Execute(Vault)"/>)
	/// to somehow upgrade the configuration.
	/// </summary>
	public interface IUpgradeRule
	{
		/// <summary>
		/// Returns whether the rules are correctly configured to allow execution.
		/// </summary>
		/// <returns><see langword="true"/> if execution can be attempted.</returns>
		bool IsValid();

		/// <summary>
		/// Runs this rule against the provided <paramref name="vault"/>.
		/// </summary>
		/// <param name="vault">The vault to run the code against.</param>
		/// <returns><see langword="true"/> if the rule ran successfully, <see langword="false"/> if the rule chose not to run (e.g. there was nothing to migrate).</returns>
		/// <remarks>May throw exceptions.</remarks>
		bool Execute(Vault vault);

		/// <summary>
		/// The version that this rule migrates from.
		/// If the version in the configuration is higher than this then this rule will be skipped.
		/// </summary>
		Version MigrateFromVersion { get; }

		/// <summary>
		/// The version of the configuration after this migration has completed.
		/// </summary>
		Version MigrateToVersion { get; }

		/// <summary>
		/// The converter to serialize/deserialize JSON.
		/// </summary>
		IJsonConvert JsonConvert { get; set; }
	}
}
