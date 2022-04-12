using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

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
		}

		/// <summary>
		/// Helper method.
		/// Returns the applications default logging dir, as defined by the environment.
		/// </summary>
		/// <returns>The logging path.</returns>
		public string GetDefaultLogPath()
		{
			// Return the logging env var value.
			return Environment.GetEnvironmentVariable("MFAPPLOGDIR", EnvironmentVariableTarget.Process);
		}

		/// <summary>
		/// Gets the root path for all logs of this application,
		/// not just the current server and numbered folder.
		/// </summary>
		/// <returns>The root path.</returns>
		public string GetRootLogPathForAllInstances()
		{
			// Go back two levels.
			// 1st level: Running number - in case of folder locks on startup
			// 2nd level: Server Instance Id
			return Path.GetFullPath(Path.Combine(this.GetDefaultLogPath(), "..//.."));
		}

		/// <summary>
		/// Provides a list of all log files and their sizes relative to the root log path.
		/// </summary>
		/// <returns>File list.</returns>
		public List<LogFileInfo> ResolveLogFiles()
		{
			// Collect all log files.
			var files = new List<LogFileInfo>();
			var rootPath = GetRootLogPathForAllInstances();
			foreach (string filePath in Directory.GetFiles(rootPath, "*.log", SearchOption.AllDirectories))
			{
				// Include info about this log file.
				var file = new FileInfo(filePath);
				file.Refresh();  // Seeing stale sizes in cloud.
				var fileInfo = new LogFileInfo
				{
					Size = file.Length,
					RelativePath = filePath.Substring(rootPath.Length).Trim('\\'),
				};
				files.Add(fileInfo);
			}
			return files;
		}
	}
}
