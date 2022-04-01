using MFilesAPI;
using System;
using System.Linq;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public class MoveConfigurationUpgradeRule
		: IUpgradeRule
	{
		/// <summary>
		/// Options defining how to move the configuration.
		/// </summary>
		protected MoveConfigurationUpgradeRuleOptions Options { get; }

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
		{
			// Sanity.
			if (null == options)
				throw new ArgumentNullException(nameof(options));
			if (null == options.Source)
				throw new ArgumentException("The source location is null.", nameof(options));
			if (null == options.Target)
				throw new ArgumentException("The target location is null.", nameof(options));
			if (false == options.Source.IsValid())
				throw new ArgumentException($"The source location is invalid.");
			if (false == options.Target.IsValid())
				throw new ArgumentException($"The target location is invalid.");

			this.Options = options;
		}

		/// <inheritdoc />
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="vault"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="NamedValueStorageManager"/> is null.</exception>
		public bool Execute(Vault vault)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == this.NamedValueStorageManager)
				throw new InvalidOperationException("The named value storage manager cannot be null.");

			// Read the values from the source.
			var values = this.Options.Source.GetNamedValues(this.NamedValueStorageManager, vault);
			if (null == values)
				return false;

			// Set the values in the target.
			this.Options.Target.SetNamedValues(this.NamedValueStorageManager, vault, values);

			// Optionally remove the old value(s).
			if(this.Options.RemoveMovedValues)
			{
				this.Options.Source.RemoveNamedValues(this.NamedValueStorageManager, vault, values.Names.Cast<string>().ToArray());
			}

			// All done!
			return true;
		}

		/// <summary>
		/// Options for <see cref="MoveConfigurationUpgradeRule"/>.
		/// </summary>
		public class MoveConfigurationUpgradeRuleOptions
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
		}
	}
}
