using MFiles.VAF.AppTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.ExtensionMethods
{
	public static class ITaskProcessingJobOfTExtensionMethods
	{
		/// <summary>
		/// Updates the in-progress status of the task.
		/// </summary>
		/// <typeparam name="TDirective">The type of directive this job accepts.</typeparam>
		/// <param name="job">The job.</param>
		/// <param name="taskInformation">Information about the task.</param>
		public static void Update<TDirective>(this ITaskProcessingJob<TDirective> job, TaskInformation taskInformation)
			where TDirective : TaskDirective
		{
			// Sanity.
			if (null == job)
				throw new ArgumentNullException(nameof(job));
			if (null == taskInformation)
				return;

			// Use the standard method.
			job.Update
			(
				percentComplete: taskInformation.PercentageComplete,
				details: taskInformation.StatusDetails,
				data: taskInformation.ToJObject()
			);
		}
	}
}
