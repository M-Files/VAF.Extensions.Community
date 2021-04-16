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
		/// Extracts the remarks from the <paramref name="taskInfo"/>'s latest update,
		/// and attempts to deserialise it into a <typeparamref name="TTaskInformation"/>.
		/// </summary>
		/// <typeparam name="TTaskInformation">The expected type of data.</typeparam>
		/// <param name="applicationTask">The task to retrieve data for.</param>
		/// <returns>the latest update, if available.</returns>
		public static TTaskInformation RetrieveTaskInfo<TTaskInformation>(this ApplicationTask applicationTask)
			where TTaskInformation : TaskInformation
		{
			// Attempt to retrieve the app task update info.
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
		public static TaskInformation RetrieveTaskInfo(this ApplicationTask applicationTask)
		{
			return applicationTask.RetrieveTaskInfo<TaskInformation>();
		}
		/// <summary>
		/// Extracts the remarks from the <paramref name="taskInfo"/>'s latest update,
		/// and attempts to deserialise it into a <typeparamref name="TTaskInformation"/>.
		/// </summary>
		/// <typeparam name="TTaskInformation">The expected type of data.</typeparam>
		/// <param name="applicationTaskInfo">The task to retrieve data for.</param>
		/// <returns>the latest update, if available.</returns>
		public static TTaskInformation RetrieveTaskInfo<TTaskInformation>(this ApplicationTaskInfo applicationTaskInfo)
			where TTaskInformation : TaskInformation
		{
			return applicationTaskInfo?.ToApplicationTask()?.RetrieveTaskInfo<TTaskInformation>();
		}
		public static TaskInformation RetrieveTaskInfo(this ApplicationTaskInfo applicationTaskInfo)
		{
			return applicationTaskInfo?.ToApplicationTask()?.RetrieveTaskInfo<TaskInformation>();
		}
	}
}