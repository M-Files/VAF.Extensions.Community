// ReSharper disable once CheckNamespace
using System;
using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.Extensions.ScheduledExecution;

namespace MFiles.VAF.Extensions
{
	public class RecurringOperationConfiguration
		: Dictionary<IRecurringOperationConfigurationAttribute, IRecurringOperation>
	{
		/// <summary>
		/// Attempts to get the configured provider of how to repeat the task processing.
		/// </summary>
		/// <param name="queueId">The queue ID</param>
		/// <param name="taskType">The task type ID</param>
		/// <param name="provider">The configured provider, if available.</param>
		/// <returns><see langword="true"/> if the provider is available, <see langword="false"/> otherwise.</returns>
		public bool TryGetValue(string queueId, string taskType, out IRecurringOperation provider)
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

		/// <summary>
		/// Gets the next time that the task processor should run,
		/// if a repeating configuration is available.
		/// </summary>
		/// <param name="queueId">The queue ID</param>
		/// <param name="taskType">The task type ID</param>
		/// <returns>The datetime it should run, or null if not available.</returns>
		public DateTime? GetNextTaskProcessorExecution(string queueId, string taskType, DateTime? after = null)
		{
			return this.TryGetValue(queueId, taskType, out IRecurringOperation provider)
				? provider.GetNextExecution(after)
				: null;
		}
	}
}
