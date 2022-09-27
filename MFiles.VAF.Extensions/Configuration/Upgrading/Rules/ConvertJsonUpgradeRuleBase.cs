using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Linq;
using MFiles.VaultApplications.Logging;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public class ConvertJsonUpgradeRule<TConvertFrom, TConvertTo>
		: ConvertJsonUpgradeRuleBase<TConvertFrom, TConvertTo>
		where TConvertFrom : class, IVersionedConfiguration, new()
		where TConvertTo : class, IVersionedConfiguration, new()
	{
		/// <summary>
		/// The logger for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger<ConvertJsonUpgradeRule<TConvertFrom, TConvertTo>>();

		/// <summary>
		/// The function that converts an instance from <typeparamref name="TConvertFrom"/> to <typeparamref name="TConvertTo"/>.
		/// Called by <see cref="Convert(TConvertFrom)"/>
		/// </summary>
		protected Func<TConvertFrom, TConvertTo> ConversionFunction { get; set; }


		public ConvertJsonUpgradeRule(Func<TConvertFrom, TConvertTo> conversionFunction, VaultApplicationBase vaultApplication, Version migrateFromVersion, Version migrateToVersion)
			: base(vaultApplication, migrateFromVersion, migrateToVersion)
		{
			this.ConversionFunction = conversionFunction ?? throw new ArgumentNullException(nameof(conversionFunction));
		}

		public ConvertJsonUpgradeRule(Func<TConvertFrom, TConvertTo> conversionFunction, ISingleNamedValueItem readFromAndWriteTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFromAndWriteTo, migrateFromVersion, migrateToVersion)
		{
			this.ConversionFunction = conversionFunction ?? throw new ArgumentNullException(nameof(conversionFunction));
		}

		public ConvertJsonUpgradeRule(Func<TConvertFrom, TConvertTo> conversionFunction, ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFrom, writeTo, migrateFromVersion, migrateToVersion)
		{
			this.ConversionFunction = conversionFunction ?? throw new ArgumentNullException(nameof(conversionFunction));
		}

		/// <inheritdoc />
		protected override TConvertTo Convert(TConvertFrom upgradeFrom)
			=> null == this.ConversionFunction
			? throw new InvalidOperationException($"{this.GetType().FullName}.{nameof(this.JsonConvert)} cannot be null.")
			: this.ConversionFunction(upgradeFrom);
	}
	public abstract class ConvertJsonUpgradeRuleBase<TConvertFrom, TConvertTo>
		: SingleNamedValueItemUpgradeRuleBase
		where TConvertFrom : class, IVersionedConfiguration, new()
		where TConvertTo : class, IVersionedConfiguration, new()
	{
		/// <summary>
		/// The logger for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger<ConvertJsonUpgradeRuleBase<TConvertFrom, TConvertTo>>();

		protected ConvertJsonUpgradeRuleBase(VaultApplicationBase vaultApplication, Version migrateFromVersion, Version migrateToVersion)
			: base(vaultApplication, migrateFromVersion, migrateToVersion)
		{
		}

		protected ConvertJsonUpgradeRuleBase(ISingleNamedValueItem readFromAndWriteTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFromAndWriteTo, migrateFromVersion, migrateToVersion)
		{
		}

		protected ConvertJsonUpgradeRuleBase(ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo, Version migrateFromVersion, Version migrateToVersion)
			: base(readFrom, writeTo, migrateFromVersion, migrateToVersion)
		{
		}

		/// <inheritdoc />
		protected override string Convert(string input)
		{
			// Sanity.
			if (null == input)
				return "{}";
			if (null == this.JsonConvert)
				throw new InvalidOperationException($"{this.GetType().FullName}.{nameof(this.JsonConvert)} cannot be null.");

			// Parse the old version, convert, then serialize the new one!
			this.Logger?.Trace($"Attempting to deserialize content to {typeof(TConvertFrom)}.");
			var oldInstance = this.JsonConvert.Deserialize<TConvertFrom>(input);

			this.Logger?.Trace($"Attempting to deserialize convert content to {typeof(TConvertTo)}.");
			var newInstance = this.Convert(oldInstance);

			this.Logger?.Trace($"Attempting to serialize new content.");
			return this.JsonConvert.Serialize(newInstance);
			
		}

		/// <summary>
		/// Converts an instance of <typeparamref name="TConvertFrom"/> to <typeparamref name="TConvertTo"/>.
		/// </summary>
		/// <param name="convertFrom">The instance to convert.</param>
		/// <returns>The converted instance.</returns>
		protected abstract TConvertTo Convert(TConvertFrom convertFrom);

	}
}
