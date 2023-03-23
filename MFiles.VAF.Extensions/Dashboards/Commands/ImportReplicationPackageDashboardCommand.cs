using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.ClientDirective;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Extensions.Directives;
using MFilesAPI;
using MFilesAPI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
	public class ImportReplicationPackageDashboardCommand<TConfiguration>
		: CustomDomainCommand
		where TConfiguration : class, new()
	{
		private int maximumImportAttempts = 10;

		/// <summary>
		/// The logger to use for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ImportReplicationPackageDashboardCommand<TConfiguration>));

		/// <summary>
		/// The vault application for this command.
		/// </summary>
		protected ConfigurableVaultApplicationBase<TConfiguration> VaultApplication { get; }

		/// <summary>
		/// The path to the replication package.
		/// </summary>
		public string ReplicationPackagePath { get; }

		/// <summary>
		/// The number of import attempts of the package before it gives up.
		/// </summary>
		public int MaximumImportAttempts 
		{ 
			get => maximumImportAttempts;
			set
			{
				if (value <= 0 || value > 100)
					value = 10;
				maximumImportAttempts = value;
			}
		}
		/// <summary>
		/// The display name for the task.
		/// </summary>
		public string TaskDisplayName { get; set; } = "Import missing vault structure";

		/// <summary>
		/// Whether this package needs to be imported.
		/// Should be calculated somehow.
		/// </summary>
		// TODO: Make this calcualted.
		public bool RequiresImporting { get; set; }

		/// <summary>
		/// Creates a command which, when clicked, will import a replication package.
		/// </summary>
		/// <param name="vaultApplication">The vault application for this command.</param>
		/// <param name="commandId">The ID of this command - must be unique within the application.</param>
		/// <param name="displayName">What to display for this command, in context menus etc.</param>
		/// <param name="replicationPackagePath">The path to the replication package.  Can either be to a .zip file or to the index.xml file of a ready-extracted package.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="vaultApplication"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="replicationPackagePath"/> does not exist.</exception>
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

			// When it's executed, run the import in a transaction.
			this.Execute = (c, o) =>
			{
				var runner = this.VaultApplication.GetTransactionRunner();
				runner.Run((v) =>
				{
					this.CreateImportTask(c, o, v);
				});
			};
			this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
			var package = new FileInfo(replicationPackagePath);
			this.ReplicationPackagePath = package.FullName;
			if (false == File.Exists(this.ReplicationPackagePath))
			{
				throw new ArgumentException("The replication package does not exist.", nameof(ReplicationPackagePath));
			}
		}

		public virtual bool TryImport(Vault vault)
		{
			this.Logger?.Trace($"Starting import of data at {this.ReplicationPackagePath}");

			var previousConcurrency = this.VaultApplication.TaskManager.MaxConcurrency;
			try
			{
				// Disable polling during the upgrade.
				this.VaultApplication.TaskManager.MaxConcurrency = 0;

				// Do the upgrade.
				this.Logger?.Trace($"Creating content import job {this.ReplicationPackagePath}");
				var job = this.CreateImportContentJob(out IDisposable disposable);
				using (disposable ?? new EmptyDisposable())
				{
					this.Logger?.Info($"Starting import of data at {this.ReplicationPackagePath}");
					this.ImportToVault(vault, job);
					disposable.Dispose();
				}
				this.Logger?.Debug($"Import of {this.ReplicationPackagePath} complete.");

				// Refresh the metadata cache on all servers.
				this.VaultApplication.ReinitializeMetadataStructureCache(this.VaultApplication.PermanentVault);
				this.VaultApplication.TaskManager.SendBroadcast
				(
					vault,
					ReinitializeMetadataCacheTaskDirective.TaskType,
					// This directive isn't needed, but shown to identify the expected
					// task directive type, if needed in the future.
					new ReinitializeMetadataCacheTaskDirective()
				);

				// Mark us as not needing to run.
				this.RequiresImporting = false;
				return true;
			}
#if DEBUG
			catch (Exception e)
			{
				this.Logger?.Error(e, $"Exception importing package");
				return false;
			}
#else
			catch (Exception e)
			{
				this.Logger?.Error($"Exception importing package; exception details hidden from log");
				return false;
			}
#endif
			finally
			{
				// Set the concurrency back.
				this.VaultApplication.TaskManager.MaxConcurrency = previousConcurrency;
			}
		}

		public virtual void CreateImportTask
		(
			IConfigurationRequestContext context,
			ClientOperations clientOperations,
			Vault transactionalVault = null
		)
		{
			// Do we have any future or current execution?
			var executions = this.VaultApplication.TaskManager.GetExecutions<ImportReplicationPackageTaskDirective>
			(
				this.VaultApplication.GetExtensionsSequentialQueueID(),
				this.VaultApplication.GetReplicationPackageImportTaskType(),
				MFTaskState.MFTaskStateWaiting,
				MFTaskState.MFTaskStateInProgress
			).ToArray();
			if (executions.Length != 0)
			{
				clientOperations.ShowMessage("Thereis already an import scheduled or in progress.");
				clientOperations.RefreshDashboard();
				return;
			}

			// Add the task.
			this.VaultApplication.TaskManager.AddTask
			(
				transactionalVault ?? context.Vault,
				this.VaultApplication.GetExtensionsSequentialQueueID(),
				this.VaultApplication.GetReplicationPackageImportTaskType(),
				new ImportReplicationPackageTaskDirective()
				{
					CommandId = this.ID,
					DisplayName = this.TaskDisplayName
				}
			);

			// Show that we did something.
			clientOperations.ShowMessage(Resources.ImportReplicationPackage.TaskCreated);
			clientOperations.RefreshDashboard();

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
				throw new Exception(Resources.ImportReplicationPackage.Failure_Generic);
			}
		}

		/// <summary>
		/// Creates the import job.
		/// </summary>
		/// <param name="disposable">An item that should be disposed when the job is finished importing.</param>
		/// <returns>The job.</returns>
		protected virtual ImportContentJob CreateImportContentJob(out IDisposable disposable)
		{
			var job = new ImportContentJob
			{
				Flags = MFImportContentFlag.MFImportContentFlagForceNoStructureIdUpdate |
					MFImportContentFlag.MFImportContentFlagOmitDoneFile,
				ActivateAutomaticPermissionsForNewOrChangedDefinitions = true,
				DisableImportedExternalObjectTypeConnections = true,
				DisableImportedExternalUserGroups = true,
				DisableImportedVaultEventHandlers = true,
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
					var fileDownloadLocation = new FileDownloadLocation
					(
						Path.GetTempPath(),
						package.Name.Substring(0, package.Name.Length - 4)
					);
					System.IO.Compression.ZipFile.ExtractToDirectory
					(
						package.FullName,
						fileDownloadLocation.Directory.FullName
					);

					// Find the path to the xml file.
					var xml = fileDownloadLocation?
						.Directory?
						.GetDirectories()?
						.FirstOrDefault()?
						.GetFiles("index.xml")?
						.FirstOrDefault()?
						.FullName;
					job.SourceLocation = xml ?? package.FullName;

					// Our file download location now contains where to dispose.
					disposable = fileDownloadLocation;

					break;
				case ".xml":
					disposable = null;
					job.SourceLocation = package.FullName;
					break;
				default:
					throw new Exception("The package must be an .xml or .zip file.");
			}
			return job;
		}
		internal class EmptyDisposable : IDisposable
		{
			public void Dispose() { }
		}
	}
}
