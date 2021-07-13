// ReSharper disable once CheckNamespace
using System;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ScheduledOperationConfigurationAttribute
		: Attribute
	{
		public string QueueID { get; set; }
		public string TaskType { get; set; }
		public ScheduledOperationConfigurationAttribute
		(
			string queueId,
			string taskType
		)
		{
			this.QueueID = queueId;
			this.TaskType = taskType;
		}
	}
}
