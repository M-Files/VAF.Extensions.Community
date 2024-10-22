using MFiles.VAF.Core;
using MFiles.VAF;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using MFiles.VAF.Configuration;
using System.Collections.Generic;
using MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution;
using System.Linq;

namespace MFiles.VAF.Extensions.Tests
{

	[TestClass]
	public partial class ConfigurableVaultApplicationBaseTests
		: TestBaseWithVaultMock
	{

		[TestMethod]
		public void GetCustomDomainCommandResolver_Default()
		{
			var proxy = new ConfigurableVaultApplicationBaseProxy<object>();
			var resolver = proxy.GetCustomDomainCommandResolver();
			Assert.IsNotNull(resolver);
			Assert.IsInstanceOfType(resolver, typeof(DefaultCustomDomainCommandResolver<object>));
		}

		[TestMethod]
		public void TaskQueueBackgroundOperationsManager_ReturnsCommands()
		{
			var proxy = new ConfigurableVaultApplicationBaseProxy<object>();

			var vaultMock = new Mock<Vault>();
			vaultMock
				.Setup(m => m.GetVaultServerAttachments())
				.Returns(() =>
				{
					var attachments = new VaultServerAttachments();
					attachments.Add(0, new VaultServerAttachment());
					return attachments;
				});
			vaultMock.SetupGet(m => m.ApplicationTaskOperations).Returns(Mock.Of<VaultApplicationTaskOperations>());

			// Create a background operation and set it up so the command should be returned.
			proxy.TaskManager = new TaskManagerEx<object>
			(
				proxy,
				"taskManager",
				vaultMock.Object,
				Mock.Of<IVaultTransactionRunner>()
			);
			var operation = proxy.TaskQueueBackgroundOperationManager.CreateBackgroundOperation("test", () => { });

			// Ensure that the domain resolver returns the command.
			var returnedCommands = proxy
					.GetCustomDomainCommandResolver()?
					.GetCustomDomainCommands()?
					.ToList() ?? new List<VAF.Configuration.AdminConfigurations.CustomDomainCommand>();
			Assert.IsTrue
			(
				returnedCommands?
					.Contains(operation.DashboardRunCommand) ?? false
			);

		}
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
		protected internal new TaskQueueBackgroundOperationManager<TSecureConfigurationType> TaskQueueBackgroundOperationManager
		{
			get { return base.TaskQueueBackgroundOperationManager; }
		}
		protected internal new TaskManagerEx<TSecureConfigurationType> TaskManager
		{
			get { return base.TaskManager; }
			set { base.TaskManager = value; }
		}
	}
	public class ConfigurableVaultApplicationBaseProxy
		: ConfigurableVaultApplicationBaseProxy<object>
	{
	}
}
