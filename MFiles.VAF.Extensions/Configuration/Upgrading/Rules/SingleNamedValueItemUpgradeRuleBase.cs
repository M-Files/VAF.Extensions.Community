using MFiles.VaultApplications.Logging;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public abstract class SingleNamedValueItemUpgradeRuleBase
		: IUpgradeRule
	{
		/// <summary>
		/// The logger for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger<SingleNamedValueItemUpgradeRuleBase>();

		/// <summary>
		/// A definition of where the value should be read from.
		/// </summary>
		public virtual ISingleNamedValueItem ReadFrom { get; }

		/// <summary>
		/// A definition of where the value should be written to.
		/// </summary>
		/// <remarks>If null then will be written to <see cref="ReadFrom"/>.</remarks>
		public virtual ISingleNamedValueItem WriteTo { get; }

		/// <inheritdoc />
		public Version MigrateFromVersion { get; }

		/// <inheritdoc />
		public Version MigrateToVersion { get; }

		/// <inheritdoc />
		public virtual bool IsValid()
			=> true;

		public SingleNamedValueItemUpgradeRuleBase(VaultApplicationBase vaultApplication, Version migrateFromVersion, Version migrateToVersion)
			: this(SingleNamedValueItem.ForLatestVAFVersion(vaultApplication), migrateFromVersion, migrateToVersion)
		{
		}

		public SingleNamedValueItemUpgradeRuleBase(ISingleNamedValueItem readFromAndWriteTo, Version migrateFromVersion, Version migrateToVersion)
			: this(readFromAndWriteTo, null, migrateFromVersion, migrateToVersion)
		{
		}

		public SingleNamedValueItemUpgradeRuleBase(ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo, Version migrateFromVersion, Version migrateToVersion)
		{
			this.ReadFrom = readFrom ?? throw new ArgumentNullException(nameof(readFrom));
			this.WriteTo = writeTo; // Allow nulls; we'll fall back to the readfrom location.
			this.MigrateFromVersion = migrateFromVersion ?? throw new ArgumentNullException(nameof(migrateFromVersion));
			this.MigrateToVersion = migrateToVersion ?? throw new ArgumentNullException(nameof(migrateToVersion));

			if (null != this.ReadFrom && false == this.ReadFrom.IsValid())
				throw new ArgumentException("The named value location is invalid.", nameof(readFrom));
			if (null != this.WriteTo && false == this.WriteTo.IsValid())
				throw new ArgumentException("The named value location is invalid.", nameof(writeTo));
		}

		/// <summary>
		/// Converts <paramref name="input"/>, which is the data loaded from NVS, across to the new format.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		protected abstract string Convert(string input);

		/// <summary>
		/// The manager to use to access named value storage.
		/// </summary>
		public INamedValueStorageManager NamedValueStorageManager { get; set; } = new VaultNamedValueStorageManager();

		/// <summary>
		/// The converter to serialize/deserialize JSON.
		/// </summary>
		public IJsonConvert JsonConvert { get; set; } = new NewtonsoftJsonConvert();

		/// <summary>
		/// Attempts to read the data from <paramref name="readFrom"/> in <paramref name="vault"/>.
		/// </summary>
		/// <param name="readFrom">The item to read from named value storage.</param>
		/// <param name="vault">The vault to read the data from.</param>
		/// <param name="data">The data in the vault.</param>
		/// <param name="version">The version of the data, if available.</param>
		/// <returns><see langword="true"/> if the data could be parsed, false otherwise.</returns>
		public bool TryRead(ISingleNamedValueItem readFrom, Vault vault, out string data, out Version version)
		{
			data = null;
			version = null;
			return readFrom?.TryRead(vault, this.NamedValueStorageManager, this.JsonConvert, out data, out version) ?? false;
		}

		/// <inheritdoc />
		public virtual bool Execute(Vault vault)
		{
			this.Logger?.Trace($"Starting conversion of configuration in {this.ReadFrom}.");

			// Sanity.
			if (null == this.NamedValueStorageManager)
				throw new InvalidOperationException($"{nameof(this.NamedValueStorageManager)} cannot be null.");
			if (null == this.JsonConvert)
				throw new InvalidOperationException($"{nameof(this.JsonConvert)} cannot be null.");

			try
			{

				// If we can't get the data then die.
				if(false == this.TryRead(this.ReadFrom, vault, out string data, out Version version))
				{
					this.Logger?.Debug($"Skipping convert configuration rule, as no configuration found in {this.ReadFrom}");
					return false;
				}

				// If the version is not the same as what we expected then die.
				if (version != null
					&& version.ToString() != "0.0" // Default
					&& version != this.MigrateFromVersion)
				{
					this.Logger?.Debug($"Skipping convert configuration rule, as configured version ({version}) does not match expected version ({this.MigrateFromVersion}).");
					return false;
				}

				// Convert it.
				var newData = this.Convert(data);

				// Ensure that we have the Version property, if it's JSON.
				if (null != this.MigrateToVersion)
				{
					try
					{
						var obj = Newtonsoft.Json.Linq.JObject.Parse(newData);
						if (null == obj["Version"])
						{
							this.Logger?.Warn("Converted JSON data did not contain a version property; adding automatically.");
							obj["Version"] = this.MigrateToVersion.ToString();
							newData = this.JsonConvert.Serialize(obj);
						}
						else if (obj.Value<string>("Version") != this.MigrateToVersion.ToString())
						{
							this.Logger?.Warn($"Converted JSON data contained a version of {obj.Value<string>("Version")}, but {this.MigrateToVersion.ToString()} was expected; updating automatically.");
							obj["Version"] = this.MigrateToVersion.ToString();
							newData = this.JsonConvert.Serialize(obj);
						}
					}
					catch (Exception ex)
					{
						this.Logger?.Warn(ex, "Could not parse text into JSON; cannot check/set version number.");
						return false;
					}
				}

				// Save the new data to storage.
				this.Logger?.Debug($"Attempting to update configuration in NVS.");
				{
					// Update the named values.
					var namedValues = this.NamedValueStorageManager.GetNamedValues(vault, this.ReadFrom.NamedValueType, this.ReadFrom.Namespace);
					namedValues[this.WriteTo?.Name ?? this.ReadFrom.Name] = newData;
					this.Logger?.Trace("Writing new configuration...");
					this.NamedValueStorageManager.SetNamedValues
					(
						vault,
						this.WriteTo?.NamedValueType ?? this.ReadFrom.NamedValueType,
						this.WriteTo?.Namespace ?? this.ReadFrom.Namespace,
						namedValues
					);
				}

				// Remove the old data.
				{
					// Update the named values.
					var namedValues = this.NamedValueStorageManager.GetNamedValues(vault, this.ReadFrom.NamedValueType, this.ReadFrom.Namespace);
					namedValues[this.ReadFrom.Name] = null;
					this.Logger?.Trace("Removing old configuration...");
					this.NamedValueStorageManager.SetNamedValues
					(
						vault,
						this.ReadFrom.NamedValueType,
						this.ReadFrom.Namespace,
						namedValues
					);
				}

				// Done!
				this.Logger?.Info($"Converted configuration from version {this.MigrateFromVersion} to version {this.MigrateToVersion}.");
				this.Logger?.Trace($"Successfully converted configuration in {this.ReadFrom} from version {this.MigrateFromVersion} to version {this.MigrateToVersion}.");

			}
			catch (Exception e)
			{
				this.Logger?.Error(e, $"Could not convert configuration in {this.ReadFrom} from version {this.MigrateFromVersion} to version {this.MigrateToVersion}.");
				return false;
			}

			return true;
		}
	}
	public abstract class SingleNamedValueItemUpgradeRuleBase<TOptions>
		: SingleNamedValueItemUpgradeRuleBase
		where TOptions : class, IUpgradeRuleOptions
	{
		/// <summary>
		/// Options for the rule.
		/// </summary>
		protected internal TOptions Options { get; } = default;

		/// <inheritdoc />
		public override bool IsValid()
			=> base.IsValid() && (this.Options?.IsValid() ?? true);

		public SingleNamedValueItemUpgradeRuleBase(VaultApplicationBase vaultApplication, Version migrateFromVersion, Version migrateToVersion)
			: base(vaultApplication, migrateFromVersion, migrateToVersion)
		{
		}

		public SingleNamedValueItemUpgradeRuleBase(ISingleNamedValueItem readFromAndWriteTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFromAndWriteTo, migrateFromVersion, migrateToVersion)
		{
		}

		public SingleNamedValueItemUpgradeRuleBase(ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFrom, writeTo, migrateFromVersion, migrateToVersion)
		{
		}

		public SingleNamedValueItemUpgradeRuleBase(TOptions options, VaultApplicationBase vaultApplication, Version migrateFromVersion, Version migrateToVersion)
			: base(vaultApplication, migrateFromVersion, migrateToVersion)
		{
			// Sanity.
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
		}

		public SingleNamedValueItemUpgradeRuleBase(TOptions options, ISingleNamedValueItem readFromAndWriteTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFromAndWriteTo, migrateFromVersion, migrateToVersion)
		{
			// Sanity.
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
		}

		public SingleNamedValueItemUpgradeRuleBase(TOptions options, ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFrom, writeTo, migrateFromVersion, migrateToVersion)
		{
			// Sanity.
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
		}
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
		public virtual bool IsValid()
			=> true;
	}
}
