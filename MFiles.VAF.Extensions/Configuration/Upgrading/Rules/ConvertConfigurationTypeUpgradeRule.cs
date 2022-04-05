using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Linq;
using System.Resources;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public class ConvertConfigurationTypeUpgradeRule
	{
		public static Newtonsoft.Json.JsonSerializerSettings DefaultJsonSerializerSettings { get; }
			= new Newtonsoft.Json.JsonSerializerSettings()
			{
				DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
			};
	}
	/// <summary>
	/// Defines an upgrade rule where the type of configuration data fundamentally changes
	/// from <typeparamref name="TInput"/> to <typeparamref name="TOutput"/>.  An instance of
	/// this class allows the configuration to be converted/upgraded before it is loaded,
	/// ensuring that the application will continue to load.
	/// </summary>
	/// <typeparam name="TInput">The older type of configuration.</typeparam>
	/// <typeparam name="TOutput">The newer type of configuration.</typeparam>
	public class ConvertConfigurationTypeUpgradeRule<TInput, TOutput>
		: UpgradeRuleBase<ConvertConfigurationTypeUpgradeRule<TInput, TOutput>.ConvertConfigurationTypeUpgradeRuleOptions>
		where TInput : class, IVersionedConfiguration, new()
		where TOutput : class, IVersionedConfiguration, new()
	{
		/// <summary>
		/// The configuration storage to use.
		/// </summary>
		protected internal virtual IConfigurationStorage ConfigurationStorage { get; }

		protected Func<TInput, TOutput> Conversion { get; }

		public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; } = ConvertConfigurationTypeUpgradeRule.DefaultJsonSerializerSettings;

		public ConvertConfigurationTypeUpgradeRule
		(
			ConvertConfigurationTypeUpgradeRuleOptions options,
			Func<TInput, TOutput> conversion
		)
			: this(options, conversion, null)
		{
		}

		internal ConvertConfigurationTypeUpgradeRule
		(
			ConvertConfigurationTypeUpgradeRuleOptions options, 
			Func<TInput, TOutput> conversion,
			IConfigurationStorage configurationStorage = null
		)
			: base(options)
		{
			this.ConfigurationStorage = configurationStorage ?? new ConfigurationStorageInVault
			(
				primaryLocation: options.Source.NamedValueType
			);
			this.Conversion = conversion ?? throw new ArgumentNullException(nameof(conversion), "The conversion function cannot be null.");
		}

		/// <inheritdoc />
		public override bool Execute(Vault vault)
		{
			this.Logger?.Trace($"Starting conversion of configuration in {this.Options.Source} from {typeof(TInput)} to {typeof(TOutput)}.");

			try
			{

				// Attempt to load the data from storage.
				if (false == this.ConfigurationStorage.ReadConfigurationData(vault, this.Options.Source.Namespace, this.Options.Source.Name, out string oldData))
				{
					this.Logger?.Debug($"Skipping convert configuration rule, as no configuration found in {this.Options.Source}");
					return false; // Not there, so die.
				}

				// What are we going "from" and "to"?
				var migrateFromVersion = this.GetVersionFromConfiguration<TInput>();
				var migrateToVersion = this.GetVersionFromConfiguration<TOutput>();

				// Sanity.
				if(null == migrateToVersion)
				{
					this.Logger?.Error($"Cannot convert configuration as the target type ({typeof(TOutput).FullName}) does not have a [Version] attribute.");
					return false;
				}

				// Deserialize it.
				var oldObject = this.ConfigurationStorage.Deserialize<TInput>(oldData);

				// If the version is not the same as what we expected then die.
				if (oldObject?.Version != null
					&& oldObject.Version != migrateFromVersion)
				{
					this.Logger?.Debug($"Skipping convert configuration rule, as configured version ({oldObject.Version}) does not match expected version ({migrateFromVersion}).");
					return false;
				}

				// Convert it.
				var newObject = this.Convert(oldObject);
				var newString = Newtonsoft.Json.JsonConvert.SerializeObject
				(
					newObject,
					Newtonsoft.Json.Formatting.Indented,
					this.JsonSerializerSettings
				);

				// Save the new data to storage.
				this.Logger?.Info($"Converted configuration in {this.Options.Source} from {typeof(TInput)} to {typeof(TOutput)}.");
				this.ConfigurationStorage.SaveConfigurationData(vault, this.Options.Source.Namespace, newString, this.Options.Source.Name);

			}
			catch (Exception e)
			{
				this.Logger?.Error(e, $"Could not convert configuration in {this.Options.Source} from {typeof(TInput)} to {typeof(TOutput)}.");
			}

			this.Logger?.Trace($"Successfully converted configuration in {this.Options.Source} from {typeof(TInput)} to {typeof(TOutput)}.");
			return true;
		}

		/// <summary>
		/// Returns the version from any [ConfigurationVersion] attribute on <typeparamref name="TConfigurationType"/>.
		/// </summary>
		/// <typeparam name="TConfigurationType">The type to check.</typeparam>
		/// <returns>The version, or a version indicating a zero version number.</returns>
		protected virtual Version GetVersionFromConfiguration<TConfigurationType>()
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
		/// Converts the older configuration (<paramref name="input"/>) to an instance of <typeparamref name="TOutput"/>.
		/// </summary>
		/// <param name="input">An instance of the older configuration to migrate.</param>
		/// <returns>The equivalent new configuration structure.</returns>
		public TOutput Convert(TInput input)
			=> this.Conversion(input);

		/// <summary>
		/// Options for <see cref="ConvertConfigurationTypeUpgradeRule"/>.
		/// </summary>
		public class ConvertConfigurationTypeUpgradeRuleOptions
			: UpgradeRuleOptionsBase
		{
			/// <summary>
			/// A definition of where the values are stored.
			/// The value will be read, converted, then written to the same location.
			/// </summary>
			public ISingleNamedValueItem Source { get; set; }

			/// <inheritdoc />
			public override bool IsValid()
			{
				if (null == this.Source || false == this.Source.IsValid())
					return false;

				return true;
			}

			public static ConvertConfigurationTypeUpgradeRuleOptions ForLatestLocation(VaultApplicationBase vaultApplication)
			{
				return new ConvertConfigurationTypeUpgradeRuleOptions()
				{
					Source = new SingleNamedValueItem
					(
						MFNamedValueType.MFSystemAdminConfiguration,
						vaultApplication?.GetType()?.FullName ?? throw new ArgumentNullException(nameof(vaultApplication)),
						"configuration"
					)
				};
			}
		}
	}
}
