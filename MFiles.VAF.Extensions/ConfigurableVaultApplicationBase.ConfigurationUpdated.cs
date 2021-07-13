// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.MultiserverMode;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using System.Reflection;
using System.Collections;
using MFiles.VAF.Extensions.ScheduledExecution;

namespace MFiles.VAF.Extensions
{
	public class TaskProcessingScheduleConfiguration
		: Dictionary<IScheduledConfiguration, IExecutionDateTimeProvider>
	{
		/// <summary>
		/// Attempts to get the configured provider of how to repeat the task processing.
		/// </summary>
		/// <param name="queueId">The queue ID</param>
		/// <param name="taskType">The task type ID</param>
		/// <param name="provider">The configured provider, if available.</param>
		/// <returns><see langword="true"/> if the provider is available, <see langword="false"/> otherwise.</returns>
		public bool TryGetValue(string queueId, string taskType, out IExecutionDateTimeProvider provider)
		{
			var key = this.Keys.FirstOrDefault(c => c.QueueID == queueId && c.TaskType == taskType);
			if (null == key)
			{
				provider = null;
				return false;
			}
			provider = this[key];
			return true;
		}
	}

	/// <summary>
	/// A base class that automatically implements the pattern required for broadcasting
	/// configuration changes to other servers.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The configuration type.</typeparam>
	/// <remarks>See https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes for further details.</remarks>
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: MFiles.VAF.Core.ConfigurableVaultApplicationBase<TSecureConfiguration>
	where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// Contains information about VAF configuration that
		/// control when task processing repeats.
		/// </summary>
		protected TaskProcessingScheduleConfiguration TaskProcessingScheduleConfiguration { get; }
			= new TaskProcessingScheduleConfiguration();

		/// <summary>
		/// Gets the next time that the task processor should run,
		/// if a repeating configuration is available.
		/// </summary>
		/// <param name="queueId">The queue ID</param>
		/// <param name="taskType">The task type ID</param>
		/// <returns>The datetime it should run, or null if not available.</returns>
		public DateTime? GetNextTaskProcessorExecution(string queueId, string taskType, DateTime? after = null)
		{
			return this.TaskProcessingScheduleConfiguration.TryGetValue(queueId, taskType, out IExecutionDateTimeProvider provider)
				? provider.GetNextExecution(after)
				: null;
		}

		/// <summary>
		/// Reads <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.Configuration"/>
		/// to populate <see cref="TaskProcessingScheduleConfiguration"/>
		/// </summary>
		protected virtual void PopulateTaskProcessingScheduleConfiguration()
		{
			// Remove anything we have configured.
			this.TaskProcessingScheduleConfiguration.Clear();

			// Attempt to find any scheduled operation configuration.
			var schedules = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
			this.GetTaskProcessorConfiguration(this.Configuration, out schedules);

			// Can weapply a schedule to something?
			if (null != schedules && schedules.Count > 0)
			{
				// Load all the processors.
				var dictionary = this.TaskQueueResolver
					.GetQueues()
					.SelectMany
					(
						q => this.TaskQueueResolver
							.GetProcessors(q)
							.Select
							(
								p => new KeyValuePair<string, Processor>($"{q}-{p.Type}", p)
							)
					).ToDictionary
					(
						p => p.Key,
						p => p.Value
					);

				// Iterate over each schedule and see whether we can apply it.
				foreach (var tuple in schedules)
				{
					// Load the key and the schedule.
					var key = $"{tuple.Item1.QueueID}-{tuple.Item1.TaskType}";
					var schedule = tuple.Item2;

					// Validate.
					if (false == dictionary.ContainsKey(key))
					{
						SysUtils.ReportToEventLog
						(
							$"Found configuration schedule for queue {tuple.Item1.QueueID} and type {tuple.Item1.TaskType}, but no task processors were registerd with that combination.",
							System.Diagnostics.EventLogEntryType.Warning
						);
						continue;
					}

					// Cancel any future executions.
					this.TaskManager.CancelAllFutureExecutions
					(
						tuple.Item1.QueueID,
						tuple.Item1.TaskType,
						includeCurrentlyExecuting: false,
						vault: this.PermanentVault
					);

					// If we don't have a schedule then stop.
					var nextExecution = schedule?.GetNextExecution();
					if (false == nextExecution.HasValue)
						continue;

					// Add it to the dictionary.
					this.TaskProcessingScheduleConfiguration.Add
					(
						tuple.Item1,
						tuple.Item2
					);

					// Schedule the next run.
					this.TaskManager.AddTask
					(
						this.PermanentVault,
						tuple.Item1.QueueID,
						tuple.Item1.TaskType,
						activationTime: nextExecution.Value
					);
				}
			}
		}

		protected override void OnConfigurationUpdated(TSecureConfiguration oldConfiguration, bool updateExternals)
		{
			// Populate the task processing schedule configuration.
			this.PopulateTaskProcessingScheduleConfiguration();

			// Base implementation is empty, but good practice to call it.
			base.OnConfigurationUpdated(oldConfiguration, updateExternals);
		}

