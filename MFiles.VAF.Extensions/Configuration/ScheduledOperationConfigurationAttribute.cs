// ReSharper disable once CheckNamespace
using MFiles.VAF.Configuration;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Defines that the following property or field controls how a task processor should recur.
	/// The property or field that follows should be a <see cref="ScheduledExecution.Schedule"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class ScheduledOperationConfigurationAttribute
		: JsonConfEditorAttribute, IRecurringOperationConfigurationAttribute
	{
		/// <inheritdoc />
		public string QueueID { get; set; }

		/// <inheritdoc />
		public string TaskType { get; set; }

		/// <inheritdoc />
		public Type ExpectedPropertyOrFieldType { get; private set; }

		public ScheduledOperationConfigurationAttribute
		(
			string queueId,
			string taskType
		)
		{
			this.QueueID = queueId;
			this.TaskType = taskType;
			this.ExpectedPropertyOrFieldType = typeof(ScheduledExecution.Schedule);
		}
	}
}
