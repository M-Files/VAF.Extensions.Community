using MFiles.VAF;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	public static class TaskProcessorJobExtensionMethods
	{
		/// <summary>
		/// Wraps a call to <see cref="TaskQueueDirective.Parse{T}(Common.ApplicationTaskQueue.ApplicationTask)"/>
		/// to extract any directive supplied to the <paramref name="job" />.
		/// </summary>
		/// <typeparam name="TDirective">The type of the directive.</typeparam>
		/// <param name="job">The job to retrieve the directive for.</param>
		/// <returns>The directive, or null if no directive passed.</returns>
		public static TDirective GetTaskQueueDirective<TDirective>(this TaskProcessorJob job)
		where TDirective : TaskQueueDirective
		{
			// Sanity.
			if(null == job?.Data?.Value)
				return null;

			// Unwrap.
			return TaskQueueDirective.Parse<TDirective>(job.Data?.Value);
		}
		
		/// <summary>
		/// Wraps a call to <see cref="TaskQueueDirective.Parse{T}(Common.ApplicationTaskQueue.ApplicationTask)"/>
		/// to extract any directive supplied to the <paramref name="job" />.
		/// </summary>
		/// <param name="job">The job to retrieve the directive for.</param>
		/// <returns>The directive, or null if no directive passed.</returns>
		public static TaskQueueDirective GetTaskQueueDirective(this TaskProcessorJob job)
		{
			return job.GetTaskQueueDirective<TaskQueueDirective>();
		}
	}
}
