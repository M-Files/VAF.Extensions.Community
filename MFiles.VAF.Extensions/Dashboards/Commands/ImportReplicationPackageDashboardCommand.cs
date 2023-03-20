using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.ClientDirective;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using MFilesAPI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
	public class ImportReplicationPackageDashboardCommand<TConfiguration>
		: CustomDomainCommand
		where TConfiguration : class, new()
	{
		/// <summary>
		/// The logger to use for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ImportReplicationPackageDashboardCommand<TConfiguration>));

		protected ConfigurableVaultApplicationBase<TConfiguration> VaultApplication { get; } 
		protected string ReplicationPackagePath { get; }

		public ImportReplicationPackageDashboardCommand
		(
			ConfigurableVaultApplicationBase<TConfiguration> vaultApplication,
			string commandId,
			string displayName,
			string replicationPackagePath
		)
		{
			this.ID = commandId;
			this.DisplayName = displayName;
			this.Execute = this.Import;
			this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
			var package = new FileInfo(replicationPackagePath);
			this.ReplicationPackagePath = package.FullName;
			if(false == File.Exists( this.ReplicationPackagePath ) )
			{
				throw new ArgumentException("The replication package does not exist.", nameof(ReplicationPackagePath));
			}
		}

		public virtual void Import
		(
			IConfigurationRequestContext context, 
			ClientOperations clientOperations
		)
		{
			this.Logger?.Trace($"Starting import of data at {this.ReplicationPackagePath}");

			// TODO: Disable polling during the upgrade.
			// this.VaultApplication.TaskManager.EnableTaskPolling(false);

			// Do the upgrade.
			this.Logger?.Trace($"Creating content import job {this.ReplicationPackagePath}");
			var job = this.CreateImportContentJob(out IDisposable disposable);
			using (disposable)
			{
				this.Logger?.Info($"Starting import of data at {this.ReplicationPackagePath}");
				this.ImportToVault(context.Vault, job);
			}
			this.Logger?.Debug($"Import of {this.ReplicationPackagePath} complete.");

			// TODO: Re-enable it after the upgrade.
			// this.TaskQueueManager.EnableTaskPolling(true);

			// TODO: Send a broadcast forcing all instances to refresh vault structure.
			this.VaultApplication.ReinitializeMetadataStructureCache(context.Vault);

			// Update the client.
			clientOperations.RefreshMetadataCache();
			clientOperations.ShowMessage("Structure imported");
			clientOperations.ReloadDomain();

		}

		protected virtual void ImportToVault
		(
			Vault vault,
			ImportContentJob job
		)
		{
			// Run the import job.
			try
			{
				vault.ManagementOperations.ImportContent(job);
			}
			catch (Exception)
			{
				// Exceptions from this method can expose server paths (#160640).
				// We show a simple error message instead.
				// Exceptions are typically already written to event log by M-Files,
				// so we don't bother to write it ourselves.
				this.Logger?.Info($"Exception importing job at {this.ReplicationPackagePath}");
				throw new Exception("Cannot import package");
			}
		}

		protected virtual ImportContentJob CreateImportContentJob(out IDisposable disposable)
		{
			var job = new ImportContentJob
			{
				Flags = MFImportContentFlag.MFImportContentFlagForceNoStructureIdUpdate |
					MFImportContentFlag.MFImportContentFlagOmitDoneFile,
				ActivateAutomaticPermissionsForNewOrChangedDefinitions = true,
				DisableImportedExternalObjectTypeConnections = true,
				DisableImportedExternalUserGroups = true,
				DisableImportedVaultEventHandlers = false,
				IgnoreAutomaticPermissionsDefinedByObjects = false,
				ResetExportTimestamps = false
			};
			job.DisableFeaturesRequiringSystemAdministratorRole();

			// If needed, unzip the file.
			var package = new FileInfo(this.ReplicationPackagePath);
			switch (package?.Extension?.ToLower()?.Trim())
			{
				case ".zip":
					// Unzip it to a temporary folder.
					var temporaryFolder = Path.GetTempPath() + package.Name.Substring(0, package.Name.Length - 4);
					disposable = new TemporaryDirectory(temporaryFolder);
					System.IO.Compression.ZipFile.ExtractToDirectory(package.FullName, temporaryFolder);

					// Find the path to the xml file.
					var xml = new DirectoryInfo(temporaryFolder)
						.GetDirectories()?
						.FirstOrDefault()?
						.GetFiles("index.xml")?
						.FirstOrDefault()?
						.FullName;
					job.SourceLocation = xml ?? package.FullName;

					break;
				case ".xml":
					disposable = new EmptyDisposable();
					job.SourceLocation = package.FullName;
					break;
				default:
					{
						// Allow to fail.
						disposable = new EmptyDisposable();
						break;
					}
			}

			return job;
		}
		internal class TemporaryDirectory : IDisposable
		{
			private ILogger Logger { get; } = LogManager.GetLogger(typeof(TemporaryDirectory));

			private bool disposedValue;

			public string Path { get; }
			public TemporaryDirectory(string path)
			{
				this.Path = path;
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						this.Logger?.Info($"Deleting {this.Path}");
						Directory.Delete(this.Path, true);
					}

					// TODO: free unmanaged resources (unmanaged objects) and override finalizer
					// TODO: set large fields to null
					disposedValue = true;
				}
			}

			// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
			// ~TemporaryDirectory()
			// {
			//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			//     Dispose(disposing: false);
			// }

			public void Dispose()
			{
				// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}
		}
		internal class EmptyDisposable : IDisposable
		{
			public void Dispose() { }
		}
	}
	internal static partial class ImportContentJobExtensionMethods
	{
	}
}
