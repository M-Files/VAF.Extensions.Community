using MFiles.VAF.AppTasks;
using MFiles.VaultApplications.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Copied from CK; may move into VAF in the future so we can drop it from here.
 */


namespace MFiles.VAF.Extensions
{	
	/// <summary>
	/// Logs events emitted by a <see cref="VAF.AppTasks.TaskManager"/>.
	/// Automatically disposed when the <see cref="TaskManager"/> is shutdown.
	/// </summary>
	public class TaskManagerLogger : IDisposable
	{
		/// <summary>
		/// The task manager.
		/// </summary>
		public TaskManager TaskManager { get; }

		/// <summary>
		/// The log target.
		/// </summary>
		private ILogger logger = LogManager.GetLogger(typeof(TaskManagerLogger));

		/// <summary>
		/// Keeps track of whether the instance has been disposed.
		/// </summary>
		private bool isDisposed;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="taskManager">The task manager to log events from.</param>
		public TaskManagerLogger(TaskManager taskManager)
		{
			// Sanity check.
			if (taskManager == null)
				throw new ArgumentNullException(nameof(taskManager));

			// Set members.
			this.TaskManager = taskManager;

			// Set ourselves to cleanup when the task manager is shutdown.
			this.TaskManager.ShutdownToken.Register(() => Dispose());

			// Map events to their log handlers.
			logMethodsByEvent = new Dictionary<TaskManagerEventType, Action<TaskManagerEventArgs>>
			{
				{ TaskManagerEventType.ProcessingStarted, LogProcessingStartedEvent },
				{ TaskManagerEventType.ProcessingStopped, LogProcessingStoppedEvent },
				{ TaskManagerEventType.Stopping, LogStoppingEvent },
				{ TaskManagerEventType.Shutdown, LogShutdownEvent },
				{ TaskManagerEventType.ProcessLoopStarting, LogProcessLoopStartingEvent },
				{ TaskManagerEventType.ProcessLoopCanceled, LogProcessLoopCanceledEvent },
				{ TaskManagerEventType.ProcessLoopFailed, LogProcessLoopFailedEvent },
				{ TaskManagerEventType.ProcessLoopCompleted, LogProcessLoopCompletedEvent },
				{ TaskManagerEventType.QueueRegistered, LogQueueRegisteredEvent },
				{ TaskManagerEventType.QueueStopping, LogQueueStoppingEvent },
				{ TaskManagerEventType.QueueShutdown, LogQueueShutdownEvent },
				{ TaskManagerEventType.QueueDetached, LogQueueDetachedEvent },
				{ TaskManagerEventType.BroadcastsPolled, LogBroadcastsPolledEvent },
				{ TaskManagerEventType.BroadcastPollFailed, LogBroadcastPollFailedEvent },
				{ TaskManagerEventType.TasksPolled, LogTasksPolledEvent },
				{ TaskManagerEventType.TaskPollFailed, LogTaskPollFailedEvent },
				{ TaskManagerEventType.BroadcastParseError, LogBroadcastParseErrorEvent },
				{ TaskManagerEventType.BroadcastsProcessed, LogBroadcastsProcessedEvent },
				{ TaskManagerEventType.BroadcastProcessingFailed, LogBroadcastProcessingFailedEvent },
				{ TaskManagerEventType.TaskJobStarted, LogTaskJobStartedEvent },
				{ TaskManagerEventType.TaskJobRestarted, LogTaskJobRestartedEvent },
				{ TaskManagerEventType.TaskUpdated, LogTaskUpdatedEvent },
				{ TaskManagerEventType.TaskUpdateFailed, LogTaskUpdateFailedEvent },
				{ TaskManagerEventType.WaitingTasksCanceled, LogWaitingTasksCanceledEvent },
				{ TaskManagerEventType.TaskUpdateSkipped, LogTaskUpdateSkippedEvent },
				{ TaskManagerEventType.TaskJobFinished, LogTaskJobFinishedEvent },
				{ TaskManagerEventType.TaskAdded, LogTaskAddedEvent }
			};

			// Start listening.
			RegisterListeners();
		}

		/// <summary>
		/// Registers task manager listeners to detect events that should be logged.
		/// </summary>
		private void RegisterListeners()
		{
			// Register listeners.
			TaskManager.LifeCycleEvent += OnLifeCycleEvent;
			TaskManager.ProcessLoopEvent += OnProcessLoopEvent;
			TaskManager.QueueEvent += OnQueueEvent;
			TaskManager.PollEvent += OnPollEvent;
			TaskManager.BroadcastEvent += OnBroadcastEvent;
			TaskManager.TaskEvent += OnTaskEvent;
		}

