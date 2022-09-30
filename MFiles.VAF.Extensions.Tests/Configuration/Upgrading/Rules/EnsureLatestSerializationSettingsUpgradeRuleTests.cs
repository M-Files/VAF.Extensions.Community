using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	[TestClass]
	public partial class EnsureLatestSerializationSettingsUpgradeRuleTests
		: TestBaseWithVaultMock
	{
		internal class EnsureLatestSerializationSettingsUpgradeRuleProxy
			: Extensions.Configuration.Upgrading.Rules.EnsureLatestSerializationSettingsUpgradeRule<EnsureLatestSerializationSettingsUpgradeRuleProxy.MyConfiguration>
		{

			public EnsureLatestSerializationSettingsUpgradeRuleProxy()
				: base(Mock.Of<ISingleNamedValueItem>(m => m.IsValid() == true))
			{
				NamedValueStorageManager = new Mock<INamedValueStorageManager>().Object;
			}

			public class MyConfiguration
			{

			}

		}
	}
}
