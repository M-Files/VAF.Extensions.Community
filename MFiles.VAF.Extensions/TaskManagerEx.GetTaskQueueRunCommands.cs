using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class TaskManagerEx<TConfiguration>
	{
		private object _lock = new object();
		protected Dictionary<string, CustomDomainCommand> TaskQueueRunCommands { get; }
			= new Dictionary<string, CustomDomainCommand>();

		/// <summary>
		/// Returns the commands associated with manually running task queue background operations.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<CustomDomainCommand> GetTaskQueueRunCommands(TaskQueueResolver taskQueueResolver)
		{
			if (null == taskQueueResolver)
				throw new ArgumentNullException(nameof(taskQueueResolver));

			// Get the task processors that have a ShowRunCommandInAttribute attribute.
			foreach (var queue in this.GetQueues())
			{
				foreach (var processor in queue.GetTaskProcessors())
				{
					CustomDomainCommand command = null;
					try
					{
						var methodInfo = taskQueueResolver.GetTaskProcessorMethodInfo(queue.Id, processor.Type);

						// Only return ones that are marked with the attribute.
						var showOnDashboardAttribute = methodInfo
							.GetCustomAttributes(typeof(ShowOnDashboardAttribute), true)?
							.FirstOrDefault() as ShowOnDashboardAttribute;
						if (null == showOnDashboardAttribute)
							continue;

						// Should we show the run command?
						if (!showOnDashboardAttribute.ShowRunCommand)
							continue;

						// Add it to the cache.
							var key = $"{queue.Id}-{processor.Type}";
						lock (_lock)
						{
							// Create the command if we need to.
							if (!this.TaskQueueRunCommands.ContainsKey(key))
							{
								command = new CustomDomainCommand()
								{
									ID = Guid.NewGuid().ToString("N"),
									ConfirmMessage = showOnDashboardAttribute.RunCommandConfirmationText
										?? ShowOnDashboardAttribute.DefaultRunCommandConfirmationText,
									DisplayName = showOnDashboardAttribute.RunCommandDisplayText
										?? ShowOnDashboardAttribute.DefaultRunCommandDisplayText,
									Blocking = true
								};
								command.Execute = (c, o) =>
								{
									// Cancel future executions?
									if (showOnDashboardAttribute.RunNowRecalculationType == RunNowRecalculationType.RecalculateFutureExecutions)
									{
										// Cancel any future executions.
										this.VaultApplication?.TaskManager?.CancelAllFutureExecutions(queue.Id, processor.Type);
									}

									// Make it run ASAP.
									this.AddTask(c.Vault, queue.Id, processor.Type);

									// Refresh the dashboard.
									if (false == string.IsNullOrEmpty(showOnDashboardAttribute.RunCommandSuccessText))
										o.ShowMessage(showOnDashboardAttribute.RunCommandSuccessText);
									o.RefreshDashboard();
								};
								this.TaskQueueRunCommands.Add(key, command);
							}
							else
							{
								command = this.TaskQueueRunCommands[key];
							}
						}
					}
					catch { }

					// Return the command if we can.
					if (null != command)
						yield return command;
				}
			}
		}
	}
}
