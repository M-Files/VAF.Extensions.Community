using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class TaskManagerEx<TConfiguration>
	{
		/// <summary>
		/// Cancels all future exeuctions of a given task type on a given queue.
		/// </summary>
		/// <param name="queueId">The queue ID.</param>
		/// <param name="taskType">The task type ID (null or not provided means all tasks on the queue).</param>
		/// <param name="includeCurrentlyExecuting">Whether to attempt to cancel executing tasks.</param>
		/// <param name="vault">The vault reference to use.</param>
		public void CancelAllFutureExecutions
		(
			string queueId,
			string taskType = null,
			bool includeCurrentlyExecuting = true,
			Vault vault = null,
			string remarks = null
		)
		{
			this.CancelAllFutureExecutions<TaskDirective>(queueId, taskType, includeCurrentlyExecuting, vault, remarks);
		}

		/// <summary>
		/// Cancels all future exeuctions of a given task type on a given queue.
		/// </summary>
		/// <typeparam name="TDirective">The type of the directive for this task type.</typeparam>
		/// <param name="queueId">The queue ID.</param>
		/// <param name="taskType">The task type ID (null or not provided means all tasks on the queue).</param>
		/// <param name="includeCurrentlyExecuting">Whether to attempt to cancel executing tasks.</param>
		/// <param name="vault">The vault reference to use.</param>
		public void CancelAllFutureExecutions<TDirective>
		(
			string queueId,
			string taskType = null,
			bool includeCurrentlyExecuting = true,
			Vault vault = null,
			string remarks = null
		)
			where TDirective : TaskDirective
		{
			this.Logger?.Debug($"Cancelling future executions on queue {queueId} of task {taskType}.");
			var tasks = this.GetPendingExecutions<TDirective>(queueId, taskType, includeCurrentlyExecuting);
			foreach (var task in tasks)
				this.CancelExecution(task, vault, remarks);
		}

		/// <summary>
		/// Cancels a specific execution.
		/// </summary>
		/// <param name="task">The task to cancel.</param>
		/// <param name="vault">The vault reference to cancel using.</param>
		public void CancelExecution
		(
			TaskInfo<TaskDirective> task, 
			Vault vault = null,
			string remarks = null
		)
		{
			this.CancelExecution<TaskDirective>(task, vault, remarks);
		}

		/// <summary>
		/// Cancels a specific execution.
		/// </summary>
		/// <typeparam name="TDirective">The type of the directive for this task type.</typeparam>
		/// <param name="task">The task to cancel.</param>
		/// <param name="vault">The vault reference to cancel using.</param>
		public void CancelExecution<TDirective>
		(
			TaskInfo<TDirective> task, 
			Vault vault = null,
			string remarks = null
		)
			where TDirective : TaskDirective
		{
			if (null == task)
				return;

			switch (task.State)
			{
				case MFTaskState.MFTaskStateInProgress:
					this.CancelActiveTask
					(
						vault ?? this.Vault,
						task.TaskId,
						remarks
					);
					break;
				case MFTaskState.MFTaskStateWaiting:
					this.CancelWaitingTask
					(
						vault ?? this.Vault,
						task.TaskId
					);
					break;
				default:
					// Cannot cancel ones in other states.
					break;
			}
		}

		/// <summary>
		/// Returns executions of items on queue <paramref name="queueId"/>
		/// with optional type of <paramref name="taskType"/> in state(s) <paramref name="taskStates"/>.
		/// </summary>
		/// <param name="queueId">The ID of the queue to query.</param>
		/// <param name="taskType">The type of task to filter by</param>
		/// <typeparam name="TDirective">The type of directive used.</typeparam>
		/// <returns>Any matching executions.</returns>
		public IEnumerable<TaskInfo<TDirective>> GetPendingExecutions<TDirective>
		(
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
			return this.GetExecutions<TDirective>
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
		/// <param name="taskType">The type of task to filter by</param>
		/// <typeparam name="TDirective">The type of directive used.</typeparam>
		/// <returns>Any matching executions.</returns>
		public IEnumerable<TaskInfo<TDirective>> GetAllExecutions<TDirective>
		(
			string queueId,
			string taskType = null
		)
			where TDirective : TaskDirective
		{
			return this.GetExecutions<TDirective>
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
		/// <param name="taskStates">The states to find tasks in.</param>
		/// <param name="taskType">The type of task to filter by</param>
		/// <typeparam name="TDirective">The type of directive used.</typeparam>
		/// <returns>Any matching executions.</returns>
		public IEnumerable<TaskInfo<TDirective>> GetExecutions<TDirective>
		(
			string queueId,
			string taskType = null,
			params MFTaskState[] taskStates
		)
			where TDirective : TaskDirective
		{
			var query = new TaskQuery();
			query.Queue(queueId);
			if (false == string.IsNullOrWhiteSpace(taskType))
				query.TaskType(taskType);
			query.TaskState(taskStates);

			return query
				.FindTasks<TDirective>(this);
		}
	}
}