		/// <summary>
		/// Unregisters task manager listeners.
		/// </summary>
		private void UnregisterListeners()
		{
			// Register listeners.
			TaskManager.LifeCycleEvent -= OnLifeCycleEvent;
			TaskManager.ProcessLoopEvent -= OnProcessLoopEvent;
			TaskManager.QueueEvent -= OnQueueEvent;
			TaskManager.PollEvent -= OnPollEvent;
			TaskManager.BroadcastEvent -= OnBroadcastEvent;
			TaskManager.TaskEvent -= OnTaskEvent;
		}

		#region Event handlers

		/// <summary>
		/// Log handlers by event type.
		/// </summary>
		private Dictionary<TaskManagerEventType, Action<TaskManagerEventArgs>> logMethodsByEvent;

		/// <summary>
		/// Delegates an event to the correct logger.
		/// </summary>
		/// <param name="category">Category of event. Based on event emitter.</param>
		/// <param name="args">The event args.</param>
		private void OnEvent(
			string category,
			TaskManagerEventArgs args
		)
		{
			// Log the event.
			Action<TaskManagerEventArgs> logMethod;
			if (this.logMethodsByEvent.TryGetValue(args.EventType, out logMethod))
				logMethod(args);
			else
				this.logger.Debug($"Unknown {category} event type: {args.EventType}. "
					+ GetEventDataString(args));
		}

		/// <summary>
		/// Listener for life cycle events.
		/// </summary>
		/// <param name="sender">Object that emitted the event.</param>
		/// <param name="args">Event arguments.</param>
		private void OnLifeCycleEvent(
			object sender,
			TaskManagerEventArgs args
		)
		{
			// Delegate.
			OnEvent("life-cycle", args);
		}

		/// <summary>
		/// Listener for process loop events.
		/// </summary>
		/// <param name="sender">Object that emitted the event.</param>
		/// <param name="args">Event arguments.</param>
		private void OnProcessLoopEvent(
			object sender,
			TaskManagerEventArgs args
		)
		{
			// Delegate.
			OnEvent("process-loop", args);
		}

		/// <summary>
		/// Listener for queue events.
		/// </summary>
		/// <param name="sender">Object that emitted the event.</param>
		/// <param name="args">Event arguments.</param>
		private void OnQueueEvent(
			object sender,
			TaskManagerEventArgs args
		)
		{
			// Delegate.
			OnEvent("queue", args);
		}

		/// <summary>
		/// Listener for poll events.
		/// </summary>
		/// <param name="sender">Object that emitted the event.</param>
		/// <param name="args">Event arguments.</param>
		private void OnPollEvent(
			object sender,
			TaskManagerEventArgs args
		)
		{
			// Delegate.
			OnEvent("poll", args);
		}

		/// <summary>
		/// Listener for broadcast events.
		/// </summary>
		/// <param name="sender">Object that emitted the event.</param>
		/// <param name="args">Event arguments.</param>
		protected virtual void OnBroadcastEvent(
			object sender,
			TaskManagerEventArgs args
		)
		{
			// Delegate.
			OnEvent("broadcast", args);
		}

		/// <summary>
		/// Listener for task events.
		/// </summary>
		/// <param name="sender">Object that emitted the event.</param>
		/// <param name="args">Event arguments.</param>
		protected virtual void OnTaskEvent(
			object sender,
			TaskManagerEventArgs args
		)
		{
			// Delegate.
			OnEvent("task", args);
		}

		#endregion Event handlers

		#region Specific event loggers

