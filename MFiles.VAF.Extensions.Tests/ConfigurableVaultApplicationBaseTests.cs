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
