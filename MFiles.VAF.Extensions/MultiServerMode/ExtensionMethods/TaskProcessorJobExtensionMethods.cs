using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods
{
	public static class TaskProcessorJobExtensionMethods
	{
		public static TDirective GetTaskQueueDirective<TDirective>(this TaskProcessorJob job)
		where TDirective : TaskQueueDirective
		{
			// Sanity.
			if(null == job?.Data?.Value)
				return null;

			// Unwrap.
			return TaskQueueDirective.Parse<TDirective>(job.Data?.Value);
		}
		public static TaskQueueDirective GetTaskQueueDirective(this TaskProcessorJob job)
		{
			return job.GetTaskQueueDirective<TaskQueueDirective>();
		}
	}
}
