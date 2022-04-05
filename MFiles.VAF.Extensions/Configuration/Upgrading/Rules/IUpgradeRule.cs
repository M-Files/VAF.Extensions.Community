using MFiles.VaultApplications.Logging;
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
		/// Runs this rule against the provided <paramref name="vault"/>.
		/// </summary>
		/// <param name="vault">The vault to run the code against.</param>
		/// <returns><see langword="true"/> if the rule ran successfully, <see langword="false"/> if the rule chose not to run (e.g. there was nothing to migrate).</returns>
		/// <remarks>May throw exceptions.</remarks>
		bool Execute(Vault vault);
	}
	public abstract class UpgradeRuleBase<TOptions>
		: IUpgradeRule
		where TOptions : class, IUpgradeRuleOptions
	{
		/// <summary>
		/// The logger for this class.
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// Options for the rule.
		/// </summary>
		protected internal TOptions Options { get; }

		public UpgradeRuleBase(TOptions options)
		{
			// Sanity.
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
			this.Logger = LogManager.GetLogger(this.GetType());
		}

		/// <inheritdoc />
		public abstract bool Execute(Vault vault);
	}
	public interface IUpgradeRuleOptions
	{
		/// <summary>
		/// Returns whether the rules are correctly configured to allow execution.
		/// </summary>
		/// <returns><see langword="true"/> if execution can be attempted.</returns>
		bool IsValid();
	}
	public abstract class UpgradeRuleOptionsBase
		: IUpgradeRuleOptions
	{
		/// <inheritdoc />
		public abstract bool IsValid();
	}
}
