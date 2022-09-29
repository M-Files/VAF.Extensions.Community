using MFiles.VAF.Extensions.Configuration.Upgrading.Rules;
using MFiles.VaultApplications.Logging;
using MFilesAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
	internal class ConfigurationUpgradeManager
		: IConfigurationUpgradeManager
	{
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ConfigurationUpgradeManager));

		/// <summary>
		/// The NamedValueStorageManager used to interact with named value storage.
		/// </summary>
		/// <remarks>Typically an instance of <see cref="NamedValueStorageManager"/>.</remarks>
		public INamedValueStorageManager NamedValueStorageManager { get; set; } = new VaultNamedValueStorageManager();

		/// <summary>
		/// The converter to serialize/deserialize JSON.
		/// </summary>
		public IJsonConvert JsonConvert { get; set; } = new NewtonsoftJsonConvert();

		/// <summary>
		/// The vault application that instantiated this upgrade manager.
		/// </summary>
		protected VaultApplicationBase VaultApplication { get; set; } 

		public ConfigurationUpgradeManager(VaultApplicationBase vaultApplication)
		{
			this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		/// <inheritdoc />
		public void UpgradeConfiguration<TSecureConfiguration>(Vault vault)
		{
			try
			{
				// Clear anything we've previously scanned.
				this.CheckedConfigurationUpgradeTypes.Clear();

				// Run any configuration upgrade rules.
				foreach (var rule in this.GetConfigurationUpgradePath<TSecureConfiguration>(vault) ?? Enumerable.Empty<IUpgradeRule>())
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

				// If the serialization has changed then update the saved data.
				{
					// Where should we read from?
					var readFrom = typeof(TSecureConfiguration).GetConfigurationLocation(this.VaultApplication);

					// Ensure the data is formatted properly.
					this.EnsureLatestSerializationSettings<TSecureConfiguration>(vault, readFrom);
				}


			}
			catch (Exception e)
			{
				this.Logger?.Fatal(e, $"Could not get configuration upgrade rules.");
			}
		}

		/// <summary>
		/// Used to ensure that the value in <paramref name="location"/> is serialised according to the current settings.
		/// For example: if a TimeSpanEx used to hold a location as a string, but now holds it as hour/minute/second properties,
		/// this method will ensure that the previous data is loaded, parsed, and saved back with the hour/minute/data properties.
		/// </summary>
		/// <typeparam name="TSecureConfiguration">The type of configuration to load.</typeparam>
		/// <param name="vault">The vault to load the data from.</param>
		/// <param name="location">The item within Named Value Storage to read/write.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="vault"/> or <paramref name="location"/> are <see langword="null"/>.</exception>
		protected virtual void EnsureLatestSerializationSettings<TSecureConfiguration>(Vault vault, ISingleNamedValueItem location)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == location)
				throw new ArgumentNullException(nameof(location));

			// Read the data.
			var existingData = this
				.NamedValueStorageManager
				.GetValue(vault, location.NamedValueType, location.Namespace, location.Name, "{}");

			// Go through a serialize/deserialize loop to check that serialization hasn't changed.
			var newData = this.JsonConvert.Serialize
			(
				this.JsonConvert.Deserialize<TSecureConfiguration>(existingData)
			);

			// If serialization has changed then force updating the new version.
			if (false == this.AreEqual(JObject.Parse(existingData), JObject.Parse(newData)))
			{
				this.NamedValueStorageManager.SetValue(vault, location.NamedValueType, location.Namespace, location.Name, newData);
			}
		}

		protected internal virtual bool AreEqual(JObject a, JObject b)
		{
			// Simple.
			if (a == null && b == null)
				return true;
			if (a == null || b == null)
				return false;

			// Compare property names.  Ignore comments.
			var aProperties = a.Properties().Select(p => p.Name).Where(n => !n.EndsWith("-Comment")).ToArray();
			var bProperties = b.Properties().Select(p => p.Name).Where(n => !n.EndsWith("-Comment")).ToArray();
			if (aProperties.Length != bProperties.Length)
				return false;

			// Check each in turn.
			foreach(var propertyName in aProperties)
			{
				// Sanity.
				var aPropertyValue = a[propertyName];
				var bPropertyValue = b[propertyName];
				if (aPropertyValue == null && bPropertyValue == null)
					return true;
				if (aPropertyValue == null || bPropertyValue == null)
					return false;
				if (a.Type != b.Type)
					return false;
				
				// Check each type.
				switch(aPropertyValue.Type)
				{
					case JTokenType.Object:
						if (false == this.AreEqual((JObject)aPropertyValue, (JObject)bPropertyValue))
							return false;
						break;
					case JTokenType.Array:
						{
							var aPropertyValueJArray = (JArray)aPropertyValue;
							var bPropertyValueJArray = (JArray)bPropertyValue;
							if (aPropertyValueJArray.Count != bPropertyValueJArray.Count)
								return false;
							// Does this need to be better?
							if (false == (aPropertyValueJArray.ToString() == bPropertyValueJArray.ToString()))
								return false;
						}
						break;
					default:
						if (false == (aPropertyValue.ToString() == bPropertyValue.ToString()))
							return false;
						break;
				}
			}

			// Everything was the same.
			return true;
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
		protected internal virtual IEnumerable<IUpgradeRule> GetConfigurationUpgradePath<TSecureConfiguration>(Vault vault)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));

			// Get all the declared configuration upgrades.
			if (false == this.TryGetDeclaredConfigurationUpgrades<TSecureConfiguration>(out Version configurationVersion, out IEnumerable<DeclaredConfigurationUpgradeRule> declaredUpgradeRules))
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

			this.Logger?.Debug($"There are {upgradePath.Count} upgrade rules to run to get the configuration from version {upgradePath.Min(r => r.MigrateFromVersion) ?? configurationVersion} to {configurationVersion}.");

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
		public static readonly Version VersionZero = new Version("0.0");

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
		protected internal virtual bool TryGetDeclaredConfigurationUpgrades<TSecureConfiguration>
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
			if (this.CheckedConfigurationUpgradeTypes.Contains(configuration))
			{
				this.Logger?.Warn($"Configuration ({configuration}) type provided to {nameof(TryGetDeclaredConfigurationUpgrades)} was already checked.  This could indicate a cyclic configuration upgrade definition.  Skipping.");
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

				// If it's not static then it should return void.
				if (!method.IsStatic && method.ReturnType != typeof(void))
				{
					// Return type is incorrect.
					this.Logger?.Error($"Skipping {configuration.FullName}.{method.Name} as the return type was not void.");
					continue;
				}

				// The method should take one parameter.  This parameter is the type we're upgrading from.
				Type parameterType = null;
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
					parameterType = methodParameters[0].ParameterType;
				}

				// If the parameter type is the same as the configuration,
				// then it's an "upgrade to" method and it RETURNS the new type.
				// So use that instead.
				if(parameterType == configuration)
				{
					parameterType = method.ReturnType;
				}

				// Get the data about the other type.
				var readFrom = parameterType.GetConfigurationLocation(this.VaultApplication, out Version parameterTypeVersion);

				// If the versions are the same then skip.
				if(parameterTypeVersion == configurationVersion)
				{
					this.Logger?.Error($"Skipping upgrade marked by {configuration.FullName}.{method.Name} as it points to a class with the same version number ({parameterTypeVersion}).");
					continue;
				}

				// Where should we write to? (this comes from the newest configuration attribute)
				var writeTo = (configurationVersionAttribute?.UsesCustomNVSLocation ?? false)
					? new SingleNamedValueItem(configurationVersionAttribute.NamedValueType, configurationVersionAttribute.Namespace, configurationVersionAttribute.Key)
					: SingleNamedValueItem.ForLatestVAFVersion(this.VaultApplication);

				// Is this an "upgrade from" or "upgrade to" method?
				var upgradeFrom = parameterTypeVersion < configurationVersion;

				// This method is okay.
				{
					var rule = upgradeFrom
							? new DeclaredConfigurationUpgradeRule(readFrom, writeTo, parameterTypeVersion, configurationVersion, method)
							{
								UpgradeToType = configuration,
								UpgradeFromType = parameterType,
								NamedValueStorageManager = this.NamedValueStorageManager
							}
							: new DeclaredConfigurationUpgradeRule(writeTo, readFrom, configurationVersion, parameterTypeVersion, method)
							{
								UpgradeToType = parameterType,
								UpgradeFromType = configuration,
								NamedValueStorageManager = this.NamedValueStorageManager
							};
					rule.JsonConvert = this.JsonConvert;
					identifiedUpgradeMethods.Add(rule);
					this.Logger?.Debug($"Adding {configuration.FullName}.{method.Name} as an upgrade path from {(upgradeFrom ? parameterTypeVersion : configurationVersion)} to {(upgradeFrom ? configurationVersion : parameterTypeVersion)}");
				}

				// Add in upgrade methods on the related type, if we can.
				{
					if (parameterType != configuration && false == this.CheckedConfigurationUpgradeTypes.Contains(parameterType))
					{
						if (this.TryGetDeclaredConfigurationUpgrades
						(
							parameterType,
							out Version v,
							out IEnumerable<DeclaredConfigurationUpgradeRule> mi
						))
							identifiedUpgradeMethods.AddRange(mi);
					}
				}
			}

			// Add in upgrade methods on the previous type, if we can.
			if(null != configurationVersionAttribute?.PreviousVersionType && false == this.CheckedConfigurationUpgradeTypes.Contains(configurationVersionAttribute?.PreviousVersionType))
			{
				if (configurationVersionAttribute.PreviousVersionType == configuration)
				{
					this.Logger?.Error($"{configuration.FullName} indicates the previous type to be itself.");
				}
				else
				{
					if (this.TryGetDeclaredConfigurationUpgrades
					(
						configurationVersionAttribute.PreviousVersionType,
						out Version v,
						out IEnumerable<DeclaredConfigurationUpgradeRule> mi
					))
						identifiedUpgradeMethods.AddRange(mi);
				}
			}

			// Validate the rules.
			{
				var multipleUpgradeRules = identifiedUpgradeMethods
					.GroupBy(m => new Tuple<Version, Version>(m.MigrateFromVersion, m.MigrateToVersion))
					.Where(m => m.Count() > 1)
					.ToList();
				if (multipleUpgradeRules.Any())
				{
					var errorMessage = $"Multiple upgrade rules are defined between configuration versions: {string.Join(", ", multipleUpgradeRules.Select(m => $"{m.Key.Item1}-{m.Key.Item2} ({m.Count()} rules)"))}.";
					this.Logger?.Error(errorMessage);
					return false;
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
