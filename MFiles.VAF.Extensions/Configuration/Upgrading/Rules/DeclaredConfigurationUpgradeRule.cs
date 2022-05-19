using MFiles.VAF.Configuration;
using MFiles.VaultApplications.Logging;
using System;
using System.Reflection;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// An upgrade rule that has been declared via attributes.
	/// </summary>
	public class DeclaredConfigurationUpgradeRule
		: SingleNamedValueItemUpgradeRuleBase
	{
		/// <summary>
		/// The logger for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger<DeclaredConfigurationUpgradeRule>();

		public DeclaredConfigurationUpgradeRule(VaultApplicationBase vaultApplication)
			: base(vaultApplication)
		{
		}

		public DeclaredConfigurationUpgradeRule(ISingleNamedValueItem readFromAndWriteTo)
			: base(readFromAndWriteTo)
		{
		}

		public DeclaredConfigurationUpgradeRule(ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo)
			: base(readFrom, writeTo)
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

			// Handle empty data.
			if (string.IsNullOrWhiteSpace(input))
				input = "{}";

			this.Logger?.Info($"Converting configuration version from {this.MigrateFromVersion} to {this.MigrateToVersion}.");

			// Get the input object.
			var inputObj = base.JsonConvert.Deserialize(input);

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
			return base.JsonConvert.Serialize(outputObj);

		}
	}
}
