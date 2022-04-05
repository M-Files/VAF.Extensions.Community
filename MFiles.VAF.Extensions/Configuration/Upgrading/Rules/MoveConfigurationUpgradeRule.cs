using MFilesAPI;
using System;
using System.Linq;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public class MoveConfigurationUpgradeRule
		: UpgradeRuleBase<MoveConfigurationUpgradeRule.MoveConfigurationUpgradeRuleOptions>
	{

		/// <summary>
		/// The manager to interact with named value storage.
		/// </summary>
		protected INamedValueStorageManager NamedValueStorageManager { get; set; }
			= new VaultNamedValueStorageManager();

		/// <summary>
		/// Creates a rule to move configuration from one location in NVS to another.
		/// </summary>
		/// <param name="options">The options defining how to move the configuration.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="options"/> is null.</exception>
		/// <exception cref="ArgumentException">If some part of <paramref name="options"/> is invalid.</exception>
		public MoveConfigurationUpgradeRule(MoveConfigurationUpgradeRuleOptions options)
			: base(options)
		{
			// Sanity.
			if(false == options.IsValid())
				throw new ArgumentException("The options are invalid.", nameof(options));
		}

		/// <inheritdoc />
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="vault"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="NamedValueStorageManager"/> is null.</exception>
		public override bool Execute(Vault vault)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == this.NamedValueStorageManager)
				throw new InvalidOperationException("The named value storage manager cannot be null.");
			this.Logger?.Trace($"Starting move of configuration from {this.Options.Source} to {this.Options.Target}");

			try
			{

				// Read the values from the source.
				var values = this.Options.Source.GetNamedValues(this.NamedValueStorageManager, vault);
				if (null == values)
				{
					this.Logger?.Debug($"Skipping move configuration rule, as no configuration found in {this.Options.Source}");
					return false;
				}

				// Set the values in the target.
				this.Logger?.Info($"Setting configuration in {this.Options.Target} ({ values })");
				this.Options.Target.SetNamedValues(this.NamedValueStorageManager, vault, values);

				// Optionally remove the old value(s).
				if (this.Options.RemoveMovedValues)
				{
					this.Logger?.Info($"Removing configuration from {this.Options.Source} ({ values })");
					this.Options.Source.RemoveNamedValues(this.NamedValueStorageManager, vault, values.Names.Cast<string>().ToArray());
				}

			}
			catch(Exception e)
			{
				this.Logger?.Error(e, $"Could not move configuration from {this.Options.Source} to {this.Options.Target}");
			}

			// All done!
			this.Logger?.Trace($"Configuration successfully moved from {this.Options.Source} to {this.Options.Target}");
			return true;
		}

		/// <summary>
		/// Options for <see cref="MoveConfigurationUpgradeRule"/>.
		/// </summary>
		public class MoveConfigurationUpgradeRuleOptions
			: UpgradeRuleOptionsBase
		{
			/// <summary>
			/// Whether to remove values from the <see cref="Source"/> once they have been moved.
			/// Defaults to true.
			/// </summary>
			public bool RemoveMovedValues { get; set; } = true;

			/// <summary>
			/// A definition of where to copy the values from.
			/// </summary>
			public ISourceNamedValueItem Source { get; set; }
			
			/// <summary>
			/// A definition of where to copy the values to.
			/// </summary>
			public ITargetNamedValueItem Target { get; set; }

			/// <inheritdoc />
			public override bool IsValid()
			{
				if (null == this.Source || false == this.Source.IsValid())
					return false;
				if (null == this.Target || false == this.Target.IsValid())
					return false;

				return true;
			}
		}
	}
}
