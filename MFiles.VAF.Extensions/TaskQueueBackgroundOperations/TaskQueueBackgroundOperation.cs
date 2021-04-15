using System;
using System.Linq;
using System.Threading;
using MFiles.VAF.Common;
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Extensions.ScheduledExecution;
using MFiles.VAF;
using MFilesAPI;
using MFiles.VAF.MultiserverMode;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
{
	public class TaskQueueBackgroundOperation<TDirective>
		: TaskQueueBackgroundOperation
		where TDirective : TaskQueueDirective
	{
		/// <summary>
		/// The method to run.
		/// </summary>
		public new Action<TaskProcessorJobEx, TDirective> UserMethod { get; private set; }

		/// <summary>
		/// Creates a new background operation that runs the method in separate task.
		/// </summary>
		/// <param name="name">The name of the background operation.</param>
		/// <param name="method">The method to invoke. The background operation will be passed to the method.</param>
		/// <param name="backgroundOperationManager">The background operation manager that manages this operation.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		public TaskQueueBackgroundOperation
		(
			TaskQueueBackgroundOperationManager backgroundOperationManager,
			string name,
			Action<TaskProcessorJobEx, TDirective> method,
			CancellationTokenSource cancellationTokenSource = default
		) : 
			base
				(
				backgroundOperationManager, 
				name,
				(j, d) => { }, // Ignore the method (we will set it below).
				cancellationTokenSource
				)
		{
			// Save parameters.
			this.UserMethod = method ?? throw new ArgumentNullException( nameof(method) );
		}

		/// <inheritdoc />
		internal override void RunJob(TaskProcessorJobEx job, TaskQueueDirective directive)
		{
			// Execute the callback.
			this.UserMethod(job, directive as TDirective);
		}

		/// <summary>
		/// Runs the operation at once or immediately after the current run is finished.
		/// </summary>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		/// <param name="vault">The vault reference to add the task.  Set to a transactional vault to only run the task if the transaction completes.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce
		(
			DateTime? runAt = null,
			TDirective directive = null,
			Vault vault = null
		)
		{
			// Use the base implementation.
			base.RunOnce(runAt, directive, vault: vault);
		}

		/// <summary>
		/// Begins running the operation at given intervals. If a run takes longer than the interval, the next run starts immediately after the previous run.
		/// </summary>
		/// <param name="interval">The interval between consecutive runs.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		public void RunAtIntervals(TimeSpan interval, TDirective directive = null)
		{
			// Use the base implementation.
			base.RunAtIntervals(interval, directive);
		}

		/// <summary>
		/// Begins running the operation according to the given schedule.
		/// </summary>
		/// <param name="schedule">The schedule to run on.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		public void RunOnSchedule(Schedule schedule, TDirective directive = null)
		{
			// Use the base implementation.
			base.RunOnSchedule(schedule, directive);
		}
	}

	public enum TaskQueueBackgroundOperationRepeatType
	{
		/// <summary>
		/// The background operation does not have any explicit repetition.
		/// </summary>
		NotRepeating = 0,

		/// <summary>
		/// The background operation repeats on a period (e.g. every 10 minutes).
		/// </summary>
		Interval = 1,

		/// <summary>
		/// The background operation repeats on a schedule (e.g. On Mondays and Wednesdays at 1pm).
		/// </summary>
		Schedule = 2
	}

	public class TaskQueueBackgroundOperation
	{
		/// <summary>
		/// The task type for this background operation.
		/// </summary>
		public const string TaskTypeId = "VaultApplication-BackgroundOperation";
		public const string DefaultRunCommandDisplayText = "Run now";
		public const string DefaultRunCommandConfirmationText = null;
		public const string DefaultRunCommandSuccessText = "The background operation has been scheduled to run.";

		/// <summary>
		/// The text shown to the user as a popup when the background operation has been scheduled.
		/// </summary>
		public string RunCommandSuccessText { get; set; } = DefaultRunCommandSuccessText;

		/// <summary>
		/// Whether to show the run command in the dashboard.
		/// If true, the dashboard will render a "Run now" button that will allow the user
		/// to force a run of the background operation, even if the schedule does not
		/// require it to run immediately.
		/// </summary>
		public bool ShowRunCommandInDashboard { get; set; } = false;

		/// <summary>
		/// Whether to show the background operation in the dashboard.
		/// </summary>
		public bool ShowBackgroundOperationInDashboard { get; set; } = true;

		/// <summary>
		/// The description of the background operation.
		/// </summary>
		public string Description { get; set; } = null;

		/// <summary>
		/// The run command to be shown in the dashboard.
		/// </summary>
		public CustomDomainCommand DashboardRunCommand { get; private set; }
			= new CustomDomainCommand()
			{
				ConfirmMessage = DefaultRunCommandConfirmationText,
				DisplayName = DefaultRunCommandDisplayText,
				Blocking = true
			};

		/// <summary>
		/// The unique ID for this background operation.  Re-created on startup.
		/// </summary>
		public Guid ID { get; set; } = Guid.NewGuid();

		/// <summary>
		/// How the background operation should repeat.
		/// </summary>
		public TaskQueueBackgroundOperationRepeatType RepeatType { get; set; }
			= TaskQueueBackgroundOperationRepeatType.NotRepeating;

		/// <summary>
		/// The manager that manages this operation.
		/// </summary>
		public TaskQueueBackgroundOperationManager BackgroundOperationManager { get; private set; }

		/// <summary>
		/// The name of the vault running the background operation.
		/// </summary>
		public string VaultName => this
			.BackgroundOperationManager
			.VaultApplication?
			.PermanentVault?
			.Name;

		/// <summary>
		/// The name of the background operation.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The cancellation token for the operation.
		/// </summary>
		public CancellationTokenSource CancellationTokenSource { get; private set; }

		/// <summary>
		/// Flag to show whether the task has been cancelled.
		/// </summary>
		public bool ExplicitlyCancelled { get; set; }

		/// <summary>
		/// Is the operation cancelled. A cancelled operation cannot be used and is waiting to be disposed.
		/// </summary>
		public bool Cancelled => this.ExplicitlyCancelled || (this.CancellationTokenSource?.Token.IsCancellationRequested ?? false);

		/// <summary>
		/// The interval between runs, when the operation is recurring.
		/// Only used when <see cref="RepeatType"/> is <see cref="TaskQueueBackgroundOperationRepeatType.Interval"/>.
		/// </summary>
		public TimeSpan? Interval { get; private set; }

		/// <summary>
		/// The schedule for execution.
		/// Only used when <see cref="RepeatType"/> is <see cref="TaskQueueBackgroundOperationRepeatType.Schedule"/>.
		/// </summary>
		public Schedule Schedule { get; private set; }

		/// <summary>
		/// The method to run.
		/// </summary>
		public Action<TaskProcessorJobEx, TaskQueueDirective> UserMethod { get; private set; }

		/// <summary>
		/// Creates a new background operation that runs the method in separate task.
		/// </summary>
		/// <param name="name">The name of the background operation.</param>
		/// <param name="method">The method to invoke. The background operation will be passed to the method.</param>
		/// <param name="backgroundOperationManager">The background operation manager that manages this operation.</param>
		/// <param name="options">The options for the display of the background operation in the dashboard.</param>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		public TaskQueueBackgroundOperation
		(
			TaskQueueBackgroundOperationManager backgroundOperationManager,
			string name,
			Action<TaskProcessorJobEx, TaskQueueDirective> method,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("The background operation name cannot be null or whitespace.", nameof(name));

			// Save parameters.
			this.CancellationTokenSource = cancellationTokenSource;
			this.BackgroundOperationManager = backgroundOperationManager ?? throw new ArgumentNullException(nameof(backgroundOperationManager));
			this.UserMethod = method ?? throw new ArgumentNullException( nameof(method) );
			this.Name = name ?? throw new ArgumentNullException( nameof(name) );

			// Initialize default values.
			this.DashboardRunCommand.ID = $"cmdRunBackgroundOperation-{this.ID.ToString("N")}";
			this.DashboardRunCommand.Execute = (c, o) =>
			{
				// Try and run the background operation.
				this.RunOnce();

				// Refresh the dashboard.
				if (false == string.IsNullOrEmpty(this.RunCommandSuccessText))
					o.ShowMessage(this.RunCommandSuccessText);
				o.RefreshDashboard();
			};
		}

		/// <summary>
		/// Runs the operation at once.
		/// </summary>
		/// <param name="runAt">If specified, schedules an execution at the provided time.  Otherwise schedules a call immediately.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		/// <param name="vault">The vault reference to add the task.  Set to a transactional vault to only run the task if the transaction completes.</param>
		/// <remarks>Does not remove any scheduled executions.  Use <see cref="StopRunningAtIntervals"/>.</remarks>
		public void RunOnce(DateTime? runAt = null, TaskQueueDirective directive = null, Vault vault = null)
		{
			// Schedule the next task to execute ASAP.
			this.BackgroundOperationManager.RunOnce(this.Name, runAt, directive, vault: vault);
		}

		/// <summary>
		/// Begins running the operation at given intervals. If a run takes longer than the interval, the next run starts immediately after the previous run.
		/// </summary>
		/// <param name="interval">The interval between consecutive runs.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		public void RunAtIntervals(TimeSpan interval, TaskQueueDirective directive = null)
		{
			// Check for validity.
			if (interval < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException
				(
					nameof(interval),
					"The timer interval cannot be less than zero."
				);

			// Cancel any existing executions.
			this.StopRunningAtIntervals();

			// Set up the recurrance data.
			this.Interval = interval;
			this.RepeatType = TaskQueueBackgroundOperationRepeatType.Interval;

			// Run (which will set up the next iteration).
			this.RunOnce(directive: directive);
		}

		/// <summary>
		/// Begins running the operation according to the given schedule.
		/// </summary>
		/// <param name="schedule">The schedule to run on.</param>
		/// <param name="directive">The directive ("data") to pass to the execution.</param>
		public void RunOnSchedule(Schedule schedule, TaskQueueDirective directive = null)
		{
			// Check for validity.
			if (null == schedule)
				throw new ArgumentNullException(nameof(schedule));

			// Cancel any existing executions.
			this.StopRunningAtIntervals();

			// Set up the recurrance data.
			this.Schedule = schedule;
			this.RepeatType = TaskQueueBackgroundOperationRepeatType.Schedule;

			// Run (which will set up the next iteration).
			var nextRun = this.Schedule?.GetNextExecution();
			if(nextRun.HasValue)
				this.RunOnce(runAt: nextRun.Value, directive: directive);
		}

		/// <summary>
		/// Returns all pending executions of this background operation.
		/// </summary>
		/// <returns>Any pending executions of this background operation.</returns>
		public IEnumerable<ApplicationTaskInfo> GetPendingExecutions(bool includeCurrentlyExecuting = true)
		{
			// What state should the tasks be in?
			var taskStates = includeCurrentlyExecuting
				? new[] { MFTaskState.MFTaskStateWaiting, MFTaskState.MFTaskStateInProgress }
				: new[] { MFTaskState.MFTaskStateWaiting };

			// Use the other overload.
			return this.GetExecutions
			(
				taskStates
			);
		}

		/// <summary>
		/// Returns all pending executions of this background operation.
		/// </summary>
		/// <returns>Any pending executions of this background operation.</returns>
		public IEnumerable<ApplicationTaskInfo> GetAllExecutions()
		{
			return this.GetExecutions
			(
				MFTaskState.MFTaskStateWaiting,
				MFTaskState.MFTaskStateInProgress,
				// If we include cancelled then we get lots of stuff that's not wanted.
				// MFTaskState.MFTaskStateCanceled, 
				MFTaskState.MFTaskStateCompleted,
				MFTaskState.MFTaskStateFailed
			);
		}

		/// <summary>
		/// Returns all pending executions of this background operation.
		/// </summary>
		/// <returns>Any pending executions of this background operation.</returns>
		public IEnumerable<ApplicationTaskInfo> GetExecutions(params MFTaskState[] taskStates)
		{
			// Sanity.
			taskStates = taskStates ?? new MFTaskState[0];

			// Find all tasks.
			var scheduledTasks = TaskQueueAdministrator.FindTasks
			(
				this.BackgroundOperationManager.VaultApplication.PermanentVault,
				this.BackgroundOperationManager.QueueId,
				t => t.Type == TaskQueueBackgroundOperation.TaskTypeId,
				taskStates
			).Cast<ApplicationTaskInfo>();
			return scheduledTasks.Where(t =>
			{
				// If this task is not for this background operation then ignore it.
				var directive = TaskQueueDirective.Parse<BackgroundOperationTaskQueueDirective>(t.ToApplicationTask());
				if (null == directive)
					return false;
				if (directive.BackgroundOperationName != this.Name)
					return false;
				return true;
			});
		}

		/// <summary>
		/// Marks any future executions of this job in this queue as cancelled.
		/// </summary>
		public void CancelFutureExecutions(string remarks = null)
		{
			try
			{
				foreach (var task in this.GetPendingExecutions())
				{
					// Mark each task as superseded.
					try
					{
						this.BackgroundOperationManager.TaskProcessor.UpdateCancelledJobInTaskQueue
						(
							task.ToApplicationTask(),
							string.Empty,
							remarks
						);
					}
					catch (Exception e)
					{
						SysUtils.ReportErrorToEventLog
						(
							$"Exception cancelling task {task.ToApplicationTask().Id} for background operation {this.Name} of type {TaskQueueBackgroundOperation.TaskTypeId} on queue {this.BackgroundOperationManager.QueueId}.",
							e
						);
					}
				}
			}
			catch (Exception e)
			{
				SysUtils.ReportErrorToEventLog
				(
					$"Exception cancelling tasks for background operation {this.Name} of type {TaskQueueBackgroundOperation.TaskTypeId} on queue {this.BackgroundOperationManager.QueueId}.",
					e
				);
			}
		}

		/// <summary>
		/// Stops running the operation at intervals.
		/// Cancels any scheduled executions.
		/// </summary>
		public void StopRunningAtIntervals()
		{
			// Stop any new ones.
			this.RepeatType = TaskQueueBackgroundOperationRepeatType.NotRepeating;

			// Cancel any future executions.
			this.CancelFutureExecutions("Superseded.");
		}

		/// <summary>
		/// Cancels the operation. A cancelled operation cannot be used and is waiting to be disposed.
		/// </summary>
		public void Cancel()
		{
			this.CancellationTokenSource?.Cancel();
		}

		/// <summary>
		/// Called by the <see cref="TaskQueueBackgroundOperationManager"/> when a task for this background operation
		/// is scheduled.
		/// </summary>
		/// <param name="job">The job to run.</param>
		/// <param name="directive">The directive, if any, passed in.</param>
		internal virtual void RunJob(TaskProcessorJobEx job, TaskQueueDirective directive)
		{
			// Execute the callback.
			this.UserMethod(job, directive);
		}
	}
}