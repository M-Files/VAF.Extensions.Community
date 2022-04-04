using MFiles.VAF.Configuration;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// Defines an upgrade rule where the type of configuration data fundamentally changes
	/// from <typeparamref name="TInput"/> to <typeparamref name="TOutput"/>.  An instance of
	/// this class allows the configuration to be converted/upgraded before it is loaded,
	/// ensuring that the application will continue to load.
	/// </summary>
	/// <typeparam name="TInput">The older type of configuration.</typeparam>
	/// <typeparam name="TOutput">The newer type of configuration.</typeparam>
	public abstract class ConvertConfigurationTypeUpgradeRule<TInput, TOutput>
		: UpgradeRuleBase<ConvertConfigurationTypeUpgradeRule<TInput, TOutput>.ConvertConfigurationTypeUpgradeRuleOptions>
		where TInput : class, new()
		where TOutput: class, new()
	{
		/// <summary>
		/// The configuration storage to use.
		/// </summary>
		protected internal virtual IConfigurationStorage ConfigurationStorage { get; }

		protected ConvertConfigurationTypeUpgradeRule(ConvertConfigurationTypeUpgradeRuleOptions options)
			: this(options, null)
		{
		}

		internal ConvertConfigurationTypeUpgradeRule
		(
			ConvertConfigurationTypeUpgradeRuleOptions options, 
			IConfigurationStorage configurationStorage
		)
			: base(options)
		{
			this.ConfigurationStorage = configurationStorage ?? new ConfigurationStorageInVault
			(
				primaryLocation: options.Source.NamedValueType
			);
		}

		/// <inheritdoc />
		public override bool Execute(Vault vault)
		{

			// Attempt to load the data from storage.
			if(false == this.ConfigurationStorage.ReadConfigurationData(vault, this.Options.Source.Namespace, this.Options.Source.Name, out string oldData))
				return false; // Not there, so die.

			// Deserialize it.
			var oldObject = this.ConfigurationStorage.Deserialize<TInput>(oldData);

			// Convert it.
			var newObject = this.Convert(oldObject);

			// Save the new data to storage.
			this.ConfigurationStorage.Save(vault, newObject, this.Options.Source.Namespace, this.Options.Source.Name);

			return true;
		}

		/// <summary>
		/// Converts the older configuration (<paramref name="input"/>) to an instance of <typeparamref name="TOutput"/>.
		/// </summary>
		/// <param name="input">An instance of the older configuration to migrate.</param>
		/// <returns>The equivalent new configuration structure.</returns>
		public abstract TOutput Convert(TInput input);

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
		}
	}
}