		/// <summary>
		/// Retrieves any task processor scheduling/recurring configuration
		/// that is exposed via VAF configuration.
		/// </summary>
		/// <param name="input">The object containing the <paramref name="fieldInfo"/>.</param>
		/// <param name="fieldInfo">The field to retrieve the configuration from.</param>
		/// <param name="schedules">All configuration found relating to scheduled execution.</param>
		/// <param name="recurse">Whether to recurse down the object structure exposed.</param>
		protected virtual void GetTaskProcessorConfiguration
		(
			object input,
			FieldInfo fieldInfo,
			out List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>> schedules,
			bool recurse = true
		)
		{
			schedules = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
			if (null == input)
				return;

			// Get the basic value.
			var value = fieldInfo.GetValue(input);

			// If it is enumerable then iterate over the contents and add.
			if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
			{
				foreach (var item in (IEnumerable)value)
				{
					var a = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
					this.GetTaskProcessorConfiguration(item, out a);
					schedules.AddRange(a);
				}
				return;
			}

			// Otherwise just add.
			this.GetTaskProcessorConfiguration(value, out schedules);

		}

		/// <summary>
		/// Retrieves any task processor scheduling/recurring configuration
		/// that is exposed via VAF configuration.
		/// </summary>
		/// <param name="input">The object containing the <paramref name="propertyInfo"/>.</param>
		/// <param name="propertyInfo">The property to retrieve the configuration from.</param>
		/// <param name="schedules">All configuration found relating to scheduled execution.</param>
		/// <param name="recurse">Whether to recurse down the object structure exposed.</param>
		protected virtual void GetTaskProcessorConfiguration
		(
			object input,
			PropertyInfo propertyInfo,
			out List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>> schedules,
			bool recurse = true
		)
		{
			schedules = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
			if (null == input)
				return;

			// Get the basic value.
			var value = propertyInfo.GetValue(input);

			// If it is enumerable then iterate over the contents and add.
			if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
			{
				foreach (var item in (IEnumerable)value)
				{
					var a = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
					this.GetTaskProcessorConfiguration(item, out a);
					schedules.AddRange(a);
				}
				return;
			}

			// Otherwise just add.
			this.GetTaskProcessorConfiguration(value, out schedules);

		}

		/// <summary>
		/// Retrieves any task processor scheduling/recurring configuration
		/// that is exposed via VAF configuration.
		/// </summary>
		/// <param name="input">The object containing the configuration.</param>
		/// <param name="schedules">All configuration found relating to scheduled execution.</param>
		/// <param name="recurse">Whether to recurse down the object structure exposed.</param>
		protected virtual void GetTaskProcessorConfiguration
		(
			object input, 
			out List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>> schedules,
			bool recurse = true
		)
		{
			schedules = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
			if (null == input)
				return;

			// Get all the fields marked with [DataMember].
			foreach (var f in input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
			{
				// No data member attribute then die.
				if (0 == f.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false).Length)
					continue;

				// If it has the scheduled operation configuration then we want it.
				var scheduledOperationConfigurationAttribute = f.GetCustomAttributes(typeof(ScheduledOperationConfigurationAttribute), false)
					.FirstOrDefault() as ScheduledOperationConfigurationAttribute;
				if(null != scheduledOperationConfigurationAttribute)
				{
					// Validate the type.
					if (f.FieldType != typeof(ScheduledExecution.Schedule))
					{
						SysUtils.ReportToEventLog
						(
							$"Found [ScheduledOperationConfiguration] but was not used on a schedule (actual: {f.FieldType.FullName})",
							System.Diagnostics.EventLogEntryType.Warning
						);
						continue;
					}

					// Add the schedule to the collection.
					schedules
					.Add
					(
						new Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>
						(
							scheduledOperationConfigurationAttribute,
							f.GetValue(input) as ScheduledExecution.Schedule
						)
					);
				}

				// TODO: Other types of configuration.

				// Can we recurse?
				if (recurse)
				{
					var a = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
					this.GetTaskProcessorConfiguration(input, f, out a);
					schedules.AddRange(a);
				}
			}

			// Now do the same for properties.
			foreach (var p in input.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				// No data member attribute then die.
				if (0 == p.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false).Length)
					continue;

				// If it has the scheduled operation configuration then we want it.
				var scheduledOperationConfigurationAttribute = p.GetCustomAttributes(typeof(ScheduledOperationConfigurationAttribute), false)
					.FirstOrDefault() as ScheduledOperationConfigurationAttribute;
				if (null != scheduledOperationConfigurationAttribute)
				{
					// Validate the type.
					if (p.PropertyType != typeof(ScheduledExecution.Schedule))
					{
						SysUtils.ReportToEventLog
						(
							$"Found [ScheduledOperationConfiguration] but was not used on a schedule (actual: {p.PropertyType.FullName})",
							System.Diagnostics.EventLogEntryType.Warning
						);
						continue;
					}

					// Add the schedule to the collection.
					schedules.Add
					(
						new Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>
						(
							scheduledOperationConfigurationAttribute,
							p.GetValue(input) as ScheduledExecution.Schedule
						)
					);
				}

				// TODO: Other types of configuration.

				// Can we recurse?
				if (recurse)
				{
					var a = new List<Tuple<IScheduledConfiguration, ScheduledExecution.Schedule>>();
					this.GetTaskProcessorConfiguration(input, p, out a);
					schedules.AddRange(a);
				}
			}
		}
	}
}
