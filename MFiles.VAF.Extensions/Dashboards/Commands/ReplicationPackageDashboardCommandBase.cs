using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using MFilesAPI.Extensions;
using System;
using System.IO;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
	public abstract class ReplicationPackageDashboardCommandBase
		: CustomDomainCommand
	{

		/// <summary>
		/// The logger to use for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ReplicationPackageDashboardCommandBase));

		/// <summary>
		/// The path to the replication package.
		/// </summary>
		public string ReplicationPackagePath { get; }

		/// <summary>
		/// Whether this package needs to be imported.
		/// Should be calculated somehow.
		/// </summary>
		// TODO: Make this calculated.
		public bool RequiresImporting { get; set; }

		/// <summary>
		/// The vault application for this command.
		/// </summary>
		protected VaultApplicationBase VaultApplication { get; }

		/// <summary>
		/// Creates a command which, when clicked, will import a replication package.
		/// </summary>
		/// <param name="vaultApplication">The vault application for this command.</param>
		/// <param name="commandId">The ID of this command - must be unique within the application.</param>
		/// <param name="displayName">What to display for this command, in context menus etc.</param>
		/// <param name="replicationPackagePath">The path to the replication package.  Can either be to a .zip file or to the index.xml file of a ready-extracted package.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="vaultApplication"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="replicationPackagePath"/> does not exist.</exception>
		public ReplicationPackageDashboardCommandBase
		(
			VaultApplicationBase vaultApplication,
			string commandId,
			string displayName,
			string replicationPackagePath
		)
		{
			this.ID = commandId;
			this.DisplayName = displayName;

			this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
			var package = new FileInfo(replicationPackagePath);
			this.ReplicationPackagePath = package.FullName;
			if (false == File.Exists(this.ReplicationPackagePath))
			{
				throw new ArgumentException("The replication package does not exist.", nameof(ReplicationPackagePath));
			}
		}

		/// <summary>
		/// Creates the import job.
		/// </summary>
		/// <param name="disposable">An item that should be disposed when the job is finished importing.</param>
		/// <returns>The job.</returns>
		protected virtual ImportContentJob CreateImportContentJob(out IDisposable disposable)
		{
			this.Logger?.Info($"Creating import job");
			var job = new ImportContentJob
			{
				Flags = MFImportContentFlag.MFImportContentFlagForceNoStructureIdUpdate |
					MFImportContentFlag.MFImportContentFlagOmitDoneFile,
				ActivateAutomaticPermissionsForNewOrChangedDefinitions = true,
				DisableImportedExternalObjectTypeConnections = true,
				DisableImportedExternalUserGroups = true,
				DisableImportedVaultEventHandlers = true,
				DisableCustomCodeInImportedPropertyDefinitions= true,
				DisableCustomCodeInImportedWorkflows = true,
				AllowSignedCustomCodeInImport = true,
				IgnoreAutomaticPermissionsDefinedByObjects = false,
				ResetExportTimestamps = false
			};
			job.DisableFeaturesRequiringSystemAdministratorRole();

			// If needed, unzip the file.
			var package = new FileInfo(this.ReplicationPackagePath);
			switch (package?.Extension?.ToLower()?.Trim())
			{
				case ".zip":
					this.Logger?.Debug($"Replication package path is a zip file; extracting");
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
						.GetFiles("index.xml")?
						.FirstOrDefault()?
						.FullName
						?? fileDownloadLocation?
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
					this.Logger?.Debug($"Replication package path is an xml file.");
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
