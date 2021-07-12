using MFiles.VAF.AppTasks;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public static class TaskManagerExtensionMethods
	{

		/// <summary>
		/// Returns executions of items on queue <paramref name="queueId"/>
		/// with optional type of <paramref name="taskType"/> in state(s) <paramref name="taskStates"/>.
		/// </summary>
		/// <param name="queueId">The ID of the queue to query.</param>
		/// <param name="taskManager">The task manager to use to query.</param>
		/// <param name="taskType">The type of task to filter by</param>
		/// <typeparam name="TDirective">The type of directive used.</typeparam>
		/// <returns>Any matching executions.</returns>
		public static IEnumerable<TaskInfo<TDirective>> GetPendingExecutions<TDirective>
		(
			this TaskManager taskManager, 
			string queueId,
			string taskType = null,
			bool includeCurrentlyExecuting = true
		)
			where TDirective : TaskDirective
		{
			// What state should the tasks be in?
			var taskStates = includeCurrentlyExecuting
				? new[] { MFTaskState.MFTaskStateWaiting, MFTaskState.MFTaskStateInProgress }
				: new[] { MFTaskState.MFTaskStateWaiting };

			// Use the other overload.
			return taskManager.GetExecutions<TDirective>
			(
				queueId,
				taskType,
				taskStates
			);
		}

		/// <summary>
		/// Returns executions of items on queue <paramref name="queueId"/>
		/// with optional type of <paramref name="taskType"/> in state(s) <paramref name="taskStates"/>.
		/// </summary>
		/// <param name="queueId">The ID of the queue to query.</param>
		/// <param name="taskManager">The task manager to use to query.</param>
		/// <param name="taskType">The type of task to filter by</param>
		/// <typeparam name="TDirective">The type of directive used.</typeparam>
		/// <returns>Any matching executions.</returns>
		public static IEnumerable<TaskInfo<TDirective>> GetAllExecutions<TDirective>
		(
			this TaskManager taskManager,
			string queueId,
			string taskType = null
		)
			where TDirective : TaskDirective
		{
			return taskManager.GetExecutions<TDirective>
			(
				queueId,
				taskType,
				MFTaskState.MFTaskStateWaiting,
				MFTaskState.MFTaskStateInProgress,
				// If we include cancelled then we get lots of stuff that's not wanted.
				// MFTaskState.MFTaskStateCanceled, 
				MFTaskState.MFTaskStateCompleted,
				MFTaskState.MFTaskStateFailed
			);
		}

		/// <summary>
		/// Returns executions of items on queue <paramref name="queueId"/>
		/// with optional type of <paramref name="taskType"/> in state(s) <paramref name="taskStates"/>.
		/// </summary>
		/// <param name="queueId">The ID of the queue to query.</param>
		/// <param name="taskManager">The task manager to use to query.</param>
		/// <param name="taskStates">The states to find tasks in.</param>
		/// <param name="taskType">The type of task to filter by</param>
		/// <typeparam name="TDirective">The type of directive used.</typeparam>
		/// <returns>Any matching executions.</returns>
		public static IEnumerable<TaskInfo<TDirective>> GetExecutions<TDirective>
		(
			this TaskManager taskManager,
			string queueId,
			string taskType = null,
			params MFTaskState[] taskStates
		)
			where TDirective : TaskDirective
		{
			var query = new TaskQuery();
			query.Queue(queueId);
			if(false == string.IsNullOrWhiteSpace(taskType))
				query.TaskType(taskType);
			query.TaskState(taskStates);

			return query
				.FindTasks<TDirective>(taskManager);
		}
	}
}
