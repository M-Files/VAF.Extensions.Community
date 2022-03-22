using MFiles.VAF.AppTasks;
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
		public static TTaskInformation RetrieveTaskInfo<TTaskInformation>(this ApplicationTask applicationTask, string serverId)
			where TTaskInformation : TaskInformation
		{
			// Attempt to retrieve the app task update info.
			var appTaskUpdateInfo = AppTaskUpdateInfo.From
			(
				serverId,
				applicationTask,
				true
			);

			// It will be in remarks for older tasks and progress for newer.
			var serialisedContent = appTaskUpdateInfo.Remarks ?? applicationTask.Progress;

			// Sanity.
			if (string.IsNullOrWhiteSpace(serialisedContent))
				return null;

			// Try and parse the remarks into the expected type.
			try
			{
				var info = JsonConvert.DeserializeObject<TTaskInformation>(serialisedContent);
				if (null != info)
					info.CurrentTaskState = applicationTask.State;
				return info;
			}
			catch
			{
				// If we got nothing then return something empty.
				return new TaskInformation()
				{
					StatusDetails = serialisedContent
				} as TTaskInformation;
			}
		}
		public static TaskInformation RetrieveTaskInfo(this ApplicationTask applicationTask, string serverId)
		{
			return applicationTask.RetrieveTaskInfo<TaskInformation>(serverId);
		}
		/// <summary>
		/// Extracts the remarks from the <paramref name="taskInfo"/>'s latest update,
		/// and attempts to deserialise it into a <typeparamref name="TTaskInformation"/>.
		/// </summary>
		/// <typeparam name="TTaskInformation">The expected type of data.</typeparam>
		/// <param name="applicationTaskInfo">The task to retrieve data for.</param>
		/// <returns>the latest update, if available.</returns>
		public static TTaskInformation RetrieveTaskInfo<TTaskInformation>(this ApplicationTaskInfo applicationTaskInfo, string serverId)
			where TTaskInformation : TaskInformation
		{
			return applicationTaskInfo?.ToApplicationTask()?.RetrieveTaskInfo<TTaskInformation>(serverId);
		}
		public static TaskInformation RetrieveTaskInfo(this ApplicationTaskInfo applicationTaskInfo, string serverId)
		{
			return applicationTaskInfo?.ToApplicationTask()?.RetrieveTaskInfo<TaskInformation>(serverId);
		}

		/// <summary>
		/// Returns the directive of the <paramref name="taskInfo"/> a parsed/populated instance.
		/// </summary>
		/// <typeparam name="TDirective">The type of the directive.</typeparam>
		/// <param name="applicationTaskInfo">The task to retrieve data for.</param>
		/// <returns>The directive, or null if no directive is found.</returns>
		public static TDirective GetDirective<TDirective>(this ApplicationTaskInfo applicationTaskInfo)
			where TDirective : TaskDirective
		{
			return TaskDirective.Parse<BackgroundOperationTaskDirective>(applicationTaskInfo.TaskData)?.GetParsedInternalDirective<TDirective>();
		}

		/// <summary>
		/// Returns the directive of the <paramref name="taskInfo"/> a parsed/populated instance.
		/// </summary>
		/// <param name="applicationTaskInfo">The task to retrieve data for.</param>
		/// <returns>The directive, or null if no directive is found.</returns>
		public static TaskDirective GetDirective(this ApplicationTaskInfo applicationTaskInfo)
		{
			return TaskDirective.Parse<BackgroundOperationTaskDirective>(applicationTaskInfo.TaskData)?.GetParsedInternalDirective();
		}
	}
}