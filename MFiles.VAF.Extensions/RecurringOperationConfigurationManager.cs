﻿// ReSharper disable once CheckNamespace
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Extensions.ScheduledExecution;

namespace MFiles.VAF.Extensions
{
	internal class RecurringOperationConfigurationManager
	{

	}
	/// <summary>
	/// Manages recurring operations that are configured via standard VAF configuration.
	/// </summary>
	/// <typeparam name="TSecureConfiguration">The type of configuration used by the associated vault application.</typeparam>
	public class RecurringOperationConfigurationManager<TSecureConfiguration>
		: Dictionary<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>
		where TSecureConfiguration : class, new()
	{
		private ILogger Logger { get; } 
			= LogManager.GetLogger<RecurringOperationConfigurationManager>();
		protected ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; private set; }
		public RecurringOperationConfigurationManager(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
		{
			this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
		}
		/// <summary>
		/// Attempts to get the configured provider of how to repeat the task processing.
		/// </summary>
		/// <param name="queueId">The queue ID</param>
		/// <param name="taskType">The task type ID</param>
		/// <param name="recurringOperation">The configured provider, if available.</param>
		/// <returns><see langword="true"/> if the provider is available, <see langword="false"/> otherwise.</returns>
		public bool TryGetValue(string queueId, string taskType, out IRecurrenceConfiguration recurringOperation)
		{
			var key = this.Keys.FirstOrDefault(c => c.QueueID == queueId && c.TaskType == taskType);
			if (null == key)
			{
				recurringOperation = null;
				return false;
			}
			recurringOperation = this[key];
			return true;
		}

		/// <summary>
		/// Gets the next time that the task processor should run,
		/// if a repeating configuration is available.
		/// </summary>
		/// <param name="queueId">The queue ID</param>
		/// <param name="taskType">The task type ID</param>
		/// <returns>The datetime it should run, or null if not available.</returns>
		public DateTimeOffset? GetNextTaskProcessorExecution(string queueId, string taskType, DateTime? after = null)
		{
			return this.TryGetValue(queueId, taskType, out IRecurrenceConfiguration recurringOperation)
				? recurringOperation.GetNextExecution(after)
				: null;
		}


		/// <summary>
		/// Reads <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.Configuration"/> to populate <see cref="RecurringOperationConfiguration"/>
		/// </summary>
		/// <param name="isVaultStartup">Whether the vault is starting.</param>
		public virtual void PopulateFromConfiguration(bool isVaultStartup)
			=> this.PopulateFromConfiguration(this.VaultApplication?.Configuration, isVaultStartup);

		/// <summary>
		/// Reads <paramref name="configuration"/> to populate <see cref="RecurringOperationConfiguration"/>
		/// </summary>
		/// <param name="configuration">The configuration to read</param>
		/// <param name="isVaultStartup">Whether the vault is starting.</param>
		public virtual void PopulateFromConfiguration(TSecureConfiguration configuration, bool isVaultStartup)
		{
			// Remove anything we have configured.
			this.Clear();

			// Sanity.
			if (null == configuration
				|| null == this.VaultApplication?.TaskQueueResolver
				|| null == this.VaultApplication?.TaskManager)
			{
				this.Logger?.Warn($"Vault application, task manager, or task queue resolver are null so cannot populate configuration; skipping.");
				return;
			}

			// Attempt to find any scheduled operation configuration.
			var schedules = new List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>>();
			this.GetTaskProcessorConfiguration(configuration, out schedules);
			this.Logger?.Trace($"{(schedules?.Count ?? 0)} schedules were located in the configuration.");

			// If we have nothing to apply to then die.
			if (null == schedules || schedules.Count == 0)
				return;

			// Load all the processors.
			var dictionary = this.VaultApplication.TaskQueueResolver?
				.GetQueues()?
				.SelectMany
				(
					q => this.VaultApplication.TaskQueueResolver?
						.GetProcessors(q)
						.Select
						(
							p => new KeyValuePair<string, Processor>($"{q}-{p.Type}", p)
						)
				)?.ToDictionary
				(
					p => p.Key,
					p => p.Value
				) ?? new Dictionary<string, Processor>();

			// Iterate over each schedule and see whether we can apply it.
			foreach (var tuple in schedules)
			{
				// Load the key and the schedule.
				var key = $"{tuple.Item1.QueueID}-{tuple.Item1.TaskType}";
				var schedule = tuple.Item2;

				// Validate.
				if (false == dictionary.ContainsKey(key))
				{
					this.Logger?.Warn
					(
						$"Found configuration schedule for queue {tuple.Item1.QueueID} and type {tuple.Item1.TaskType}, but no task processors were registered with that combination."
					);
					continue;
				}

				// Make sure we have no duplicates.
				if (this.Keys.Count(k => k.QueueID == tuple.Item1.QueueID && k.TaskType == tuple.Item1.TaskType) > 0)
				{
					this.Logger?.Error
					(
						$"Multiple configuration schedules found for queue {tuple.Item1.QueueID} and type {tuple.Item1.TaskType}.  Only the first loaded will be used."
					);
					continue;
				}

				// TODO: This would be nicer without the HTML...
				this.Logger?.Trace($"Schedule located for queue {tuple.Item1.QueueID} and type {tuple.Item1.TaskType}: {tuple.Item2.ToDashboardDisplayString()}");

				// Add it to the dictionary.
				this.Add
				(
					tuple.Item1,
					tuple.Item2
				);

				// Cancel any existing executions.
				this.Logger?.Trace($"Cancelling future executions for items in queue {tuple.Item1.QueueID} of type {tuple.Item1.TaskType}.");
				this.VaultApplication.TaskManager.CancelAllFutureExecutions
				(
					tuple.Item1.QueueID,
					tuple.Item1.TaskType,
					throwExceptions: false
				);

				// Work out the next execution time.
				DateTime? nextExecution = null;

				// If this should run at vault startup then run it now.
				if (isVaultStartup && schedule.RunOnVaultStartup.HasValue && schedule.RunOnVaultStartup.Value)
				{
					this.Logger?.Debug($"Processor for queue {tuple.Item1.QueueID} of type {tuple.Item1.TaskType} should run on vault startup; scheduling for now.");
					nextExecution = DateTime.UtcNow;
				}

				// If we don't have a schedule then stop.
				nextExecution = nextExecution ?? schedule?.GetNextExecution()?.UtcDateTime;
				if (false == nextExecution.HasValue)
				{
					this.Logger?.Debug($"Processor for queue {tuple.Item1.QueueID} of type {tuple.Item1.TaskType} has no next-execution returned.  It will not be scheduled to run.");
					continue;
				}
				else
				{
					this.Logger?.Debug($"Processor for queue {tuple.Item1.QueueID} of type {tuple.Item1.TaskType} will be scheduled for {nextExecution.Value:s}");
				}

				// Cancel future executions and schedule the next one if appropriate.
				this.VaultApplication.TaskManager.AddTask
				(
					this.VaultApplication.PermanentVault,
					tuple.Item1.QueueID,
					tuple.Item1.TaskType,
					activationTime: nextExecution
				);
			}
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
			out List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>> schedules,
			bool recurse = true
		)
		{
			schedules = new List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>>();
			if (null == input || null == fieldInfo)
				return;

			// Get the basic value.
			var value = fieldInfo.GetValue(input);
			if (null == value)
				return;

			// If it is enumerable then iterate over the contents and add.
			if (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
			{
				foreach (var item in (IEnumerable)value)
				{
					{
						this.GetTaskProcessorConfiguration(item, out List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>> a);
						schedules.AddRange(a);
					}
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
			out List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>> schedules,
			bool recurse = true
		)
		{
			schedules = new List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>>();
			if (null == input || null == propertyInfo)
				return;

			// Get the basic value.
			var value = propertyInfo.GetValue(input);
			if (value == null)
				return;

			// If it is enumerable then iterate over the contents and add.
			if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
			{
				foreach (var item in (IEnumerable)value)
				{
					{
						this.GetTaskProcessorConfiguration(item, out List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>>  a);
						schedules.AddRange(a);
					}
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
			out List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>> schedules,
			bool recurse = true
		)
		{
			schedules = new List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>>();
			if (null == input)
				return;

			// Get all the fields marked with [DataMember].
			foreach (var f in input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
			{
				// No data member attribute then die.
				if (0 == f.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false).Length)
					continue;

				// If it has the recurring operation configuration then we want it.
				var recurringOperationConfigurationAttributes = f
					.GetCustomAttributes(false)
					.Where(a => a is IRecurringOperationConfigurationAttribute)
					.Cast<IRecurringOperationConfigurationAttribute>();
				foreach (var recurringOperationConfigurationAttribute in recurringOperationConfigurationAttributes)
				{
					if (null != recurringOperationConfigurationAttribute)
					{
						// Validate the field type.
						if (!recurringOperationConfigurationAttribute.ExpectedPropertyOrFieldTypes.Any(t => t.IsAssignableFrom(f.FieldType)))
						{
							this.Logger?.Warn
							(
								$"Found [{recurringOperationConfigurationAttribute.GetType().Name}] but field was not one of types {(string.Join(", ", recurringOperationConfigurationAttribute.ExpectedPropertyOrFieldTypes.Select(t => t.FullName)))} (actual: {f.FieldType.FullName})"
							);
							continue;
						}

						// Get the value and deal with timespans.
						var value = f.GetValue(input);
						if (null == value)
							continue;
						if (value.GetType() == typeof(TimeSpan))
							value = new TimeSpanEx((TimeSpan)value);

						// Validate that the value is a recurring operation.
						if (!typeof(IRecurrenceConfiguration).IsAssignableFrom(value.GetType()))
						{
							this.Logger?.Warn
							(
								$"Found [{recurringOperationConfigurationAttribute.GetType().Name}] but field was not of type IRecurrenceConfiguration (actual: {value.GetType().FullName})"
							);
							continue;
						}

						// Add the schedule to the collection.
						this.Logger?.Trace($"{f.DeclaringType.FullName}.{f.Name} defines the recurrence schedule for queue {recurringOperationConfigurationAttribute.QueueID} and type {recurringOperationConfigurationAttribute.TaskType}.");
						schedules
						.Add
						(
							new Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>
							(
								recurringOperationConfigurationAttribute,
								value as IRecurrenceConfiguration
							)
						);
					}
				}

				// Can we recurse?
				if (recurse)
				{
					var a = new List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>>();
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
				var recurringOperationConfigurationAttributes = p
					.GetCustomAttributes(false)
					.Where(a => a is IRecurringOperationConfigurationAttribute)
					.Cast<IRecurringOperationConfigurationAttribute>();
				foreach (var recurringOperationConfigurationAttribute in recurringOperationConfigurationAttributes)
				{
					if (null != recurringOperationConfigurationAttribute)
					{
						// Validate the property type.
						if (!recurringOperationConfigurationAttribute.ExpectedPropertyOrFieldTypes.Any(t => t.IsAssignableFrom(p.PropertyType)))
						{
							this.Logger?.Warn
							(
								$"Found [{recurringOperationConfigurationAttribute.GetType().Name}] but property was not of type {(string.Join(", ", recurringOperationConfigurationAttribute.ExpectedPropertyOrFieldTypes.Select(t => t.FullName)))} (actual: {p.PropertyType.FullName})"
							);
							continue;
						}

						// Get the value and deal with timespans.
						var value = p.GetValue(input);
						if (null == value)
							continue;
						if (value.GetType() == typeof(TimeSpan))
							value = new TimeSpanEx((TimeSpan)value);

						// Validate the type.
						if (!typeof(IRecurrenceConfiguration).IsAssignableFrom(value.GetType()))
						{
							this.Logger?.Warn
							(
								$"Found [{recurringOperationConfigurationAttribute.GetType().Name}] but property was not of type IRecurrenceConfiguration (actual: {value.GetType().FullName})"
							);
							continue;
						}

						// Add the schedule to the collection.
						this.Logger?.Trace($"{p.DeclaringType.FullName}.{p.Name} defines the recurrence schedule for queue {recurringOperationConfigurationAttribute.QueueID} and type {recurringOperationConfigurationAttribute.TaskType}.");
						schedules.Add
						(
							new Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>
							(
								recurringOperationConfigurationAttribute,
								value as IRecurrenceConfiguration
							)
						);
					}
				}

				// Can we recurse?
				if (recurse)
				{
					var a = new List<Tuple<IRecurringOperationConfigurationAttribute, IRecurrenceConfiguration>>();
					this.GetTaskProcessorConfiguration(input, p, out a);
					schedules.AddRange(a);
				}
			}
		}
	}
}
