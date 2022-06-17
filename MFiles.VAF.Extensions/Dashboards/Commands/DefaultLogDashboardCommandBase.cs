using MFiles.VAF.Common;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
	public abstract class DefaultLogDashboardCommandBase
		: CustomDomainCommand
	{
		/// <summary>
		/// View model for file information in the download dashboard.
		/// </summary>
		[DataContract]
		public class LogFileInfo
		{
			/// <summary>
			/// Relative path of the log file.
			/// </summary>
			[DataMember(Name = "relativePath")]
			public string RelativePath { get; set; }

			/// <summary>
			/// Size of the log file.
			/// </summary>
			[DataMember(Name = "size")]
			public long Size { get; set; }

			/// <summary>
			/// When the log file was created.
			/// </summary>
			[DataMember(Name = "created")]
			public DateTime Created { get; set; }

			/// <summary>
			/// When the log file was last written to.
			/// </summary>
			[DataMember(Name = "lastWrite")]
			public DateTime LastWrite { get; set; }
		}

		/// <summary>
		/// Helper method.
		/// Returns the application's default logging dir, as defined by the environment.
		/// </summary>
		/// <param name="vault">The vault used for verifying valid root paths in some cases.</param>
		/// <returns>The logging path.</returns>
		internal static string ResolveRootLogPath(Vault vault)
		{
			// If the root path is specified as an environment variable, use it and be done.
			string envRootPath = Environment.GetEnvironmentVariable(
					"MFAPPLOGDIRROOT", EnvironmentVariableTarget.Process);
			if (!string.IsNullOrWhiteSpace(envRootPath))
				return envRootPath;

			// The root path was not specified.
			// Try to infer it from the current log dir.
			string envLogPath = Environment.GetEnvironmentVariable("MFAPPLOGDIR", EnvironmentVariableTarget.Process);
			if (!string.IsNullOrWhiteSpace(envLogPath) && Path.IsPathRooted(envLogPath))
			{
				// Verfiy the log path matches our assumptions or else it is too dangerous to infer the root from.
				// We assume the path ends with "\Applogs\<app-guid>\<instance-guid>\<number>\".
				// Guids are in uppercase with dashes and no curly braces.
				string appGuid = ApplicationDefinition.Guid.ToString("D").ToUpper();
				string instanceGuid = Guid.Parse(input: vault.GetVaultServerAttachments().GetCurrent().ServerID)
						.ToString("D").ToUpper();
				var regex = new Regex($@"\\Applogs\\{appGuid}\\{instanceGuid}\\\d+\\$");
				if (regex.IsMatch(envLogPath))
				{
					// Path matches our assumptions.
					// Return the root path as the appGuid directory (2 levels back).
					return Path.GetFullPath(Path.Combine(envLogPath, "..//.."));
				}
			}

			// No valid root path was found.
			return null;
		}

		/// <summary>
		/// Provides a list of all log files and their sizes relative to the root log path.
		/// </summary>
		/// <returns>File list.</returns>
		public List<LogFileInfo> ResolveLogFiles(Vault vault)
		{
			var rootPath = ResolveRootLogPath(vault);

			// Sanity check.
			if (string.IsNullOrWhiteSpace(rootPath))
				throw new Exception("The server environment does not support downloading application logs.");

			// Collect all log files.
			var files = new List<LogFileInfo>();
			foreach (string filePath in Directory.GetFiles(rootPath, "*.log", SearchOption.AllDirectories))
			{
				// Include info about this log file.
				var file = new FileInfo(filePath);
				file.Refresh();  // Seeing stale sizes in cloud.
				var fileInfo = new LogFileInfo
				{
					Size = file.Length,
					RelativePath = filePath.Substring(rootPath.Length).Trim('\\'),
					Created = file.CreationTimeUtc,
					LastWrite = file.LastWriteTimeUtc
				};
				files.Add(fileInfo);
			}
			return files;
		}


	}
}
