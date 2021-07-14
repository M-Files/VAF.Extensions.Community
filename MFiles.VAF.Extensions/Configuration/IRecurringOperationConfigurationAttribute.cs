// ReSharper disable once CheckNamespace

using System;

namespace MFiles.VAF.Extensions
{
	public interface IRecurringOperationConfigurationAttribute
	{
		string QueueID { get; set; }
		string TaskType { get; set; }
	}
	public abstract class RecurringOperationConfigurationAttributeBase
		: Attribute, IRecurringOperationConfigurationAttribute
	{
		public string QueueID { get; set; }
		public string TaskType { get; set; }
		public RecurringOperationConfigurationAttributeBase
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
