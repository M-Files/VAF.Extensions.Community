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
		[TestMethod]
		public void GetRebroadcastQueueIdCorrect()
		{
			Assert.AreEqual
			(
				"MFiles-VAF-Extensions-Tests-MultiServerMode-ConfigurableVaultApplicationBaseProxy-ConfigurationRebroadcastQueue",
				new ConfigurableVaultApplicationBaseProxy().GetRebroadcastQueueId()
			);
		}

		[TestMethod]
		public void ConfigurationRebroadcastTaskProcessor_IsPopulatedByRegisterTaskQueues()
		{
			var vaultApplication = new ConfigurableVaultApplicationBaseProxy()
			{
				PermanentVault = this.GetVaultMock().Object
			};
			Assert.IsNull(vaultApplication.ConfigurationRebroadcastTaskProcessor);
			vaultApplication.RegisterTaskQueues();
			Assert.IsNotNull(vaultApplication.ConfigurationRebroadcastTaskProcessor);
		}

		/// <inheritdoc />
		protected override Mock<Vault> GetVaultMock()
		{
			var vaultMock = base.GetVaultMock();

			// Set up the server attachments.
			var vaultServerAttachments = new VaultServerAttachments();
			//{
			//	{
			//		-1,
			//		new VaultServerAttachment()
			//		{
			//			IsCurrent = true,
			//			Address = "127.0.0.1",
			//			HostName = "localhost",
			//			ServerID = Guid.NewGuid().ToString( "B" ),
			//			LatestTimestamp = new Timestamp()
			//		}
			//	}
			//};
			vaultMock.Setup(v => v.GetVaultServerAttachments())
				.Returns(vaultServerAttachments);

			return vaultMock;
		}
	}

	public class VaultServerAttachment
		: MFilesAPI.VaultServerAttachment
	{

		#region Implementation of IVaultServerAttachment

		/// <inheritdoc />
		public MFilesAPI.VaultServerAttachment Clone()
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public string HostName { get; set; }

		/// <inheritdoc />
		public string Address { get; set; }

		/// <inheritdoc />
		public string ServerID { get; set; }

		/// <inheritdoc />
		public Timestamp LatestTimestamp { get; set; }

		/// <inheritdoc />
		public bool IsCurrent { get; set; }

		#endregion

	}

	public class ConfigurableVaultApplicationBaseProxy
		: MFiles.VAF.Extensions.MultiServerMode.ConfigurableVaultApplicationBase<object>
	{
		public new Vault PermanentVault
		{
			get => base.PermanentVault;
			set => base.PermanentVault = value;
		}
		public new AppTaskBatchProcessor ConfigurationRebroadcastTaskProcessor
		{
			get => base.ConfigurationRebroadcastTaskProcessor;
		}
		public new AppTaskBatchProcessor GetConfigurationRebroadcastTaskProcessor()
		{
			return base.GetConfigurationRebroadcastTaskProcessor();
		}
	}
}
