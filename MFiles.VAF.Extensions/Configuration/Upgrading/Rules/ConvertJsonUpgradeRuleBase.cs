using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Linq;
using MFiles.VaultApplications.Logging;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public abstract class ConvertJsonUpgradeRuleBase
		: UpgradeRuleBase<ConvertJsonUpgradeRuleBase.UpgradeRuleOptions>
	{

		/// <summary>
		/// The configuration storage to use.
		/// </summary>
		protected internal virtual IConfigurationStorage ConfigurationStorage { get; }

		/// <summary>
		/// The settings to use for serialization.
		/// </summary>
		public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; } = ConvertJsonUpgradeRuleBase.DefaultJsonSerializerSettings;

		/// <summary>
		/// The formatting for the JSON.
		/// </summary>
		public Newtonsoft.Json.Formatting JsonFormatting { get; set; } = ConvertJsonUpgradeRuleBase.DefaultJsonFormatting;

		/// <summary>
		/// The version that this rule migrates from.
		/// If the version in the configuration is higher than this then this rule will be skipped.
		/// </summary>
		public Version MigrateFrom { get; }

		/// <summary>
		/// The version of the configuration after this migration has completed.
		/// </summary>
		public Version MigrateTo { get; }

		public ConvertJsonUpgradeRuleBase
		(
			UpgradeRuleOptions options,
			Version migrateFrom,
			Version migrateTo
		)
			: this(options, migrateFrom, migrateTo, null)
		{
		}
		public ConvertJsonUpgradeRuleBase
		(
			UpgradeSingleNVSLocationRuleOptions options,
			Version migrateFrom,
			Version migrateTo
		)
			: this(options, migrateFrom, migrateTo, null)
		{
		}

		internal ConvertJsonUpgradeRuleBase
		(
			UpgradeRuleOptions options,
			Version migrateFrom,
			Version migrateTo,
			IConfigurationStorage configurationStorage = null
		)
			: base(options)
		{
			if (null == options.Source)
				throw new ArgumentException("The options must contain a valid source");
			this.ConfigurationStorage = configurationStorage ?? new ConfigurationStorageInVault
			(
				primaryLocation: options.Source.NamedValueType
			);
			this.MigrateFrom = migrateFrom;
			this.MigrateTo = migrateTo ?? throw new ArgumentNullException(nameof(migrateTo));
		}

		internal ConvertJsonUpgradeRuleBase
		(
			UpgradeSingleNVSLocationRuleOptions options,
			Version migrateFrom,
			Version migrateTo,
			IConfigurationStorage configurationStorage = null
		)
			: this(new UpgradeRuleOptions(options), migrateFrom, migrateTo, configurationStorage)
		{
		}

		/// <summary>
		/// Options for <see cref="SimpleConvertConfigurationTypeUpgradeRule"/>.
		/// </summary>
		public class UpgradeRuleOptions
			: UpgradeSingleNVSLocationRuleOptions
		{
			public UpgradeRuleOptions()
			{

			}
			public UpgradeRuleOptions(UpgradeSingleNVSLocationRuleOptions options)
			{
				if (null == options)
					throw new ArgumentNullException(nameof(options));
				this.Source = options.Source;
			}
		}

		/// <summary>
		/// Returns the version from any [ConfigurationVersion] attribute on <typeparamref name="TConfigurationType"/>.
		/// </summary>
		/// <typeparam name="TConfigurationType">The type to check.</typeparam>
		/// <returns>The version, or a version indicating a zero version number.</returns>
		protected static Version GetVersionFromConfiguration<TConfigurationType>()
			where TConfigurationType : IVersionedConfiguration
		{
			return typeof(TConfigurationType)
				.GetCustomAttributes(false)?
				.Where(a => a is ConfigurationVersionAttribute)
				.Cast<ConfigurationVersionAttribute>()
				.FirstOrDefault()?
				.Version ?? new Version("0.0");
		}

		/// <summary>
		/// Converts <paramref name="input"/>, which is the data loaded from NVS, across to the new format.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		protected abstract string Convert(string input);

		/// <inheritdoc />
		public override bool Execute(Vault vault)
		{
			this.Logger?.Trace($"Starting conversion of configuration in {this.Options.Source} from version {this.MigrateFrom} to version {this.MigrateTo}.");

			try
			{

				// Attempt to load the data from storage.
				if (false == this.ConfigurationStorage.ReadConfigurationData(vault, this.Options.Source.Namespace, this.Options.Source.Name, out string oldData))
				{
					this.Logger?.Debug($"Skipping convert configuration rule, as no configuration found in {this.Options.Source}");
					return false; // Not there, so die.
				}

				// Deserialize it to get the version data.
				var oldObject = this.ConfigurationStorage.Deserialize<VersionedConfigurationBase>(oldData);

				// If the version is not the same as what we expected then die.
				if (oldObject?.Version != null
					&& oldObject.Version.ToString() != "0.0" // Default
					&& oldObject.Version != this.MigrateFrom)
				{
					this.Logger?.Debug($"Skipping convert configuration rule, as configured version ({oldObject.Version}) does not match expected version ({this.MigrateFrom}).");
					return false;
				}

				// Convert it.
				var newData = this.Convert(oldData);

				// Ensure that we have the Version property, if it's JSON.
				if (null != this.MigrateTo)
				{
					try
					{
						var obj = Newtonsoft.Json.Linq.JObject.Parse(newData);
						if (null == obj["Version"])
						{
							this.Logger?.Warn("Converted JSON data did not contain a version property; adding automatically.");
							obj["Version"] = this.MigrateTo.ToString();
							newData = Newtonsoft.Json.JsonConvert.SerializeObject
							(
								obj, 
								this.JsonFormatting, 
								this.JsonSerializerSettings
							);
						}
						else if(obj.Value<string>("Version") != this.MigrateTo.ToString())
						{
							this.Logger?.Warn($"Converted JSON data contained a version of {obj.Value<string>("Version")}, but {this.MigrateTo.ToString()} was expected; updating automatically.");
							obj["Version"] = this.MigrateTo.ToString();
							newData = Newtonsoft.Json.JsonConvert.SerializeObject
							(
								obj,
								this.JsonFormatting,
								this.JsonSerializerSettings
							);
						}
					}
					catch(Exception ex)
					{
						this.Logger?.Warn(ex, "Could not parse text into JSON; cannot check/set version number.");
						return false;
					}
				}

				// Save the new data to storage.
				this.Logger?.Info($"Converted configuration in {this.Options.Source} from version {this.MigrateFrom} to version {this.MigrateTo}.");
				this.ConfigurationStorage.SaveConfigurationData(vault, this.Options.Source.Namespace, newData, this.Options.Source.Name);
				this.Logger?.Trace($"Successfully converted configuration in {this.Options.Source} from version {this.MigrateFrom} to version {this.MigrateTo}.");

			}
			catch (Exception e)
			{
				this.Logger?.Error(e, $"Could not convert configuration in {this.Options.Source} from version {this.MigrateFrom} to version {this.MigrateTo}.");
				return false;
			}

			return true;
		}

	}
	/// <summary>
	/// An <see langword="abstract"/> class that defines that the (string) configuration held in a single place in
	/// named value storage should be converted somehow.
	/// </summary>
	/// <typeparam name="TInput"></typeparam>
	/// <typeparam name="TOutput"></typeparam>
	public abstract class ConvertJsonUpgradeRuleBase<TInput, TOutput>
		: ConvertJsonUpgradeRuleBase
		where TInput : class, IVersionedConfiguration, new()
		where TOutput : class, IVersionedConfiguration, new()
	{

		public ConvertJsonUpgradeRuleBase
		(
			UpgradeRuleOptions options
		)
			: this(options, null)
		{
		}
		public ConvertJsonUpgradeRuleBase
		(
			UpgradeSingleNVSLocationRuleOptions options
		)
			: this(new UpgradeRuleOptions(options), null)
		{
		}

		internal ConvertJsonUpgradeRuleBase
		(
			UpgradeRuleOptions options,
			IConfigurationStorage configurationStorage = null
		)
			: base(options, GetVersionFromConfiguration<TInput>(), GetVersionFromConfiguration<TOutput>(), configurationStorage)
		{
		}

		/// <inheritdoc />
		public override bool Execute(Vault vault)
		{
			this.Logger?.Trace($"Starting conversion of configuration in {this.Options.Source} from {typeof(TInput)} to {typeof(TOutput)}.");
			return base.Execute(vault);
		}

	}
}
