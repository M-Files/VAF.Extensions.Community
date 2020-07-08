using MFiles.VAF.Core;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MFiles.VAF.Extensions.Tests.MultiServerMode
{
	[TestClass]
	public class ConfigurableVaultApplicationBaseTests
		: TestBaseWithVaultMock
	{
		//[TestMethod]
		//public void GetRebroadcastQueueIdCorrect()
		//{
		//	Assert.AreEqual
		//	(
		//		"MFiles-VAF-Extensions-Tests-MultiServerMode-ConfigurableVaultApplicationBaseProxy-ConfigurationRebroadcastQueue",
		//		new ConfigurableVaultApplicationBaseProxy()
		//		{
		//			PermanentVault = this.GetVaultMock().Object
		//		}.GetRebroadcastQueueId()
		//	);
		//}
	}

	public class ConfigurableVaultApplicationBaseProxy
		: MFiles.VAF.Extensions.MultiServerMode.ConfigurableVaultApplicationBase<object>
	{
		public new Vault PermanentVault
		{
			get => base.PermanentVault;
			set => base.PermanentVault = value;
		}
	}
}
