using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using Newtonsoft.Json;

namespace MFiles.VAF.Extensions
{
	public static class ApplicationTaskInfoExtensionMethods
	{
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

			// Try and parse the remarks into the expected type.
			try
			{
				return JsonConvert.DeserializeObject<TTaskInformation>(appTaskUpdateInfo?.Remarks);
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