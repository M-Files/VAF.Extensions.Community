using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.Targets;
using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using MFiles.VAF.Extensions.Dashboards.Commands;
using MFilesAPI;

namespace MFiles.VAF.Extensions.Dashboards.ReplicationPackageDashboardContent
{
	public class DashboardTableReplicationPackageDashboardContentRenderer<TSecureConfiguration>
		: IReplicationPackageDashboardContentRenderer<TSecureConfiguration>
		where TSecureConfiguration : class, new()
	{
		protected ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; }
		public DashboardTableReplicationPackageDashboardContentRenderer(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
		{
			this.VaultApplication = vaultApplication 
				?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		public virtual DashboardPanelEx GetDashboardContent
		(
			IConfigurationRequestContext context
		)
		{
			var taskManager = this.VaultApplication.TaskManager;
			var queueId = this.VaultApplication.GetExtensionsSequentialQueueID();
			var taskType = this.VaultApplication.GetReplicationPackageImportTaskType();

			// Get any pending/running tasks.
			var importTasks = taskManager.GetAllExecutions<ImportReplicationPackageTaskDirective>
			(
				queueId,
				taskType
			).ToArray();

			// Get any commands that need to be run.
			var commands = this.VaultApplication.GetCommands(null)?
				.Where(c => c is ImportReplicationPackageDashboardCommand<TSecureConfiguration>)
				.Cast<ImportReplicationPackageDashboardCommand<TSecureConfiguration>>()
				.Where(c => c.RequiresImporting)
				.ToArray();

			// If we have no data then show nothing.
			if (importTasks.Length == 0 && commands.Length == 0)
				return null;

			// Create the table
			var table = new DashboardTable();
			{
				var header = table.AddRow(DashboardTableRowType.Header);
				header.AddCell();
				header.AddCell("Current status");
				header.AddCell();
			}
			table.Styles.Add("background-color", "#f2f2f2");
			table.Styles.Add("padding", "10px");
			table.Styles.Add("margin", "16px 0px");

			// Render out a row for each command.
			foreach (var command in commands)
			{
				var row = table.AddRow();

				// Load the import status.
				var latestImportStatus = importTasks
					.Where(t => t.Directive.CommandId == command.ID)
					.OrderByDescending(t => t.LatestActivity)
					.FirstOrDefault();

				// Name/title.
				var name = new DashboardCustomContentEx($"{command.TaskDisplayName}");
				switch (latestImportStatus?.State)
				{
					case MFTaskState.MFTaskStateCompleted:
						name.Icon = "Resources/Images/Completed.png";
						break;
					case MFTaskState.MFTaskStateWaiting:
						name.Icon = "Resources/Images/Waiting.png";
						break;
					case MFTaskState.MFTaskStateInProgress:
						name.Icon = "Resources/Images/Running.png";
						break;
					case MFTaskState.MFTaskStateFailed:
					case MFTaskState.MFTaskStateCanceled:
						name.Icon = "Resources/Images/Failed.png";
						row.Styles.AddOrUpdate("color", Resources.Dashboard.Logging_ColorValidationErrors);
						break;
					case null:
						name.Icon = "Resources/Images/notenabled.png";
						break;
				}
				row.AddCell(name);

				// Render the status.
				if (latestImportStatus != null )
				{
					// Current import status.
					row.AddCell(new DashboardCustomContentEx($"{latestImportStatus.Status?.Details}"));
				}
				else
				{
					// Add in blank cells.
					row.AddCell("Not yet imported");
				}

				// Add the import command if none are waiting.
				if (latestImportStatus?.State != MFTaskState.MFTaskStateWaiting)
				{
					// Add whatever buttons, according to app configuration.
					var buttons = new DashboardContentCollection();

					buttons.Add(new DashboardDomainCommandEx()
					{
						DomainCommandID = command.ID,
						Title = command.DisplayName,
						Icon = "Resources/Images/ImportReplicationPackage.png"
					});

					// Add the buttons to the cell
					var cell = row.AddCell(buttons);
					cell.Styles.AddOrUpdate("text-align", "right");
				}
				else
				{
					row.AddCell("");
				}
			}

			// Return the panel.
			return 0 == table.Rows.Count(r => r.DashboardTableRowType == DashboardTableRowType.Body)
				? null
				: new DashboardPanelEx()
			{
				Title = "Import vault structure",
				InnerContent = new DashboardContentCollection
				{
					(IDashboardContent)table
				}
			};
		}

		IDashboardContent IReplicationPackageDashboardContentRenderer<TSecureConfiguration>.GetDashboardContent
		(
			IConfigurationRequestContext context
		)
			=> this.GetDashboardContent(context);
	}
}
