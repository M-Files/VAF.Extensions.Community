using MFiles.VAF.AppTasks;
using System;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	public class RescheduleProcessorTaskDirective
		: TaskDirective
	{
		[DataMember]
		public string QueueID { get; set; }
		[DataMember]
		public string TaskType { get; set; }
		[DataMember]
		public DateTimeOffset? NextExecution { get; set; }
		[DataMember]
		public TaskDirective InnerDirective { get; set; }
	}
}
