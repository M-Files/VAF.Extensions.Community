using MFiles.VAF.AppTasks;
using MFilesAPI;

namespace MFiles.VAF.Extensions
{
    public class TaskQueueConfiguration
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string QueueID { get; set; }
        public string TaskType { get; set; }
        public MFTaskQueueProcessingBehavior TaskQueueProcessingBehavior { get; set; }
            = MFTaskQueueProcessingBehavior.MFProcessingBehaviorConcurrent;
        public TaskQueueSettings QueueSettings { get; set; }
        public TaskProcessorSettings ProcessorSettings { get; set; }
        public TransactionMode TransactionMode { get; } = TransactionMode.Unsafe;
    }
}