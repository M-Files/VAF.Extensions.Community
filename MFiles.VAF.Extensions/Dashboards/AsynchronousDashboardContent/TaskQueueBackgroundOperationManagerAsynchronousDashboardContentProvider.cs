using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VaultApplications.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent
{
	/// <summary>
	/// Provides content from the <see cref="TaskQueueBackgroundOperationManager{TSecureConfiguration}"/> to
	/// be subsequently rendered on a dashboard.
	/// </summary>
	/// <typeparam name="TConfiguration">The type of configuration</typeparam>
	public class TaskQueueBackgroundOperationManagerAsynchronousDashboardContentProvider<TConfiguration>
		: IAsynchronousDashboardContentProvider
		where TConfiguration : class, new()
	{
		private ILogger Logger { get; }
			= LogManager.GetLogger<TaskQueueBackgroundOperationManagerAsynchronousDashboardContentProvider<TConfiguration>>();

		public ConfigurableVaultApplicationBase<TConfiguration> VaultApplication { get; protected set; }
		public TaskQueueBackgroundOperationManagerAsynchronousDashboardContentProvider(ConfigurableVaultApplicationBase<TConfiguration> vaultApplication)
		{
			this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		/// <inheritdoc />
		public IEnumerable<KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>> GetAsynchronousDashboardContent()
			=>
				this.VaultApplication.GetType()
				.GetPropertiesAndFieldsOfType<TaskQueueBackgroundOperationManager<TConfiguration>>(this.VaultApplication)
				.SelectMany(m => GetAsynchronousDashboardContent(m));

		/// <summary>
		/// Returns data from <paramref name="backgroundOperationManager"/> about asynchronous operations that should be rendered onto a dashboard.
		/// </summary>
		/// <returns>The data, or null if none should be rendered.</returns>
		protected virtual IEnumerable<KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>> GetAsynchronousDashboardContent
		(
			TaskQueueBackgroundOperationManager<TConfiguration> backgroundOperationManager
		)
		{
			// Sanity.
			if (null == backgroundOperationManager)
				yield break;

			// Iterate over each one and return them.
			foreach (var bgo in backgroundOperationManager.BackgroundOperations?.Values?.OrderBy(o => o.DashboardSortOrder))
			{
				// Sanity.
				if (null == bgo || false == bgo.ShowBackgroundOperationInDashboard)
					continue;

				// Add the run command, if we should/can.
				var commands = new List<DashboardCommand>();
				if (bgo.ShowRunCommandInDashboard)
				{
					commands.Add
					(
						new DashboardDomainCommand
						{
							DomainCommandID = bgo.DashboardRunCommand.ID,
							Title = bgo.DashboardRunCommand.DisplayName,
							Style = DashboardCommandStyle.Link
						}
					);
				}

				// Get all executions in the queue.
				var executions = bgo.GetAllExecutions().Select(ti => ti.Cast<TaskDirective>()).ToList();

				// Get the recurrence data.
				IRecurrenceConfiguration recurrenceConfiguration = null;
				switch (bgo.RepeatType)
				{
					case TaskQueueBackgroundOperationRepeatType.NotRepeating:
						recurrenceConfiguration = null; // Does not recur.
						break;
					case TaskQueueBackgroundOperationRepeatType.Interval:
						recurrenceConfiguration = new Frequency()
						{
							RecurrenceType = RecurrenceType.Interval,
							Interval = bgo.Interval
						};
						break;
					case TaskQueueBackgroundOperationRepeatType.Schedule:
						recurrenceConfiguration = new Frequency()
						{
							RecurrenceType = RecurrenceType.Schedule,
							Schedule = bgo.Schedule
						};
						break;
					default:
						Logger?.Warn($"{string.Format(Resources.AsynchronousOperations.RepeatType_UnhandledRepeatType, bgo.RepeatType)}");
						break;
				}

				// Return the execution data.
				yield return new KeyValuePair<DashboardQueueAndTaskDetails, IEnumerable<TaskInfo<TaskDirective>>>
				(
					new DashboardQueueAndTaskDetails()
					{
						QueueId = backgroundOperationManager.QueueId,
						TaskType = TaskQueueBackgroundOperation<TConfiguration>.TaskTypeId,
						Name = bgo.Name,
						Description = bgo.Description,
						Commands = commands,
						TasksInQueue = executions.Count,
						RecurrenceConfiguration = recurrenceConfiguration
					},
					executions
				);
			}
		}
	}
}
