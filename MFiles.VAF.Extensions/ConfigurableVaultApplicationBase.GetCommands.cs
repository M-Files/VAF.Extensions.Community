using MFiles.VAF.Configuration.AdminConfigurations;
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

			// Return the command related to the background operation approach.
			foreach (var c in this.GetTaskQueueBackgroundOperationRunCommands())
				yield return c;

			// Return the commands related to the VAF 2.3+ attribute-based approach.
			foreach (var c in this.TaskManager.GetTaskQueueRunCommands(this.TaskQueueResolver))
				yield return c;

			// Return the commands associated with downloading logs from the default file target.
			foreach (var c in this.GetDefaultLogTargetDownloadCommands())
				yield return c;

			// Return the commands declared via attributes.
			foreach (var c in this.GetCustomDomainCommandsFromAttributes())
				yield return c;
		}

		/// <summary>
		/// Gets custom domain commands declared via attributes on methods.
		/// </summary>
		/// <returns>The commands.</returns>
		public virtual IEnumerable<CustomDomainCommand> GetCustomDomainCommandsFromAttributes()
			=> this.GetCustomDomainCommandsFromAttributes(this.GetType(), this);

		/// <summary>
		/// Gets custom domain commands declared via attributes on methods.
		/// </summary>
		/// <param name="type">The type to check for methods.</param>
		/// <param name="instance">The instance to use when calling the methods.</param>
		/// <returns>The commands.</returns>
		public virtual IEnumerable<CustomDomainCommand> GetCustomDomainCommandsFromAttributes
		(
			Type type,
			object instance = null
		)
		{
			// Sanity.
			if (null == type)
				yield break;

			// Check whether methods have the correct attributes
			var methods = type
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				?? Enumerable.Empty<MethodInfo>();
			foreach(var m in methods)
			{
				// If we cannot get the attribute then die.
				var attr = m?.GetCustomAttribute<CustomCommandAttribute>();
				if (null == attr)
					continue;

				// Configure the attribute so that it knows which method to call.
				attr.Configure(m, instance);

				// Return the associated command.
				yield return (CustomDomainCommand)attr;
			}
		} 
		
		/// <summary>
		/// Returns the commands associated with downloading logs from the default file target.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<CustomDomainCommand> GetDefaultLogTargetDownloadCommands()
		{
			// One to allow them to select which logs...
			yield return Dashboards.Commands.ShowSelectLogDownloadDashboardCommand.Create();

			// ...and one that actually does the collation/download.
			yield return Dashboards.Commands.DownloadSelectedLogsDashboardCommand.Create();
			
			// Allow the user to see the latest log entries.
			yield return Dashboards.Commands.ShowLatestLogEntriesDashboardCommand.Create();
			yield return Dashboards.Commands.RetrieveLatestLogEntriesCommand.Create();
		}

		/// <summary>
		/// Returns the commands associated with manually running task queue background operations.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<CustomDomainCommand> GetTaskQueueBackgroundOperationRunCommands()
		{
			// Get the background operations that have a run command.
			// Note: this should be all of them.
			return this
				.GetType()
				.GetPropertiesAndFieldsOfType<TaskQueueBackgroundOperationManager<TSecureConfiguration>>(this)
				.SelectMany(tqbom => tqbom.BackgroundOperations)
				.AsEnumerable()
				.Select(bo => bo.Value?.DashboardRunCommand)
				.Where(c => null != c);
		}
	}
}
