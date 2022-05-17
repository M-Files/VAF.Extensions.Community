using System;
using System.Reflection;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// An upgrade rule that has been declared via attributes.
	/// </summary>
	public class DeclaredConfigurationUpgradeRule
		: ConvertJsonUpgradeRuleBase
	{
		public DeclaredConfigurationUpgradeRule
		(
			VaultApplicationBase vaultApplication,
			Version migrateFrom,
			Version migrateTo
		)
			: base(UpgradeSingleNVSLocationRuleOptions.ForLatestLocation(vaultApplication), migrateFrom, migrateTo)
		{
		}
		public DeclaredConfigurationUpgradeRule
		(
			UpgradeRuleOptions options,
			Version migrateFrom,
			Version migrateTo
		)
			: base(options, migrateFrom, migrateTo)
		{
		}

		/// <summary>
		/// The .NET type of the configuration class that this rule upgrades to.
		/// </summary>
		public Type UpgradeToType { get; set; }

		/// <summary>
		/// The .NET type of the configuration class that this rule upgrades from.
		/// </summary>
		public Type UpgradeFromType { get; set; }

		/// <summary>
		/// The defined method that will execute the upgrade.
		/// </summary>
		public MethodInfo MethodInfo { get; set; }

		/// <inheritdoc />
		protected override string Convert(string input)
		{
			// Sanity.
			if (null == this.UpgradeFromType)
			{
				this.Logger?.Fatal($"Cannot upgrade configuration as from type is null.");
				throw new InvalidOperationException("Cannot upgrade configuration as from type is null.");
			}
			if (null == this.UpgradeToType)
			{
				this.Logger?.Fatal($"Cannot upgrade configuration as to type is null.");
				throw new InvalidOperationException("Cannot upgrade configuration as to type is null.");
			}
			if (null == this.MethodInfo)
			{
				this.Logger?.Fatal($"Cannot upgrade configuration as upgrade method is null.");
				throw new InvalidOperationException("Cannot upgrade configuration as upgrade method is null.");
			}
			if (null == base.ConfigurationStorage)
			{
				this.Logger?.Fatal($"Cannot upgrade configuration as configuration storage instance is null.");
				throw new InvalidOperationException("Cannot upgrade configuration as configuration storage instance is null");
			}

			// Handle empty data.
			if (string.IsNullOrWhiteSpace(input))
				input = "{}";

			// Get the input object.
			var inputObj = base.ConfigurationStorage.Deserialize(this.UpgradeFromType, input);

			// Convert it.
			object outputObj = null;
			try
			{
				if (this.MethodInfo.IsStatic)
				{
					// If static we don't need an object reference.
					outputObj = this.MethodInfo.Invoke(null, new object[] { inputObj });
				}
				else
				{
					// Create an instance.
					outputObj = Activator.CreateInstance(this.UpgradeToType);
					this.MethodInfo.Invoke(outputObj, new object[] { inputObj });
				}
			}
			catch (Exception ex)
			{
				this.Logger?.Fatal(ex, $"Exception whilst upgrading configuration from {this.UpgradeFromType} to {this.UpgradeToType}.");
			}

			// Deserialise back.
			return base.ConfigurationStorage.Serialize(outputObj);

		}
	}
}
