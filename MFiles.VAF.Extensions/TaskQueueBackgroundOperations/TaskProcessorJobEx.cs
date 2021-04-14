using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using Newtonsoft.Json;
using System.Threading;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Provides access to the <see cref="TaskProcessorJobEx.Job"/> and the associated
	/// <see cref="TaskProcessorJobEx.TaskQueueBackgroundOperationManager"/> and <see cref="TaskProcessorJobEx.TaskProcessor"/>.
	/// Also provides helper methods for setting typed task information for rendering onto dashboards.
	/// </summary>
	public class TaskProcessorJobEx
	{
		/// <summary>
		/// The task processor job itself.
		/// </summary>
		public TaskProcessorJob Job { get; set; }

		/// <summary>
		/// The background operation manager that owns this job.
		/// </summary>
		public TaskQueueBackgroundOperationManager TaskQueueBackgroundOperationManager { get; set; }

		/// <summary>
		/// The task processor processing the job (from <see cref="TaskQueueBackgroundOperationManager.TaskProcessor"/>.
		/// </summary>
		public AppTaskBatchProcessor TaskProcessor { get => this.TaskQueueBackgroundOperationManager?.TaskProcessor; }

		/// <summary>
		/// Updates the task information for the job.
		/// See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/>.
		/// </summary>
		/// <param name="taskState">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		/// <param name="remarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		/// <param name="appendRemarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		public void UpdateTaskInfo(
			MFTaskState taskState,
			string remarks,
			bool appendRemarks = false
		)
		{
			this.TaskProcessor.UpdateTaskInfo(this, taskState, remarks, appendRemarks);
		}

		/// <summary>
		/// Updates the task information for the job.
		/// See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/>.
		/// </summary>
		/// <param name="remarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		/// <param name="appendRemarks">See <see cref="TaskProcessorBase{TSettings}.UpdateTaskInfo(TaskProcessorJob, MFTaskState, string, bool)"/></param>
		public void UpdateTaskInfo
			(
			string remarks,
			bool appendRemarks = false
			)
		{
			this.UpdateTaskInfo(MFTaskState.MFTaskStateInProgress, remarks, appendRemarks);
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
			this.UpdateTaskInfo(new TaskInformation()
			{
				PercentageComplete = percentageComplete,
				StatusDetails = statusDetails
			});
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
			this.UpdateTaskInfo
			(
				MFTaskState.MFTaskStateInProgress,
				null == status ? "" : JsonConvert.SerializeObject(status),
				appendRemarks: false
			);
		}

		/// <summary>
		/// Retrieves the latest task information for the job.
		/// </summary>
		/// <typeparam name="TTaskInformation">The type of the status.</typeparam>
		/// <returns>The status, or default if not found.</returns>
		public TTaskInformation RetrieveTaskInfo<TTaskInformation>()
			where TTaskInformation : TaskInformation
		{
			var appTask = this.TaskProcessor.GetLatestTaskInfo(this.Job.AppTaskId);
			var updateInfo = AppTaskUpdateInfo.From
			(
				TaskQueueBackgroundOperationManager.CurrentServer.ServerID,
				appTask,
				true
			);
			try
			{
				return JsonConvert.DeserializeObject<TTaskInformation>(updateInfo.Remarks);
			}
			catch
			{
				return default;
			}
		}

		/// <summary>
		/// Retrieves the latest task information for the job.
		/// </summary>
		/// <returns>The status, or default if not found.</returns>
		public TaskInformation RetrieveTaskInfo()
		{
			return this.RetrieveTaskInfo<TaskInformation>();
		}

		/// <summary>
		/// Converts the <see cref="TaskProcessorJobEx"/> to a simple <see cref="TaskProcessorJob"/>.
		/// </summary>
		/// <param name="input"></param>
		public static implicit operator TaskProcessorJob(TaskProcessorJobEx input)
		{
			return input?.Job;
		}
	}

	/// <summary>
	/// Details about the current job status.
	/// </summary>
	public class TaskInformation
	{
		/// <summary>
		/// Details about the current status (e.g. "Processing object 5 of 100").
		/// </summary>
		public string StatusDetails { get; set; }

		/// <summary>
		/// Details on how far through the process the task is.
		/// </summary>
		public int? PercentageComplete { get; set; }
	}
}