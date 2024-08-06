using MFiles.VAF.AdminConfigurations;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.ClientDirective;
using MFiles.VAF.Configuration.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{

	/// <summary>
	/// The command that's used to download the selected logs.
	/// </summary>
	public class DownloadSelectedLogsDashboardCommand
		: DefaultLogDashboardCommandBase
	{
		/// <summary>
		/// The logger for this command.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(DownloadSelectedLogsDashboardCommand));

		/// <summary>
		/// The ID for the command to download the selected logs.
		/// </summary>
		public static string CommandId = "__DownloadSelectedLogs";

		private DownloadSelectedLogsDashboardCommand() { }

		public static DownloadSelectedLogsDashboardCommand Create()
		{
			// Create the download logs command.
			var command = new DownloadSelectedLogsDashboardCommand
			{
				ID = CommandId,
                DisplayName = Resources.Dashboard.Logging_Table_DownloadLogs
            };
			command.Execute = command.DownloadLogs;
			return command;
		}
		
		/// <summary>
		/// Prompts the user to download zip file containing selected log files.
		/// </summary>
		/// <param name="context">The request context.</param>
		/// <param name="clientOps">Client operations.</param>
		private void DownloadLogs(
			IConfigurationRequestContext context,
			ClientOperations clientOps
		)
		{
			// Resolve the files to download from the input params.
			var envContext = (EventHandlerRequestContext)context;
			int methodIndex = envContext.Environment.InputParams.FindIndex(a => a == CommandId);
			string[] files = envContext.Environment.InputParams.Skip(methodIndex + 1).ToArray();

			// Provide all files if no specific files were named.
			if (files.Length == 0)
				files = ResolveLogFiles(context.Vault).Select(f => f.RelativePath).ToArray();
			if (files.Length == 0)
				throw new FileNotFoundException();
			string rootPath = ResolveRootLogPath(context.Vault);

			// Create a temporary zip file containing the logs in memory,
			// that we can then push to the user to download.
			string base64Content = null;
			var fileSizes = new List<string>();
			long totalSize = 0;
			long archiveSize = 0;
			int inaccessibleFileCount = 0;
			using (var memStream = new MemoryStream())
			{
				// Build the archive.
				using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, true))
				{
					// Add each log file to the archive.
					foreach (string relPath in files)
					{
						ZipArchiveEntry fileEntry = null;
						string fullPath = null;
						try
						{
							// Sanity and safety check.
							// All paths must be inside the log folder.
							fullPath = Path.GetFullPath(Path.Combine(rootPath, relPath));
							if (!fullPath.StartsWith(rootPath) ||
								Path.GetExtension(fullPath).ToLower() != ".log" ||
								!File.Exists(fullPath))
								throw new FileNotFoundException();

							// Add the file to the archive.
							// NOTE:
							//   We can't use archive.CreateEntryFromFile, because the log file may be
							//   open by the logger, and the method doesn't have options for how we open
							//   the file. To avoid issues, we must open the file with FileShare.ReadWrite.
							using (FileStream fileStream = File.Open(
									fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
							{
								fileEntry = archive.CreateEntry(relPath, CompressionLevel.Optimal);
								using (Stream entryStream = fileEntry.Open())
								{
									// Copy the file to the archive.
									fileStream.CopyTo(entryStream);

									// Bookkeeping.
									var fileSize = fileStream.Length;
									fileSizes.Add($"{relPath}: {fileSize} bytes");
									totalSize += fileSize;
								}
							}
						}
						catch (Exception e)
						{
							// The file was likely inaccessible.  This can happen in some situations
							// where the container is recycled and the file is locked.
							inaccessibleFileCount++;

							this.Logger.Error(e, $"Cannot add {relPath} to the log download.");

							// Ensure we don't have a value in the zip for this file.
							try { fileEntry?.Delete(); } catch { }
						}
					}
				}

				// Convert the memory stream of the archive to a base64 string.
				base64Content = Convert.ToBase64String(memStream.GetBuffer(), 0, (int)memStream.Length);
				archiveSize = memStream.Length;
			}

			// Show details before prompting to save (for debug).
			/*
			clientOps.ShowMessage( $"Total File Size: {totalSize} bytes\n\n" +
					$"Archive Size: {archiveSize} bytes\n\n" +
					$"Base64 Length: {base64Content.Length}\n\n" +
					$"Files:\n" +
					String.Join( "\n - ", fileSizes ) );
			*/

			// If some files weren't accessible then inform the user.
			if(inaccessibleFileCount > 0)
			{
				clientOps.ShowMessage
				(
					string.Format(Resources.Exceptions.Dashboard.LogFileDownloadIncomplete, inaccessibleFileCount)
				);
			}

			// Prompt the user to download the archive.
			clientOps.Directives.Add(new PromptDownloadFile
			{
				Name = $"{context.Vault.Name}_Logs_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}Z.zip",
				Content = base64Content,
				Base64Encoded = true,
				Filters = "Zip files (*.zip)|*.zip"
			});

		}
	}
}
