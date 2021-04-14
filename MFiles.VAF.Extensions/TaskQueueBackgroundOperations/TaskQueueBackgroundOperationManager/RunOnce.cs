﻿using System;
using MFiles.VAF;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager
	{

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <param name="backgroundOperationName">The name of the background operation that should be invoked when this job is run.</param>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive - if any - to pass to the job.</param>
		/// <param name="vault">The vault reference to add the task.  Set to a transactional vault to only run the task if the transaction completes.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce
		(
			string backgroundOperationName,
			DateTime? runAt = null,
			TaskQueueDirective directive = null,
			Vault vault = null
		)
		{
			// Use the other overload.
			this.RunOnce<TaskQueueDirective>
			(
				backgroundOperationName,
				runAt,
				directive,
				vault
			);
		}

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <param name="backgroundOperationName">The name of the background operation that should be invoked when this job is run.</param>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive - if any - to pass to the job.</param>
		/// <param name="vault">The vault reference to add the task.  Set to a transactional vault to only run the task if the transaction completes.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce<TDirective>
		(
			string backgroundOperationName,
			DateTime? runAt = null,
			TDirective directive = null,
			Vault vault = null
		)
			where TDirective : TaskQueueDirective
		{
			// Create our actual directive.
			var backgroundOperationDirective =
				new BackgroundOperationTaskQueueDirective(backgroundOperationName, directive);

			// Schedule the next task to execute at the correct time.
			var nextRun = runAt.HasValue ? runAt.Value.ToUniversalTime() : DateTime.UtcNow;
			this.TaskProcessor.CreateApplicationTaskSafe
			(
				true,
				this.QueueId,
				TaskQueueBackgroundOperation.TaskTypeId,
				backgroundOperationDirective?.ToBytes(),
				nextRun,
				vault: vault ?? this.VaultApplication?.PermanentVault
			);
		}

	}
}