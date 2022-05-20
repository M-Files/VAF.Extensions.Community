using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFiles.VaultApplications.Logging;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		void UpgradeConfiguration(Vault vault);

	}
	internal class ConfigurationUpgradeManager<TConfiguration>
		: IConfigurationUpgradeManager
		where TConfiguration : class, new()
	{
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ConfigurationUpgradeManager<TConfiguration>));

		/// <summary>
		/// The NamedValueStorageManager used to interact with named value storage.
		/// </summary>
		/// <remarks>Typically an instance of <see cref="NamedValueStorageManager"/>.</remarks>
		public INamedValueStorageManager NamedValueStorageManager { get; set; } = new VaultNamedValueStorageManager();

		/// <summary>
		/// The vault application that instantiated this upgrade manager.
		/// </summary>
		protected VaultApplicationBase VaultApplication { get; set; } 

		public ConfigurationUpgradeManager(VaultApplicationBase vaultApplication)
		{
			this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		/// <inheritdoc />
		public void UpgradeConfiguration(Vault vault)
		{
			try
			{
				// Run any configuration upgrade rules.
				foreach (var rule in this.GetConfigurationUpgradePath(vault) ?? Enumerable.Empty<IUpgradeRule>())
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
			catch (Exception e)
			{
				this.Logger?.Fatal(e, $"Could not get configuration upgrade rules.");
			}
		}

		/// <summary>
		/// Gets any rules that should be applied to the configuration.
		/// These rules may define a change in configuration location (e.g. to migrate older configuration to a newer location),
		/// or define how to map older configuration structures across to new.
		/// Rules will be executed in the order that they appear in the list.
		/// Rules are run when the configuration is loaded.
		/// </summary>
		/// <param name="vault">The location in NVS to load the configuration from.</param>
		/// <returns>
		/// The rules to run.
		/// </returns>
		/// <remarks>The default implementation uses [ConfigurationUpgradeMethod] attributes to design an upgrade path.</remarks>
		protected internal virtual IEnumerable<IUpgradeRule> GetConfigurationUpgradePath(Vault vault)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));

			// Get all the declared configuration upgrades.
			if (false == this.TryGetDeclaredConfigurationUpgrades(out Version configurationVersion, out IEnumerable<DeclaredConfigurationUpgradeRule> declaredUpgradeRules))
			{
				this.Logger?.Debug($"No upgrade rules were returned, so no configuration upgrade attempted.");
				yield break; // Could not find any, so die.
			}

			// Try to find an upgrade process.
			if (false == this.TryGetConfigurationUpgradePath(vault, declaredUpgradeRules, out Stack<DeclaredConfigurationUpgradeRule> upgradePath))
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
			Vault vault, 
			IEnumerable<DeclaredConfigurationUpgradeRule> upgradeRules,
			out Stack<DeclaredConfigurationUpgradeRule> upgradePath
		)
		{
			upgradePath = new Stack<DeclaredConfigurationUpgradeRule>();

			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == upgradeRules || false == upgradeRules.Any())
				return true; // No upgrade rules, don't attempt to do anything.

			// Go from highest version to lowest version; we only want the changes that are needed.
			foreach(var rule in upgradeRules.OrderByDescending(r => r.MigrateToVersion))
			{
				// Try and read the data from the "write to" location.
				if (false == rule.TryRead(rule.WriteTo ?? rule.ReadFrom, vault, out string data, out Version version))
				{
					// This rule needs to be run.
					upgradePath.Push(rule);
				}
				else
				{
					// This rule needs to be run if the version is not correct.
					version = version ?? new Version("0.0");
					if (version < rule.MigrateToVersion)
					{
						upgradePath.Push(rule);
					}
				}

				// This rule DOES NOT need to run, so stop.
				break;
			}

			// Okay, we can run (note: may be no rules to run, but that's fine).
			return true;
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
			if (currentVersion.Equals(targetVersion))
			{
				this.Logger?.Debug($"Configuration is already at {targetVersion}; skipping upgrade.");
				return true;
			}
			if (rules.Count == 0)
				return false;
			if (false == rules.Any(r => r.MigrateFromVersion == currentVersion))
			{
				this.Logger?.Warn($"There are no rules from {currentVersion}; skipping upgrade.");
				return false; // No rules from this version.
			}
			if (false == rules.Any(r => r.MigrateToVersion == targetVersion))
			{
				this.Logger?.Warn($"There are no rules to {targetVersion}; skipping upgrade.");
				return false; // No rules to the target version.
			}
			if (currentVersion > targetVersion)
			{
				this.Logger?.Warn($"Configuration version {currentVersion} is higher than expected target version {targetVersion}.");
				return false;
			}

			// If there's a direct rule then return that.
			{
				var directRule = rules.FirstOrDefault(r => r.MigrateFromVersion == currentVersion && r.MigrateToVersion == targetVersion);
				if (directRule != null)
				{
					upgradePath.Push(directRule);
					return true;
				}
			}

			// Remove rules that we don't care about.
			rules.RemoveAll(r => r.MigrateToVersion > targetVersion || r.MigrateFromVersion < currentVersion);

			// This is the version that we are looking for a rule for; it'll be overwritten as we progress.
			var currentTargetVersion = targetVersion;

			// Declare our function that'll do the search.
			IEnumerable<DeclaredConfigurationUpgradeRule> findUpgradePath(Version upgradeFrom, Version upgradeTo)
			{
				// Find each rule that will go "to" the target, regardless of the minimum.
				foreach (var targetRule in rules.Where(r => r.MigrateToVersion == upgradeTo))
				{
					// Is it the one we want?
					if (targetRule.MigrateFromVersion == upgradeFrom)
					{
						yield return targetRule;
						yield break;
					}

					// Find any rules that'll go from where we are to the new minimum.
					var p = findUpgradePath(upgradeFrom, targetRule.MigrateFromVersion).ToList();

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
		) => this.TryGetDeclaredConfigurationUpgrades(typeof(TConfiguration), out configurationVersion, out upgradeRules);

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
			if (this.CheckedConfigurationUpgradeTypes.Contains(configuration))
			{
				this.Logger?.Warn($"Configuration type provided to {nameof(TryGetDeclaredConfigurationUpgrades)} was already checked.  This could indicate a cyclic configuration upgrade definition.  Skipping.");
				return false;
			}
			this.CheckedConfigurationUpgradeTypes.Add(configuration);
			this.Logger?.Trace($"Adding {configuration} to the list of types checked.");

			// Does this type have a version?
			var configurationVersionAttribute = configuration
				.GetCustomAttributes(false)?
				.Where(a => a is Configuration.ConfigurationVersionAttribute)
				.Cast<Configuration.ConfigurationVersionAttribute>()
				.FirstOrDefault();
			configurationVersion = configurationVersionAttribute?.Version ?? VersionZero;

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
				if (method.IsStatic && method.ReturnType != configuration)
				{
					// Return type is incorrect.
					this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as the return type was not {configuration.FullName}.");
					continue;
				}

				// If it's not static then it should return void.
				if (!method.IsStatic && method.ReturnType != typeof(void))
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
				var oldTypeConfigurationVersionAttribute = oldType
					.GetCustomAttributes(false)?
					.Where(a => a is Configuration.ConfigurationVersionAttribute)
					.Cast<Configuration.ConfigurationVersionAttribute>()
					.FirstOrDefault();
				var oldTypeVersion = oldTypeConfigurationVersionAttribute?.Version ?? VersionZero;

				// The target version must be greater than the version we're upgrading from!.
				if (oldTypeVersion >= configurationVersion)
				{
					// Should take a single parameter which is the type of the older version.
					this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as it defines an upgrade from {oldTypeVersion} to {configurationVersion} (target configuration version must be higher).");
					continue;
				}

				// Where should we read from? (this comes from the oldest configuration attribute.
				var readFrom = (oldTypeConfigurationVersionAttribute?.UsesCustomNVSLocation ?? false)
					? new SingleNamedValueItem(oldTypeConfigurationVersionAttribute.NamedValueType, oldTypeConfigurationVersionAttribute.Namespace, oldTypeConfigurationVersionAttribute.Key)
					: SingleNamedValueItem.ForLatestVAFVersion(this.VaultApplication);

				// Where should we write to? (this comes from the newest configuration attribute)
				var writeTo = (configurationVersionAttribute?.UsesCustomNVSLocation ?? false)
					? new SingleNamedValueItem(configurationVersionAttribute.NamedValueType, configurationVersionAttribute.Namespace, configurationVersionAttribute.Key)
					: SingleNamedValueItem.ForLatestVAFVersion(this.VaultApplication);

				// This method is okay.
				identifiedUpgradeMethods.Add
				(
					new DeclaredConfigurationUpgradeRule(readFrom, writeTo, oldTypeVersion, configurationVersion, method)
					{
						UpgradeToType = configuration,
						UpgradeFromType = oldType,
						NamedValueStorageManager = this.NamedValueStorageManager
					}
				);
				this.Logger?.Debug($"Adding {configuration.FullName}.{method.Name} as an upgrade path from {oldTypeVersion} to {configurationVersion}");

				// Add in upgrade methods on the related type, if we can.
				{
					if (this.TryGetDeclaredConfigurationUpgrades
					(
						oldType,
						out Version v,
						out IEnumerable<DeclaredConfigurationUpgradeRule> mi
					))
						identifiedUpgradeMethods.AddRange(mi);
				}
			}

			// Copy out the ones we found.
			upgradeRules = identifiedUpgradeMethods.OrderBy(m => m.MigrateToVersion);

			// We have something?!
			this.Logger?.Trace($"{upgradeRules.Count()} upgrade rules identified in total.");
			return upgradeRules.Any();
		}
	}
}
