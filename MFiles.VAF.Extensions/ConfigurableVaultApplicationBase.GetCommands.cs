// ReSharper disable once CheckNamespace
using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.MultiServerMode;
using MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		/// <inheritdoc />
		public override IEnumerable<CustomDomainCommand> GetCommands(IConfigurationRequestContext context)
		{
			// Return the base commands, if any.
			foreach (var c in base.GetCommands(context) ?? new CustomDomainCommand[0])
				yield return c;

			// Return our commands.
			foreach (var c in this.GetTaskQueueBackgroundOperationRunCommands())
				yield return c;
		}

		/// <summary>
		/// Returns the commands associated with manually running task queue background operations.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<CustomDomainCommand> GetTaskQueueBackgroundOperationRunCommands()
		{
			// Create a list for our items.
			var list = new List<Tuple<TaskQueueBackgroundOperation, ShowRunCommandOnDashboardAttribute[]>>();

			// Get the background operations that have been explicitly marked as runnable.
			var taskQueueBackgroundOperationManagers = this
				.GetType()
				.GetPropertiesAndFieldsOfType<TaskQueueBackgroundOperationManager>(this);
			foreach (var m in taskQueueBackgroundOperationManagers)
			{
				foreach (var bo in m.BackgroundOperations)
				{
					// If we don't have a background operation, or it's not been marked as runnable then skip.
					if (null == bo.Value?.BackgroundOperation)
						continue;
					if (false == (bo.Value.BackgroundOperation.DashboardDisplayOptions?.ShowRunCommandInDashboard ?? false))
						continue;

					// Add this background operation to the list.
					list.Add
					(
						new Tuple<TaskQueueBackgroundOperation, ShowRunCommandOnDashboardAttribute[]>
						(
							bo.Value.BackgroundOperation,
							new[]
							{
								// Create an attribute for us to use.
								new ShowRunCommandOnDashboardAttribute()
								{
									ButtonText = bo.Value.BackgroundOperation.DashboardDisplayOptions.RunCommandText
										?? BackgroundOperationDashboardDisplayOptions.DefaultRunCommandText,
									Message = bo.Value.BackgroundOperation.DashboardDisplayOptions.RunCommandMessageText
										?? BackgroundOperationDashboardDisplayOptions.DefaultRunCommandMessageText,
								}
							}
						)
					);
				}
			}

			// Get the background operations that have been marked as runnable via an attribute.
			list.AddRange
			(
				this.GetType()
					.GetPropertiesAndFieldsOfTypeWithAttribute<TaskQueueBackgroundOperation, ShowRunCommandOnDashboardAttribute>
					(
					this
					)
					.AsEnumerable()
			);

			// Set up the commands.
			foreach (var tuple in list.DistinctBy(t => t.Item1.ID))
			{
				// Load the data and die if not valid.
				var backgroundOperation = tuple.Item1;
				if (null == backgroundOperation)
					continue;
				var attribute = tuple.Item2.FirstOrDefault();
				if (null == attribute)
					continue;

				// Set up the command.
				var command = new CustomDomainCommand()
				{
					ID = $"cmdRunBackgroundOperation-{backgroundOperation.ID.ToString("N")}",
					DisplayName = attribute.ButtonText,
					Execute = (c, o) =>
					{
						// Try and run the background operation.
						backgroundOperation?.RunOnce();

						// Refresh the dashboard.
						if(false == string.IsNullOrEmpty(attribute.Message))
							o.ShowMessage(attribute.Message);
						o.RefreshDashboard();
					}
				};

				// Store a reference to the command on the background operation.
				// This allows us to then easily output a button for it on the dashboard.
				backgroundOperation.RunCommand = command;
				yield return command;
			}

		}

	}
}
