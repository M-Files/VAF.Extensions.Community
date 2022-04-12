using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Interfaces.Domain;
using MFiles.VaultApplications.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
	public class ShowSelectLogDownloadDashboardCommand
		: DefaultLogDashboardCommandBase
	{
		/// <summary>
		/// The logger for this command.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ShowSelectLogDownloadDashboardCommand));

		/// <summary>
		/// The ID for the command to show the dashboard that allows log selection.
		/// </summary>
		public static string CommandId = "__ShowSelectLogDownloadDashboard";

		/// <summary>
		/// Location of the dashboard file as an embedded resource.
		/// </summary>
		private const string DashboardResourceId =
				"MFiles.VAF.Extensions.Resources.DefaultLogDownloadDashboard.html";

		private ShowSelectLogDownloadDashboardCommand() { }

		public static ShowSelectLogDownloadDashboardCommand Create()
		{
			var command = new ShowSelectLogDownloadDashboardCommand
			{
				ID = CommandId,
				DisplayName = "Download Logs",
				Locations = new List<ICommandLocation> { new DomainMenuCommandLocation() }
			};
			command.Execute = command.ShowLogSelectionDashboard;
			return command;
		}

		/// <summary>
		/// Launches the status dashboard in an MFAdmin modal dialog.
		/// </summary>
		/// <param name="context">The request context.</param>
		/// <param name="clientOps">Client operations.</param>
		private void ShowLogSelectionDashboard(
			IConfigurationRequestContext context,
			ClientOperations clientOps
		)
		{
			// Check if there are any logs to download.
			List<LogFileInfo> logFiles = ResolveLogFiles();
			if (logFiles.Count == 0)
			{
				// Show a message that there are no files to download and be done.
				clientOps.ShowMessage("There are no log files available for download.");
				return;
			}

			// Resolve the assembly that contains the dashboard template.
			string template = null;

			// Load the template from embedded resources.
			Assembly assembly = Assembly.GetExecutingAssembly();
			using (Stream s = assembly.GetManifestResourceStream(DashboardResourceId))
			{
				using (var reader = new StreamReader(s))
				{
					template = reader.ReadToEnd();
				}
			}

			// Inject the log file list into the dashboard template.
			string logFilesJson = JsonConvert.SerializeObject(
					logFiles.OrderByDescending(f => f.RelativePath));
			var downloadMethod = clientOps.Manager.CreateCommandMethodSource(
					clientOps.DefaultNodeLocation, DownloadSelectedLogsDashboardCommand.CommandId);
			string downloadMethodJson = JsonConvert.SerializeObject(downloadMethod);
			string dashboard = template
					.Replace("%LOG_FILES_DATA%", logFilesJson)
					.Replace("%DOWNLOAD_METHOD%", downloadMethodJson);

			// Show the dashboard.
			clientOps.Directives.Add(new VAF.Configuration.Domain.ClientDirective.ShowModalDashboard { Content = dashboard });
		}
	}
}
