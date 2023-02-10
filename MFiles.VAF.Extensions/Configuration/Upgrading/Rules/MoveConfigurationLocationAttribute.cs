using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class MoveConfigurationLocationAttribute
		: Attribute
	{
		protected ISingleNamedValueItem Source { get; set; }

		protected ISingleNamedValueItem Target { get; set; }

		protected MoveConfigurationLocationAttribute()
		{

		}

		public MoveConfigurationLocationAttribute
		(
			MFNamedValueType sourceNamedValueType,
			string sourceNamespace,
			string sourceName,
			MFNamedValueType targetNamedValueType,
			string targetNamespace,
			string targetName
		)
		{
			this.Source = new SingleNamedValueItem(sourceNamedValueType, sourceNamespace, sourceName);
			this.Target = new SingleNamedValueItem(targetNamedValueType, targetNamespace, targetName);
		}

		/// <summary>
		/// Whether to run this migration rule before any other conversion rules.
		/// If true, it is run before conversion rules.  If false it is run afterwards.
		/// </summary>
		public bool RunBeforeOtherUpgradeRules { get; set; } = true;

		/// <summary>
		/// Returns this as an upgrade rule that can be run.
		/// </summary>
		/// <param name="vaultApplication">The application that this is running in.</param>
		/// <returns>The upgrade rule, or null if none can be created.</returns>
		public virtual IUpgradeRule AsUpgradeRule(VaultApplicationBase vaultApplication)
		{
			return new MoveConfigurationUpgradeRule
			(
				this.Source,
				this.Target,
				new Version("0.0"),
				new Version("0.0")
			);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Migrating configuration from {this.Source} to {this.Target}.";
		}
	}
}
