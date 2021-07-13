// ReSharper disable once CheckNamespace
using System;

namespace MFiles.VAF.Extensions
{
	public interface IScheduledConfiguration
	{
		string QueueID { get; set; }
		string TaskType { get; set; }
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ScheduledOperationConfigurationAttribute
		: Attribute, IScheduledConfiguration
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
