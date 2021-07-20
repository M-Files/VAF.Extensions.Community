﻿using System;
using MFiles.VAF;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager
	{
		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A scheduled background operation.</returns>
		public TaskQueueBackgroundOperation StartRecurringBackgroundOperation
		(
			string name,
			TimeSpan interval,
			Action method
		)
		{
			return this.StartRecurringBackgroundOperation
			(
				name,
				interval,
				(j, d) => method()
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A scheduled background operation.</returns>
		public TaskQueueBackgroundOperation StartRecurringBackgroundOperation
		(
			string name,
			TimeSpan interval,
			Action<TaskProcessorJobEx> method
		)
		{
			return this.StartRecurringBackgroundOperation
			(
				name,
				interval,
				(j, d) => method(j)
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="directive">The directive to pass to the job.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A started background operation.</returns>
		public TaskQueueBackgroundOperation StartRecurringBackgroundOperation
		(
			string name,
			TimeSpan interval,
			Action<TaskProcessorJobEx, TaskQueueDirective> method,
			TaskQueueDirective directive = null
		)
		{
			return this.StartRecurringBackgroundOperation<TaskQueueDirective>
			(
				name,
				interval,
				method,
				directive
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="directive">The directive to pass to the job.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A started background operation.</returns>
		public TaskQueueBackgroundOperation<TDirective> StartRecurringBackgroundOperation<TDirective>
		(
			string name,
			TimeSpan interval,
			Action<TaskProcessorJobEx, TDirective> method,
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
			backgroundOperation.RunAtIntervals(interval, directive);

			// Return the background operation.
			return backgroundOperation;
		}
	}
}