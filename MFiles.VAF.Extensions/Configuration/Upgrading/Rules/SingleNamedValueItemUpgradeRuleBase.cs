using MFiles.VaultApplications.Logging;
using MFilesAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

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
		public virtual ISingleNamedValueItem ReadFrom { get; protected set; }

		/// <summary>
		/// A definition of where the value should be written to.
		/// </summary>
		/// <remarks>If null then will be written to <see cref="ReadFrom"/>.</remarks>
		public virtual ISingleNamedValueItem WriteTo { get; protected set; }

		/// <inheritdoc />
		public Version MigrateFromVersion { get; protected set; }

		/// <inheritdoc />
		public Version MigrateToVersion { get; protected set; }

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

				// Deal with the JSON.
				{
					JObject sourceObj, targetObj;
					try
					{
						// Try to parse both old and new data.
						sourceObj = JObject.Parse(data);
						targetObj = JObject.Parse(newData);

						// Copy the comments from the source (original) to target (new) data.
						this.CopyComments(sourceObj, targetObj);


						// Ensure that we have the Version property, if it's JSON.
						if (null != this.MigrateToVersion)
						{
							try
							{
								if (null == targetObj["Version"])
								{
									this.Logger?.Debug("Converted JSON data did not contain a version property; adding automatically.");
									targetObj["Version"] = this.MigrateToVersion.ToString();
								}
								else if (targetObj.Value<string>("Version") != this.MigrateToVersion.ToString())
								{
									this.Logger?.Debug($"Converted JSON data contained a version of {targetObj.Value<string>("Version")}, but {this.MigrateToVersion} was expected; updating automatically.");
									targetObj["Version"] = this.MigrateToVersion.ToString();
								}
							}
							catch (Exception ex)
							{
								this.Logger?.Warn(ex, "Could not parse text into JSON; cannot check/set version number.");
								return false;
							}
						}

						// Update the string representation of the new data.
						newData = this.JsonConvert.Serialize(targetObj);
					}
					catch (Exception ex)
					{
						this.Logger?.Warn(ex, "Could not parse text into JSON; cannot check/set version number.");
						return false;
					}
				}

				// Save the new data to storage.
				var type = this.WriteTo?.NamedValueType ?? this.ReadFrom.NamedValueType;
				var @namespace = this.WriteTo?.Namespace ?? this.ReadFrom.Namespace;
				var name = this.WriteTo?.Name ?? this.ReadFrom.Name;

				this.Logger?.Debug($"Attempting to update configuration in NVS.");
				{
					// Update the named values.
					this.Logger?.Trace($"Writing new configuration in {@namespace}.{name} ({type})...");
					var namedValues = this.NamedValueStorageManager.GetNamedValues(vault, type, @namespace) ?? new NamedValues();
					namedValues[name] = newData;
					this.NamedValueStorageManager.SetNamedValues(vault, type, @namespace, namedValues);
				}

				// Remove the old data.
				{
					// Only remove the data if we're moving places.
					if (this.WriteTo != null && (
						this.WriteTo.Namespace != this.ReadFrom.Namespace
						|| this.WriteTo.NamedValueType != this.ReadFrom.NamedValueType
						|| this.WriteTo.Name != this.ReadFrom.Name
						))
					{
						// Update the named values.
						this.Logger?.Debug($"Removing old configuration from {this.ReadFrom.Namespace}.{this.ReadFrom.Name} ({this.ReadFrom.NamedValueType})...");
						this.NamedValueStorageManager.RemoveNamedValues
						(
							vault,
							this.ReadFrom.NamedValueType,
							this.ReadFrom.Namespace,
							new[] { this.ReadFrom.Name }
						);
					}
				}

				// Done!
				this.Logger?.Info($"Converted configuration from version {this.MigrateFromVersion} to version {this.MigrateToVersion}.");

			}
			catch (Exception e)
			{
				this.Logger?.Error(e, $"Could not convert configuration in {this.ReadFrom} from version {this.MigrateFromVersion} to version {this.MigrateToVersion}.");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Performs a deep-copy of all comments from <paramref name="source"/> to <paramref name="target"/>.
		/// </summary>
		/// <param name="source">The source object to copy comments from.</param>
		/// <param name="target">The target object to copy comments to.</param>
		/// <remarks>
		/// If the value exists in <paramref name="source"/> but not <paramref name="target"/> then it is copied to <paramref name="target"/>.
		/// If the value exists in <paramref name="target"/> but not <paramref name="source"/> then it is left as-is.
		/// If the value exists in both <paramref name="source"/> and <paramref name="target"/> then <paramref name="target"/> is updated with the value from <paramref name="source"/>.
		/// </remarks>
		protected internal virtual void CopyComments(JObject source, JObject target)
		{
			// Sanity.
			if (null == source || null == target)
				return;
			this.Logger?.Trace($"Copying comments on '{(string.IsNullOrWhiteSpace(source.Path) ? "(root)" : source.Path)}'.");

			var sourceProperties = source.Properties().Select(p => p.Name).Where(n => !n.EndsWith("-Comment")).ToArray();
			foreach (var propertyName in sourceProperties)
			{
				// Skip if the target is missing this property.
				var sourcePropertyValue = source[propertyName];
				var targetPropertyValue = target[propertyName];
				if (null == targetPropertyValue)
					continue;

				// Do we need to do anything more clever with this type of property value?
				switch (sourcePropertyValue.Type)
				{
					case JTokenType.Object:
						{
							// If the type of the source property is object then recurse.
							this.CopyComments(sourcePropertyValue as JObject, targetPropertyValue as JObject);
							break;
						}
					case JTokenType.Array:
						{
							// If this is an array then we can copy comments for the elements.
							var sourceJArray = sourcePropertyValue as JArray;
							var targetJArray = targetPropertyValue as JArray ?? new JArray();
							if (null != sourceJArray)
							{
								// Iterate over the items in the collection.
								for (var i = 0; i < sourceJArray.Count; i++)
								{
									// Copy the comment for the array index from source to target.
									this.CopyPropertyComment(source, target, $"{propertyName}-{i}");

									// If there's an equivalent entry in the target then process the elements.
									if (i < targetJArray.Count
										&& sourceJArray[i] is JObject sourceElement
										&& targetJArray[i] is JObject targetElement)
									{
										this.CopyComments(sourceElement, targetElement);
									}
								}
							}
							break;
						}
					case JTokenType.Null:
						{
							// Skip nulls.
							continue;
						}
				}

				// Copy the comment from source to target.
				this.CopyPropertyComment(source, target, propertyName);
			}
		}

		/// <summary>
		/// Copies the comment for a single property named <paramref name="propertyName"/> from <paramref name="source"/> to <paramref name="target"/>.
		/// Note: if you wish to copy the "Item-Comment" property then "Item" should be passed in <paramref name="propertyName"/>.
		/// Note: if you wish to copy the comment for the second array element in "Triggers" then "Triggers-1" should be passed in <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="source">The source object to locate the property in.</param>
		/// <param name="target">The target object to update.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="target"/> are null.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="propertyName"/> is null or whitespace.</exception>
		/// <remarks>
		/// If the value exists in <paramref name="source"/> but not <paramref name="target"/> then it is copied to <paramref name="target"/>.
		/// If the value exists in <paramref name="target"/> but not <paramref name="source"/> then it is left as-is.
		/// If the value exists in both <paramref name="source"/> and <paramref name="target"/> then <paramref name="target"/> is updated with the value from <paramref name="source"/>.
		/// </remarks>
		protected internal void CopyPropertyComment(JObject source, JObject target, string propertyName)
		{
			// Sanity.
			if (null == source)
				throw new ArgumentNullException(nameof(source));
			if (null == target)
				throw new ArgumentNullException(nameof(target));
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentException(nameof(propertyName));

			// Get the comment for this property in both source and target.
			propertyName += "-Comment";
			var sourcePropertyValue = source[propertyName];
			var targetPropertyValue = target[propertyName];

			// Does it not exist in either?
			if (null == sourcePropertyValue && null == targetPropertyValue)
				return;

			// Does it exist in the target and not the source?
			if (null == sourcePropertyValue && null != targetPropertyValue)
				return;

			// If it's already in the target then remove it (we'll add it in a sec).
			if (null != targetPropertyValue)
				target.Remove(propertyName);

			// Copy the source to the target.
			this.Logger?.Debug($"Copying '{(string.IsNullOrWhiteSpace(source.Path) ? "" : source.Path + ".")}{propertyName}' from source to target.");
			targetPropertyValue = sourcePropertyValue.DeepClone();
			target.Add(new JProperty(propertyName, targetPropertyValue));

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
