using MFiles.VAF.Core;
using MFiles.VAF;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MFiles.VAF.Extensions.Tests
{
	[TestClass]
	public class ConfigurableVaultApplicationBaseTests
		: TestBaseWithVaultMock
	{
		// Commented out whilst tracker item 156295 is resolved.
		//[TestMethod]
		//public void GetRebroadcastQueueIdCorrect()
		//{
		//	Assert.AreEqual
		//	(
		//		"MFiles-VAF-Extensions-Tests-ConfigurableVaultApplicationBaseProxy-ConfigurationRebroadcastQueue",
		//		new ConfigurableVaultApplicationBaseProxy()
		//		{
		//			PermanentVault = this.GetVaultMock().Object
		//		}.GetRebroadcastQueueId()
		//	);
		//}
	}

	public class ConfigurableVaultApplicationBaseProxy
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<object>
	{
		public new Vault PermanentVault
		{
			get => base.PermanentVault;
			set => base.PermanentVault = value;
		}
	}
}
