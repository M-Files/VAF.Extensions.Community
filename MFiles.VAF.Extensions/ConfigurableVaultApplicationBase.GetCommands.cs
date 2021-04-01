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
			// Get the background operation and attributes.
			var items = this.GetType()
				.GetPropertiesAndFieldsOfTypeWithAttribute<TaskQueueBackgroundOperation, ShowRunCommandOnDashboardAttribute>
				(
				this
				);

			// Set up the commands.
			foreach (var tuple in items)
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
					ID = $"cmdRunBackgroundOperation-{backgroundOperation.Name.GetHashCode()}",
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
