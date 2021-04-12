using System;
using MFiles.VAF.Extensions.ScheduledExecution;
using MFiles.VAF;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager
	{
		/// <summary>
		/// Creates a new background operation and starts it.
		/// The background operation runs the given method according to the <paramref name="schedule"/>.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="schedule">The schedule that defines when the operation should run.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A scheduled background operation.</returns>
		public TaskQueueBackgroundOperation StartScheduledBackgroundOperation
		(
			string name,
			Schedule schedule,
			Action method
		)
		{
			return this.StartScheduledBackgroundOperation
			(
				name,
				schedule,
				(j, d) => method()
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it.
		/// The background operation runs the given method according to the <paramref name="schedule"/>.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="schedule">The schedule that defines when the operation should run.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A scheduled background operation.</returns>
		public TaskQueueBackgroundOperation StartScheduledBackgroundOperation
		(
			string name,
			Schedule schedule,
			Action<TaskProcessorJob> method
		)
		{
			return this.StartScheduledBackgroundOperation
			(
				name,
				schedule,
				(j, d) => method(j)
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it.
		/// The background operation runs the given method according to the <paramref name="schedule"/>.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="schedule">The schedule that defines when the operation should run.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="directive">The directive to pass to the job.</param>
		/// <returns>A started background operation.</returns>
		public TaskQueueBackgroundOperation StartScheduledBackgroundOperation
		(
			string name,
			Schedule schedule,
			Action<TaskProcessorJob, TaskQueueDirective> method,
			TaskQueueDirective directive = null
		)
		{
			return this.StartScheduledBackgroundOperation<TaskQueueDirective>
			(
				name,
				schedule,
				method,
				directive
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it.
		/// The background operation runs the given method according to the <paramref name="schedule"/>.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="schedule">The schedule that defines when the operation should run.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="directive">The directive to pass to the job.</param>
		/// <returns>A started background operation.</returns>
		public TaskQueueBackgroundOperation<TDirective> StartScheduledBackgroundOperation<TDirective>
		(
			string name,
			Schedule schedule,
			Action<TaskProcessorJob, TDirective> method,
			TDirective directive = null
		)
			where TDirective : TaskQueueDirective
		{
			// Create the background operation.
			var backgroundOperation = this.CreateBackgroundOperation
			(
				name,
				method
			);

			// Start it running.
			backgroundOperation.RunOnSchedule(schedule, directive);

			// Return the background operation.
			return backgroundOperation;
		}
	}
}