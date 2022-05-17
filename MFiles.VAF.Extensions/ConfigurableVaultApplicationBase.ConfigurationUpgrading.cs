using MFiles.VAF.Extensions.Configuration;
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

		/// <inheritdoc />
		/// <remarks>Will run any <see cref="GetConfigurationUpgradeRules"/> at this point.</remarks>
		protected override void PopulateConfigurationObjects(Vault vault)
		{
			// Run any configuration upgrade rules.
			var rules = this.GetConfigurationUpgradeRules(vault);
			if (null != rules)
			{
				foreach (var rule in rules)
				{
					try
					{
						rule?.Execute(vault);
					}
					catch (Exception ex)
					{
						this.Logger?.Error(ex, $"Could not execute configuration migration rule of type {rule?.GetType()?.FullName}");
					}
				}
			}

			// Use the base implementation.
			base.PopulateConfigurationObjects(vault);
		}

		/// <summary>
		/// Gets any rules that should be applied to the configuration.
		/// These rules may define a change in configuration location (e.g. to migrate older configuration to a newer location),
		/// or define how to map older configuration structures across to new.
		/// Rules will be executed in the order that they appear in the list.
		/// Rules are run when the configuration is loaded.
		/// </summary>
		/// <returns>
		/// The rules to run.
		/// </returns>
		/// <remarks>The default implementation uses [ConfigurationUpgradeMethod] attributes to design an upgrade path.</remarks>
		protected internal virtual IEnumerable<Configuration.Upgrading.Rules.IUpgradeRule> GetConfigurationUpgradeRules(Vault vault)
			=> this.GetConfigurationUpgradeRules(vault, UpgradeSingleNVSLocationRuleOptions.ForLatestLocation(this));

		/// <summary>
		/// Gets any rules that should be applied to the configuration.
		/// These rules may define a change in configuration location (e.g. to migrate older configuration to a newer location),
		/// or define how to map older configuration structures across to new.
		/// Rules will be executed in the order that they appear in the list.
		/// Rules are run when the configuration is loaded.
		/// </summary>
		/// <param name="options">The location in NVS to load the configuration from.</param>
		/// <returns>
		/// The rules to run.
		/// </returns>
		/// <remarks>The default implementation uses [ConfigurationUpgradeMethod] attributes to design an upgrade path.</remarks>
		protected internal virtual IEnumerable<IUpgradeRule> GetConfigurationUpgradeRules
		(
			Vault vault,
			UpgradeSingleNVSLocationRuleOptions options,
			VAF.Configuration.IConfigurationStorage configurationStorage = null
		)
		{
			// Sanity.
			if (null == options)
				throw new ArgumentNullException(nameof(options));
			configurationStorage = configurationStorage ?? this.ConfigurationStorage;

			// Try and get the data from NVS.
			if (false == configurationStorage.ReadConfigurationData(vault, options.Source.Namespace, options.Source.Name, out string configurationData))
			{
				this.Logger?.Debug($"No configuration stored in {options.Source}");
				return Enumerable.Empty<IUpgradeRule>();
			}

			// Try and get the version.
			var currentVersion = configurationStorage.Deserialize<VersionedConfigurationBase>(configurationData)?.Version ?? VersionZero;

			// Use the other overload.
			return this.GetConfigurationUpgradeRules(currentVersion);

		}

		/// <summary>
		/// Gets any rules that should be applied to the configuration.
		/// These rules may define a change in configuration location (e.g. to migrate older configuration to a newer location),
		/// or define how to map older configuration structures across to new.
		/// Rules will be executed in the order that they appear in the list.
		/// Rules are run when the configuration is loaded.
		/// </summary>
		/// <param name="options">The location in NVS to load the configuration from.</param>
		/// <returns>
		/// The rules to run.
		/// </returns>
		/// <remarks>The default implementation uses [ConfigurationUpgradeMethod] attributes to design an upgrade path.</remarks>
		protected internal virtual IEnumerable<IUpgradeRule> GetConfigurationUpgradeRules(Version currentVersion)
		{
			// Sanity.
			if (null == currentVersion)
				throw new ArgumentNullException(nameof(currentVersion));

			// Get all the declared configuration upgrades.
			if (false == this.TryGetDeclaredConfigurationUpgrades(out Version configurationVersion, out IEnumerable<DeclaredConfigurationUpgradeRule> upgradeRules))
			{
				this.Logger?.Debug($"No upgrade rules were returned, so no configuration upgrade attempted.");
				yield break; // Could not find any, so die.
			}

			// Try to find an upgrade process.
			if(false == this.TryGetConfigurationUpgradePath(upgradeRules.ToList(), currentVersion, configurationVersion, out Stack<DeclaredConfigurationUpgradeRule> upgradePath))
			{
				this.Logger?.Warn($"Could not find an upgrade path to {configurationVersion}; configuration upgrade failure.");
				yield break;
			}

			// Return the associated upgrade rules.
			foreach (var rule in upgradePath)
				yield return rule;
		}

		internal bool TryGetConfigurationUpgradePath
		(
			List<DeclaredConfigurationUpgradeRule> rules, 
			Version currentVersion, 
			Version targetVersion, 
			out Stack<DeclaredConfigurationUpgradeRule> upgradePath
		)
		{
			upgradePath = new Stack<DeclaredConfigurationUpgradeRule>();

			// Sanity.
			if (null == rules)
				throw new ArgumentNullException(nameof(rules));
			if (null == currentVersion)
				throw new ArgumentNullException(nameof(currentVersion));
			if (null == targetVersion)
				throw new ArgumentNullException(nameof(targetVersion));
			if (rules.Count == 0)
				return false;
			if (false == rules.Any(r => r.MigrateFrom == currentVersion))
				return false; // No rules from this version.
			if (false == rules.Any(r => r.MigrateTo == targetVersion))
				return false; // No rules to the target version.
			if(currentVersion > targetVersion)
			{
				this.Logger?.Warn($"Configuration version {currentVersion} is higher than expected target version {targetVersion}.");
				return false;
			}

			// Already at the right version?  Die.
			if (currentVersion == targetVersion)
			{
				this.Logger?.Debug($"Configuration is already at {targetVersion}; skipping upgrade.");
				return true;
			}

			// If there's a direct rule then return that.
			{
				var directRule = rules.FirstOrDefault(r => r.MigrateFrom == currentVersion && r.MigrateTo == targetVersion);
				if(directRule != null)
				{
					upgradePath.Push(directRule);
					return true;
				}
			}

			// Remove rules that we don't care about.
			rules.RemoveAll(r => r.MigrateTo > targetVersion || r.MigrateFrom < currentVersion);

			// This is the version that we are looking for a rule for; it'll be overwritten as we progress.
			var currentTargetVersion = targetVersion;

			// Declare our function that'll do the search.
			IEnumerable<DeclaredConfigurationUpgradeRule> findUpgradePath(Version upgradeFrom, Version upgradeTo)
			{
				// Find each rule that will go "to" the target, regardless of the minimum.
				foreach(var targetRule in rules.Where(r => r.MigrateTo == upgradeTo))
				{
					// Is it the one we want?
					if (targetRule.MigrateFrom == upgradeFrom)
					{
						yield return targetRule;
						yield break;
					}

					// Find any rules that'll go from where we are to the new minimum.
					var p = findUpgradePath(upgradeFrom, targetRule.MigrateFrom).ToList();

					// If there are no rules then skip to the next rule.
					if (false == p.Any())
						continue;

					// It worked; return the path.
					foreach (var r in p)
						yield return r;

					// Now return this rule.
					yield return targetRule;
				}

				// No valid upgrade path.
				yield break;
			}

			// Find the upgrade path.
			var path = findUpgradePath(currentVersion, targetVersion).Reverse().ToList();
			if (false == path.Any())
				return false;
			foreach (var r in path)
				upgradePath.Push(r);
			return true;

		}

		/// <summary>
		/// A representation of "no version".
		/// </summary>
		private static readonly Version VersionZero = new Version("0.0");

		/// <summary>
		/// The configuration upgrade types that have been already checked.
		/// Used to stop cyclic references.
		/// </summary>
		private readonly List<Type> CheckedConfigurationUpgradeTypes = new List<Type>();

		/// <summary>
		/// Returns any configuration upgrades that have been declared via <see cref="ConfigurationUpgradeMethodAttribute"/> attributes.
		/// </summary>
		/// <param name="configurationVersion">The version number on <see cref="configuration"/>.</param>
		/// <param name="upgradeRules">All upgrade methods declared by <see cref="configuration"/> and any referenced classes.</param>
		/// <returns>All upgrade rules that have been declared, regardless of whether they need to be run.</returns>
		protected internal virtual bool TryGetDeclaredConfigurationUpgrades
		(
			out Version configurationVersion,
			out IEnumerable<DeclaredConfigurationUpgradeRule> upgradeRules
		) => this.TryGetDeclaredConfigurationUpgrades(typeof(TSecureConfiguration), out configurationVersion, out upgradeRules);

		/// <summary>
		/// Returns any configuration upgrades that have been declared via <see cref="ConfigurationUpgradeMethodAttribute"/> attributes.
		/// </summary>
		/// <param name="configuration">The configuration to test.</param>
		/// <param name="configurationVersion">The version number on <see cref="configuration"/>.</param>
		/// <param name="upgradeRules">All upgrade methods declared by <see cref="configuration"/> and any referenced classes.</param>
		/// <returns>All upgrade rules that have been declared, regardless of whether they need to be run.</returns>
		protected internal virtual bool TryGetDeclaredConfigurationUpgrades
		(
			Type configuration,
			out Version configurationVersion,
			out IEnumerable<DeclaredConfigurationUpgradeRule> upgradeRules
		)
		{
			this.Logger?.Trace($"Attempting to retrieve configuration upgrade information for {configuration}");
			configurationVersion = VersionZero;
			upgradeRules = Enumerable.Empty<DeclaredConfigurationUpgradeRule>();

			// Sanity.
			if (null == configuration)
			{
				this.Logger?.Warn($"Configuration type provided to {nameof(TryGetDeclaredConfigurationUpgrades)} was null.  Skipping.");
				return false;
			}
			if(this.CheckedConfigurationUpgradeTypes.Contains(configuration))
			{
				this.Logger?.Warn($"Configuration type provided to {nameof(TryGetDeclaredConfigurationUpgrades)} was already checked.  This could indicate a cyclic configuration upgrade definition.  Skipping.");
				return false;
			}
			this.CheckedConfigurationUpgradeTypes.Add(configuration);
			this.Logger?.Trace($"Adding {configuration} to the list of types checked.");

			// Does this type have a version?
			configurationVersion = configuration
				.GetCustomAttributes(false)?
				.Where(a => a is Configuration.ConfigurationVersionAttribute)
				.Cast<Configuration.ConfigurationVersionAttribute>()
				.FirstOrDefault()?
				.Version ?? VersionZero;

			// If there is no version then die now.
			if (configurationVersion.Equals(VersionZero))
			{
				this.Logger?.Debug($"The configuration type {typeof(TSecureConfiguration)} does not expose a configuration version; skipping upgrade rules.");
				return false;
			}

			// Check for upgrade methods.
			var identifiedUpgradeMethods = new List<DeclaredConfigurationUpgradeRule>();
			foreach (var method in configuration.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
			{
				// Skip methods without upgrade options.
				if (null == method.GetCustomAttribute<ConfigurationUpgradeMethodAttribute>())
				{
					this.Logger?.Trace($"{configuration.FullName}.{method.Name} does not have a [ConfigurationUpgradeMethod] attribute, so not checking it as an upgrade option.");
					continue;
				}

				// If it's static then it should return the current type.
				if(method.IsStatic && method.ReturnType != configuration)
				{
					// Return type is incorrect.
					this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as the return type was not {configuration.FullName}.");
					continue;
				}

				// If it's not static then it should return void.
				if(!method.IsStatic && method.ReturnType != typeof(void))
				{
					// Return type is incorrect.
					this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as the return type was not void.");
					continue;
				}

				// The method should take one parameter.  This parameter is the type we're upgrading from.
				Type oldType = null;
				{
					var methodParameters = method.GetParameters();
					if ((methodParameters?.Length ?? 0) != 1)
					{
						// Should take a single parameter which is the type of the older version.
						this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as 1 parameter is expected (found {methodParameters.Length}).");
						continue;
					}
					if (methodParameters[0].IsOut)
					{
						// Should take a single parameter which is the type of the older version.
						this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as parameter was defined as 'out'.");
						continue;
					}
					oldType = methodParameters[0].ParameterType;
				}

				// Get the data about the old type.
				var oldTypeVersion = oldType
					.GetCustomAttributes(false)?
					.Where(a => a is Configuration.ConfigurationVersionAttribute)
					.Cast<Configuration.ConfigurationVersionAttribute>()
					.FirstOrDefault()?
					.Version ?? VersionZero;

				// The target version must be greater than the version we're upgrading from!.
				if(oldTypeVersion >= configurationVersion)
				{
					// Should take a single parameter which is the type of the older version.
					this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as it defines an upgrade from {oldTypeVersion} to {configurationVersion} (target configuration version must be higher).");
					continue;
				}

				// This method is okay.
				identifiedUpgradeMethods.Add
				(
					new DeclaredConfigurationUpgradeRule(this, oldTypeVersion, configurationVersion)
					{
						UpgradeToType = configuration,
						UpgradeFromType = oldType,
						MethodInfo = method
					}
				);
				this.Logger?.Debug($"Adding {configuration.FullName}.{method.Name} as an upgrade path from {oldTypeVersion} to {configurationVersion}");

				// Add in upgrade methods on the related type, if we can.
				{
					if(this.TryGetDeclaredConfigurationUpgrades
					(
						oldType,
						out Version v,
						out IEnumerable<DeclaredConfigurationUpgradeRule> mi
					))
						identifiedUpgradeMethods.AddRange(mi);
				}
			}

			// Copy out the ones we found.
			upgradeRules = identifiedUpgradeMethods;

			// We have something?!
			this.Logger?.Trace($"{upgradeRules.Count()} upgrade rules identified in total.");
			return upgradeRules.Any();
		}
	}
}
