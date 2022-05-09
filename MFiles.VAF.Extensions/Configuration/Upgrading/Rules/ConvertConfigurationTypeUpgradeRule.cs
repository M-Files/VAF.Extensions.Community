using MFiles.VAF.Configuration;
using System;
using System.Resources;

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
	public class ConvertConfigurationTypeUpgradeRule<TInput, TOutput>
		: ConvertJsonUpgradeRuleBase<TInput, TOutput>
		where TInput : class, IVersionedConfiguration, new()
		where TOutput : class, IVersionedConfiguration, new()
	{

		protected Func<TInput, TOutput> Conversion { get; }

		public ConvertConfigurationTypeUpgradeRule
		(
			UpgradeRuleOptions options,
			Func<TInput, TOutput> conversion
		)
			: this(options, conversion, null)
		{
        }

        public ConvertConfigurationTypeUpgradeRule
        (
            VaultApplicationBase vaultApplication,
            Func<TInput, TOutput> conversion
        )
            : this(UpgradeSingleNVSLocationRuleOptions.ForLatestLocation(vaultApplication), conversion)
        {
        }

        public ConvertConfigurationTypeUpgradeRule
		(
			UpgradeSingleNVSLocationRuleOptions options,
			Func<TInput, TOutput> conversion
		)
			: this(new UpgradeRuleOptions(options), conversion)
		{
		}

		internal ConvertConfigurationTypeUpgradeRule
		(
			UpgradeRuleOptions options, 
			Func<TInput, TOutput> conversion,
			IConfigurationStorage configurationStorage = null
		)
			: base(options, configurationStorage)
		{
			this.Conversion = conversion ?? throw new ArgumentNullException(nameof(conversion), "The conversion function cannot be null.");
		}

		internal ConvertConfigurationTypeUpgradeRule
		(
			UpgradeSingleNVSLocationRuleOptions options,
			Func<TInput, TOutput> conversion,
			IConfigurationStorage configurationStorage = null
		)
			: this(new UpgradeRuleOptions(options), conversion, configurationStorage)
		{
		}

		/// <inheritdoc />
		protected override string Convert(string input)
		{
			// Deserialize it.
			this.Logger?.Trace($"Attempting to deserialize content to {typeof(TInput)}.");
			var oldObject = this.ConfigurationStorage.Deserialize<TInput>(input);

			// Convert it.
			this.Logger?.Trace($"Attempting to convert content to {typeof(TOutput)}.");
			var newObject = this.Convert(oldObject);

			// Serialize the new string.
			return Newtonsoft.Json.JsonConvert.SerializeObject
			(
				newObject,
				this.JsonFormatting,
				this.JsonSerializerSettings
			);
		}

		/// <summary>
		/// Converts the older configuration (<paramref name="input"/>) to an instance of <typeparamref name="TOutput"/>.
		/// </summary>
		/// <param name="input">An instance of the older configuration to migrate.</param>
		/// <returns>The equivalent new configuration structure.</returns>
		public virtual TOutput Convert(TInput input)
			=> this.Conversion(input);
	}
}
