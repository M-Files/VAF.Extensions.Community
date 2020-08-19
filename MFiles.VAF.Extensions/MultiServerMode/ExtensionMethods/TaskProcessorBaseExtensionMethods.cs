using MFiles.VAF.MultiserverMode;
using System;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// Contains helper methods for <see cref="TaskProcessorBase{TSettings}"/>.
	/// </summary>
	public static class TaskProcessorBaseExtensionMethods
	{
		/// <summary>
		/// Adds a task to the task queue, optionally with a directive.
		/// </summary>
		/// <typeparam name="TSettings">The settings type used by the task processor.</typeparam>
		/// <typeparam name="TTaskQueueDirectiveType">The directive type to add.</typeparam>
		/// <param name="taskProcessor">The task processor to add the task to.</param>
		/// <param name="taskQueue">The queue ID to add this task to.</param>
		/// <param name="taskType">The task type.</param>
		/// <param name="directive">The directive - if any - to associate with the job.</param>
		/// <param name="allowRetry">Whether to allow retries if needed.</param>
		/// <param name="activationTimestamp">The datetime to activate the task (otherwise ASAP).</param>
		/// <returns></returns>
		public static string AddTask<TSettings, TTaskQueueDirectiveType>
		(
			this TaskProcessorBase<TSettings> taskProcessor,
			string taskQueue,
			string taskType,
			TTaskQueueDirectiveType directive = null,
			bool allowRetry = true,
			DateTime? activationTimestamp = null
		)
		where TTaskQueueDirectiveType : TaskQueueDirective
		where TSettings : AppTaskProcessorSettings
		{
			// Use the CreateApplicationTaskSafe method.
			return taskProcessor.CreateApplicationTaskSafe
			(
				allowRetry,
				taskQueue,
				taskType,
				directive?.ToBytes(),
				activationTimestamp ?? default
			);
		}
		
		/// <summary>
		/// Adds a task to the task queue, optionally with a directive.
		/// </summary>
		/// <typeparam name="TSettings">The settings type used by the task processor.</typeparam>
		/// <param name="taskProcessor">The task processor to add the task to.</param>
		/// <param name="taskQueue">The queue ID to add this task to.</param>
		/// <param name="taskType">The task type.</param>
		/// <param name="directive">The directive - if any - to associate with the job.</param>
		/// <param name="allowRetry">Whether to allow retries if needed.</param>
		/// <param name="activationTimestamp">The datetime to activate the task (otherwise ASAP).</param>
		public static string AddTask<TSettings>
		(
			this TaskProcessorBase<TSettings> taskProcessor,
			string taskQueue,
			string taskType,
			TaskQueueDirective directive = null,
			bool allowRetry = true,
			DateTime? activationTimestamp = null
		)
			where TSettings : AppTaskProcessorSettings
		{
			// Use the other overload
			return taskProcessor.AddTask<TSettings, TaskQueueDirective>
			(
				taskQueue,
				taskType,
				directive,
				allowRetry: allowRetry,
				activationTimestamp
			);
		}
	}
}
