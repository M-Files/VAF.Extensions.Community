using MFiles.VAF.Core;
using MFiles.VAF;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using MFiles.VAF.Configuration;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Tests
{

	[TestClass]
	public partial class ConfigurableVaultApplicationBaseTests
		: TestBaseWithVaultMock
	{

	}

	public class ConfigurableVaultApplicationBaseProxy<TSecureConfigurationType>
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<TSecureConfigurationType>
		where TSecureConfigurationType : class, new()
	{
		public new Vault PermanentVault
		{
			get => base.PermanentVault;
			set => base.PermanentVault = value;
		}
		public new VAF.Configuration.IConfigurationStorage ConfigurationStorage
		{
			get => base.ConfigurationStorage;
			set => base.ConfigurationStorage = value;
		}
	}
	public class ConfigurableVaultApplicationBaseProxy
		: ConfigurableVaultApplicationBaseProxy<object>
	{
	}
}
