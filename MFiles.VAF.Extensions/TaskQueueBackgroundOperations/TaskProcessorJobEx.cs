using MFiles.VAF.Extensions.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Provides access to the <see cref="TaskProcessorJobEx.Job"/> and the associated
	/// <see cref="TaskProcessorJobEx.TaskQueueBackgroundOperationManager"/> and <see cref="TaskProcessorJobEx.TaskProcessor"/>.
	/// Also provides helper methods for setting typed task information for rendering onto dashboards.
	/// </summary>
	public class TaskProcessorJobEx<TDirective, TSecureConfiguration>
		where TDirective : AppTasks.TaskDirective
		where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// The task processor job itself.
		/// </summary>
		public AppTasks.ITaskProcessingJob<TDirective> Job { get; set; }

		/// <summary>
		/// The vault associated with the job.
		/// </summary>
		public Vault Vault { get => this.Job?.Vault; }

		/// <summary>
		/// The background operation manager that owns this job.
		/// </summary>
		public TaskQueueBackgroundOperationManager<TSecureConfiguration> TaskQueueBackgroundOperationManager { get; set; }

		/// <summary>
		/// Updates the task information for the job.
		/// See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/>.
		/// </summary>
		/// <param name="taskState">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		/// <param name="remarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		/// <param name="appendRemarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		public void UpdateTaskInfo(
			MFTaskState taskState,
			string remarks
		)
		{
			var info = this.RetrieveTaskInfo() ?? new TaskInformation();
			info.CurrentTaskState = taskState;
			info.StatusDetails = remarks;
			info.LastActivity = DateTime.Now;
			this.UpdateTaskInfo(info);
		}

		/// <summary>
		/// Updates the task information for the job.
		/// See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/>.
		/// </summary>
		/// <param name="remarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		/// <param name="appendRemarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		public void UpdateTaskInfo
			(
			string remarks
			)
		{
			this.UpdateTaskInfo(MFTaskState.MFTaskStateInProgress, remarks);
		}

		/// <summary>
		/// Updates the task information for the job.
		/// Serialises <paramref name="status"/> into the "remarks" argument on <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/>.
		/// </summary>
		/// <param name="status">The latest status.</param>
		public void UpdateTaskInfo(TaskInformation status)
		{
			this.UpdateTaskInfo<TaskInformation>(status);
		}

		/// <summary>
		/// Updates the task information for the job.
		/// </summary>
		/// <param name="percentageComplete">How far complete this job is in processing, if known.</param>
		/// <param name="statusDetails">Any additional details on the task status.</param>
		public void UpdateTaskInfo(int? percentageComplete, string statusDetails)
		{
			var info = this.RetrieveTaskInfo() ?? new TaskInformation();
			info.PercentageComplete = percentageComplete;
			info.StatusDetails = statusDetails;
			this.UpdateTaskInfo(info);
		}

		/// <summary>
		/// Updates the task information for the job.
		/// Serialises <paramref name="status"/> into the "remarks" argument on <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/>.
		/// </summary>
		/// <typeparam name="TTaskInformation">The type of the status.</typeparam>
		/// <param name="status">The latest status.</param>
		public void UpdateTaskInfo<TTaskInformation>(TTaskInformation status )
			where TTaskInformation : TaskInformation
		{
			// Copy data from the current info.
			var info = this.RetrieveTaskInfo();
			if (null != info && null != status)
			{
				status.Started = info.Started;
				status.Completed = info.Completed;
			}

			// Ensure the last activity is correct.
			if (null != status)
			{
				status.LastActivity = DateTime.Now;
			}

			// Use the other overload.
			this.Job.Update(status);
		}

		/// <summary>
		/// Retrieves the latest task information for the job.
		/// </summary>
		/// <typeparam name="TTaskInformation">The type of the status.</typeparam>
		/// <returns>The status, or default if not found.</returns>
		public TaskInformation RetrieveTaskInfo()
		{
			return new TaskInformation(this.Job.GetStatus()?.Data);
		}
	}
}