using MFiles.VaultApplications.Logging;
using MFilesAPI;
using System;
using System.Linq;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	public class MoveConfigurationUpgradeRule
		: SingleNamedValueItemUpgradeRuleBase
	{
		public MoveConfigurationUpgradeRule(ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo)
			: base(readFrom, writeTo)
		{
		}

		private ILogger Logger { get; } = LogManager.GetLogger<MoveConfigurationUpgradeRule>();

		/// <inheritdoc />
		/// <remarks>This method does not make any changes to the content.</remarks>
		protected override string Convert(string input)
			=> input;
	}
}
