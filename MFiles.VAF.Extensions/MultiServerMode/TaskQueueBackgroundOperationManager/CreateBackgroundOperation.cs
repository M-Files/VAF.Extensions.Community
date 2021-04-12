using System;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode
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
			Action method,
			BackgroundOperationDashboardOptions options = null
		)
		{
			return this.CreateBackgroundOperation
			(
				name,
				(j, d) => method(),
				options
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
			Action<TaskProcessorJob> method,
			BackgroundOperationDashboardOptions options = null
		)
		{
			return this.CreateBackgroundOperation
			(
				name,
				(j, d) => method(j),
				options
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
			Action<TaskProcessorJob, TaskQueueDirective> method,
			BackgroundOperationDashboardOptions options = null
		)
		{
			return this.CreateBackgroundOperation<TaskQueueDirective>
			(
				name,
				method,
				options
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
			Action<TaskProcessorJob, TDirective> method,
			BackgroundOperationDashboardOptions options = null
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
					this.CancellationTokenSource,
					options
				);

				// Add it to the dictionary.
				this.BackgroundOperations.Add(name, new TaskQueueBackgroundOperationOverview()
				{
					BackgroundOperation = backgroundOperation
				});
			}

			// Return it.
			return backgroundOperation;
		}
	}
}