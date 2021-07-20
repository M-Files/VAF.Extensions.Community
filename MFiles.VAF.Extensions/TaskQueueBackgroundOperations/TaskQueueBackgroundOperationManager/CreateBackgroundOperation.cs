using System;
using MFiles.VAF;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager
	{
		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation CreateBackgroundOperation
		(
			string name,
			Action method
		)
		{
			return this.CreateBackgroundOperation
			(
				name,
				(j, d) => method()
			);
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation CreateBackgroundOperation
		(
			string name,
			Action<TaskProcessorJobEx> method
		)
		{
			return this.CreateBackgroundOperation
			(
				name,
				(j, d) => method(j)
			);
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation CreateBackgroundOperation
		(
			string name,
			Action<TaskProcessorJobEx, TaskQueueDirective> method
		)
		{
			return this.CreateBackgroundOperation<TaskQueueDirective>
			(
				name,
				method
			);
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation<TDirective> CreateBackgroundOperation<TDirective>
		(
			string name,
			Action<TaskProcessorJobEx, TDirective> method
		)
			where TDirective : TaskQueueDirective
		{
			TaskQueueBackgroundOperation<TDirective> backgroundOperation;

			lock (TaskQueueBackgroundOperationManager._lock)
			{
				if (this.BackgroundOperations.ContainsKey(name))
					throw new ArgumentException(
						$"A background operation with the name {name} in queue {this.QueueId} already exists.",
						nameof(name));

				// Create the background operation.
				backgroundOperation = new TaskQueueBackgroundOperation<TDirective>
				(
					this,
					name,
					method,
					this.CancellationTokenSource
				);

				// Add it to the dictionary.
				this.BackgroundOperations.Add(name, backgroundOperation);
			}

			// Return it.
			return backgroundOperation;
		}
	}
}