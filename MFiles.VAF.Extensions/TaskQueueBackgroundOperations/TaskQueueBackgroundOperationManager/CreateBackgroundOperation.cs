﻿using System;
using MFiles.VAF;
using MFiles.VAF.AppTasks;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager<TSecureConfiguration>
	{
		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation<TSecureConfiguration> CreateBackgroundOperation
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
		public TaskQueueBackgroundOperation<TSecureConfiguration> CreateBackgroundOperation
		(
			string name,
			Action<TaskProcessorJobEx<BackgroundOperationTaskDirective, TSecureConfiguration>> method
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
		public TaskQueueBackgroundOperation<TSecureConfiguration> CreateBackgroundOperation
		(
			string name,
			Action<TaskProcessorJobEx<BackgroundOperationTaskDirective, TSecureConfiguration>, TaskDirective> method
		)
		{
			return this.CreateBackgroundOperation<BackgroundOperationTaskDirective>
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
		public TaskQueueBackgroundOperation<TDirective, TSecureConfiguration> CreateBackgroundOperation<TDirective>
		(
			string name,
			Action<TaskProcessorJobEx<BackgroundOperationTaskDirective, TSecureConfiguration>, TDirective> method
		)
			where TDirective : TaskDirective
		{
			TaskQueueBackgroundOperation<TDirective, TSecureConfiguration> backgroundOperation;

			lock (TaskQueueBackgroundOperationManager<TSecureConfiguration>._lock)
			{
				if (this.BackgroundOperations.ContainsKey(name))
					throw new ArgumentException
					(
						string.Format
						(
							Resources.Exceptions.TaskQueueBackgroundOperations.BackgroundOperationAlreadyExistsWithThatName,
							name,
							this.QueueId
						),
						nameof(name)
					);

				// Create the background operation.
				backgroundOperation = new TaskQueueBackgroundOperation<TDirective, TSecureConfiguration>
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