		#region Life cycle events

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.ProcessingStarted"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogProcessingStartedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Info("Task Manager message processing loop started.");
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.ProcessingStopped"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogProcessingStoppedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Task Manager message processing loop stopped.");
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.Stopping"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogStoppingEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Info("Task Manager is shutting down.");
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.Shutdown"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogShutdownEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Info("Task Manager shut down completed.");
		}

		#endregion Life cycle events

		#region Process cycle events

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.ProcessLoopStarting"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogProcessLoopStartingEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Trace("Process loop cycle starting.");
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.ProcessLoopCanceled"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogProcessLoopCanceledEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Process loop cycle canceled.");
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.ProcessLoopFailed"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogProcessLoopFailedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Error(args.Exception, "Process loop cycle failed.");
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.ProcessLoopCompleted"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogProcessLoopCompletedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Trace("Process loop cycle completed.");
		}

		#endregion Process cycle events

		#region Queue events

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.QueueRegistered"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogQueueRegisteredEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Task queue registered. Queue:" + args.Queues[0]);
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.QueueStopping"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogQueueStoppingEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Task queue stopping. Queue:" + args.Queues[0]);
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.QueueShutdown"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogQueueShutdownEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Task queue shut down. Queue:" + args.Queues[0]);
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.QueueDetached"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogQueueDetachedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Task queue detached. Queue:" + args.Queues[0]);
		}

		#endregion Queue events

		#region Polling events

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.BroadcastsPolled"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogBroadcastsPolledEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Trace("Broadcasts polled. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.BroadcastPollFailed"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogBroadcastPollFailedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Error(args.Exception, "Broadcast poll failed. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TasksPolled"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTasksPolledEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Trace("Tasks polled. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskPollFailed"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskPollFailedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Error(args.Exception, "Task poll failed. " + GetEventDataString(args));
		}

		#endregion Polling events

		#region Broadcast events

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.BroadcastParseError"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogBroadcastParseErrorEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Error(args.Exception,
					"Failed to parse broadcast message. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.BroadcastsProcessed"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogBroadcastsProcessedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Processed broadcast message(s). " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.BroadcastProcessingFailed"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogBroadcastProcessingFailedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Error(args.Exception,
					"Failed to process broadcast message(s). " + GetEventDataString(args));
		}

		#endregion Broadcast events

		#region Task events

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskJobStarted"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskJobStartedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Task job starting. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskJobRestarted"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskJobRestartedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug("Task job re-starting. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskUpdated"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskUpdatedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Trace("Task job status updated. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskUpdateFailed"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskUpdateFailedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Warn(args.Exception,
					"Failed to update job status. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.WaitingTasksCanceled"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogWaitingTasksCanceledEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Debug(args.Exception,
					"Job canceled. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskUpdateSkipped"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskUpdateSkippedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Trace("Task job status update skipped. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskJobFinished"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskJobFinishedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			if (args.JobResult == TaskProcessingJobResult.Fatal)
			{
				// Something went badly wrong.
				this.logger?.Error
				(
					args.Exception,
					$"Job(s) {string.Join(", ", args.Tasks?.Select(t => t.TaskID))} finished with a fatal result: {args.JobStatus.ErrorMessage}"
				);
			}
			else
				this.logger.Debug("Task job finished. " + GetEventDataString(args));
		}

		/// <summary>
		/// Logs <see cref="TaskManagerEventType.TaskAdded"/> events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void LogTaskAddedEvent(TaskManagerEventArgs args)
		{
			// Log the event.
			this.logger.Trace("Task added. " + GetEventDataString(args));
		}

		#endregion Task events

		#endregion Specific event loggers

		#region Format helpers

		/// <summary>
		/// Generates a data string based on the values available in an event.
		/// </summary>
		/// <param name="args">The event args.</param>
		/// <returns>A string describing the event data.</returns>
		protected virtual string GetEventDataString(TaskManagerEventArgs args)
		{
			// Create a string with included data.
			var parts = new List<string>();
			IncludeListData(parts, args.Queues, s => s, "Queue", "Queues");
			IncludeData(parts, args.TaskType, "TaskType");
			IncludeListData(parts, args.Tasks, t => t.TaskID, "Task", "Tasks");
			IncludeListData(parts, args.Broadcasts, b => b.BroadcastMessageID, "Broadcast", "Broadcasts");
			IncludeData(parts, $"{args.JobResult}", "Result");
			return String.Join("; ", parts);
		}

		/// <summary>
		/// Adds a label/value string to a list of data, if the value is not empty.
		/// </summary>
		/// <param name="data">The data list.</param>
		/// <param name="value">The value.</param>
		/// <param name="label">The label.</param>
		protected virtual void IncludeData(
			List<string> data,
			string value,
			string label
		)
		{
			// Add the label/value pair if there is a value.
			if (!String.IsNullOrWhiteSpace(value))
				data.Add(label + ": " + value);
		}

		/// <summary>
		/// Adds a label/value-list string to a list of data, if the list value is not empty.
		/// </summary>
		/// <typeparam name="T">The type of data the value list holds.</typeparam>
		/// <param name="data">The data list.</param>
		/// <param name="list">The value list.</param>
		/// <param name="callback">A callback to convert the list items to strings.</param>
		/// <param name="label">The label.</param>
		/// <param name="labelPlural">The plural label to use, if there is more than one item in the list.</param>
		protected virtual void IncludeListData<T>(
			List<string> data,
			IEnumerable<T> list,
			Func<T, string> callback,
			string label,
			string labelPlural = null
		)
		{
			// Skip if there is no data.
			if (list == null || !list.Any())
				return;

			// Resolve the label to use.
			string bestLabel = label;
			if (!string.IsNullOrEmpty(labelPlural) && list.Count() > 0)
				bestLabel = labelPlural;

			// Resolve the value.
			string value = String.Join(", ", list.Select(callback));

			// Append to the data.
			IncludeData(data, value, bestLabel);
		}

		#endregion Format helpers

		#region IDisposable

		/// <summary>
		/// Dispose implementation.
		/// </summary>
		/// <param name="disposing">Indicates if the other dispose overload was called.</param>
		protected virtual void Dispose(bool disposing)
		{
			// Skip if we've already been disposed.
			if (!this.isDisposed)
			{
				// Check if we are being deliberatley disposed.
				if (disposing)
					UnregisterListeners();

				// Update dispose flag.
				this.isDisposed = true;
			}
		}

		/// <summary>
		/// Disposes the logger.
		/// </summary>
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method.
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable
	}
}
