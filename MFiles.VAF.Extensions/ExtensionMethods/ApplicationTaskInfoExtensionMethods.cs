using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using Newtonsoft.Json;
using System;

namespace MFiles.VAF.Extensions
{
	public static class ApplicationTaskInfoExtensionMethods
	{
		/// <summary>
		/// Gets the time elapsed between the latest activity and the activation timestamp.
		/// </summary>
		/// <param name="taskInfo">The task in question.</param>
		/// <returns>The time span, or <see cref="TimeSpan.Zero"/> if null.</returns>
		public static TimeSpan GetElapsedTime(this ApplicationTaskInfo taskInfo)
		{
			// Sanity.
			if (null == taskInfo)
				return TimeSpan.Zero;
			
			// If the activation is in the future then no elapsed time yet.
			var activation = taskInfo.ActivationTimestamp.ToDateTime(DateTimeKind.Utc);
			if (activation > DateTime.UtcNow)
				return TimeSpan.Zero;

			// What's the difference?
			var delta =
				taskInfo.LatestActivityTimestamp.ToDateTime(DateTimeKind.Utc)
				-
				activation;

			// If it's less than a second then zero.
			return delta < TimeSpan.FromSeconds(1)
				? TimeSpan.Zero
				: delta;
		}
		/// <summary>
		/// Extracts the remarks from the <paramref name="taskInfo"/>'s latest update,
		/// and attempts to deserialise it into a <typeparamref name="TTaskInformation"/>.
		/// </summary>
		/// <typeparam name="TTaskInformation">The expected type of data.</typeparam>
		/// <param name="taskInfo">The task to retrieve data for.</param>
		/// <returns>the latest update, if available.</returns>
		public static TTaskInformation RetrieveTaskInfo<TTaskInformation>(this ApplicationTaskInfo taskInfo)
			where TTaskInformation : TaskInformation
		{
			// Attempt to retrieve the app task update info.
			if (null == taskInfo)
				return null;
			var applicationTask = taskInfo.ToApplicationTask();
			if (null == applicationTask)
				return null;
			var appTaskUpdateInfo = AppTaskUpdateInfo.From
			(
				TaskQueueBackgroundOperationManager.CurrentServer.ServerID,
				applicationTask,
				true
			);

			// Sanity.
			if (string.IsNullOrWhiteSpace(appTaskUpdateInfo?.Remarks))
				return null;

			// Try and parse the remarks into the expected type.
			try
			{
				var info = JsonConvert.DeserializeObject<TTaskInformation>(appTaskUpdateInfo.Remarks);
				if (null != info)
					info.CurrentTaskState = applicationTask.State;
				return info;
			}
			catch
			{
				// If we got nothing then return something empty.
				return new TaskInformation()
				{
					StatusDetails = appTaskUpdateInfo?.Remarks
				} as TTaskInformation;
			}
		}
		public static TaskInformation RetrieveTaskInfo(this ApplicationTaskInfo taskInfo)
		{
			return taskInfo.RetrieveTaskInfo<TaskInformation>();
		}
	}
}