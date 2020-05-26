using MFiles.VAF.Common;
using MFiles.VAF.MultiserverMode;
using MFilesAPI;
using System;
using System.Threading;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// Provides an implementation of <see cref="TaskQueueBackgroundOperation{TDirective}"/> that does not
	/// require a generic type argument (defaults to <see cref="TaskQueueDirective"/>).
	/// </summary>
	public class TaskQueueBackgroundOperationManager
		: TaskQueueBackgroundOperationManager<TaskQueueDirective>
	{	
		/// <summary>
		/// Lock for <see cref="CurrentServer"/>.
		/// </summary>
		private static readonly object _lock = new object();

		/// <summary>
		/// The server that this application is running on.
		/// </summary>
		internal static VaultServerAttachment CurrentServer { get; private set; }

		/// <summary>
		/// Ensures that <see cref="CurrentServer"/> is set correctly.
		/// </summary>
		/// <param name="vaultApplication">The vault application where this code is running.</param>
		internal static void SetCurrentServer(VaultApplicationBase vaultApplication)
		{
			// Sanity.
			if (null == vaultApplication)
				throw new ArgumentNullException(nameof(vaultApplication));

			// Ensure that we have a current server.
			lock (TaskQueueBackgroundOperationManager._lock)
			{
				if (null == TaskQueueBackgroundOperationManager.CurrentServer)
				{
					TaskQueueBackgroundOperationManager.CurrentServer
						= vaultApplication
							.PermanentVault
							.GetVaultServerAttachments()
							.GetCurrent();
				}
			}
		}

		/// <inheritdoc />
		public TaskQueueBackgroundOperationManager
		(
			VaultApplicationBase vaultApplication,
			string queueId,
			CancellationTokenSource cancellationTokenSource = default
		)
			: base(vaultApplication, queueId, cancellationTokenSource)
		{
		}
	}

	/// <summary>
	/// Manages one or more background operations within a single queue.
	/// </summary>
	/// <typeparam name="TDirective">The directive type supported by the tasks in this queue.</typeparam>
	public class TaskQueueBackgroundOperationManager<TDirective>
		where TDirective : TaskQueueDirective
	{
		/// <summary>
		/// The queue Id.
		/// </summary>
		public string QueueId { get;set; }

		/// <summary>
		/// The vault application that contains this background operation manager.
		/// </summary>
		public VaultApplicationBase VaultApplication { get; private set; }

		/// <summary>
		/// The cancellation token source, if cancellation should be supported.
		/// </summary>
		public CancellationTokenSource CancellationTokenSource { get; private set; }

		/// <summary>
		/// Creates a background operation manager for a given queue.
		/// </summary>
		/// <param name="vaultApplication">The vault application that contains this background operation manager.</param>
		/// <param name="queueId">The queue Id.</param>
		/// <param name="cancellationTokenSource">The cancellation token source, if cancellation should be supported.</param>
		public TaskQueueBackgroundOperationManager
		(
			VaultApplicationBase vaultApplication,
			string queueId,
			CancellationTokenSource cancellationTokenSource = default
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(queueId))
				throw new ArgumentException("The queue id cannot be null or whitespace.", nameof(queueId));

			// Assign.
			this.CancellationTokenSource = cancellationTokenSource;
			this.VaultApplication = vaultApplication ?? throw new ArgumentNullException(nameof(vaultApplication));
			this.QueueId = queueId;

			// Ensure we have a current server.
			TaskQueueBackgroundOperationManager.SetCurrentServer(vaultApplication);
		}

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="taskTypeId">The type of the background task.  Must be unique within the <see cref="QueueId" />.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A scheduled background operation.</returns>
		public TaskQueueBackgroundOperation<TDirective> StartRecurringBackgroundOperation
		(
			string name,
			string taskTypeId,
			TimeSpan interval,
			Action method
		)
		{
			return this.StartRecurringBackgroundOperation
			(
				name,
				taskTypeId,
				interval,
				(v, d) => method()
			);
		}

		/// <summary>
		/// Creates a new background operation and starts it. The background operation runs the given method at given intervals.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="taskTypeId">The type of the background task.  Must be unique within the <see cref="QueueId" />.</param>
		/// <param name="interval">The target interval between method calls. If the method call takes longer than the interval, the method will be invoked immediately after the previous method call returns.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A started background operation.</returns>
		public TaskQueueBackgroundOperation<TDirective> StartRecurringBackgroundOperation
		(
			string name,
			string taskTypeId,
			TimeSpan interval,
			Action<Vault, TDirective> method
		)
		{
			// Create the background operation.
			var backgroundOperation = this.CreateBackgroundOperation
			(
				name,
				taskTypeId,
				method
			);
			
			// Start it running.
			backgroundOperation.RunAtIntervals(interval);

			// Return the background operation.
			return backgroundOperation;
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="taskTypeId">The type of the background task.  Must be unique within the <see cref="QueueId" />.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation<TDirective> CreateBackgroundOperation
		(
			string name, 
			string taskTypeId,
			Action method
		)
		{
			return this.CreateBackgroundOperation
			(
				name,
				taskTypeId,
				(v, d) => method()
			);
		}

		/// <summary>
		/// Creates a new background operation. The background operations runs the given method at given intervals. Must be separately started.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <param name="taskTypeId">The type of the background task.  Must be unique within the <see cref="QueueId" />.</param>
		/// <param name="method">The method to invoke at given intervals.</param>
		/// <returns>A new background operation, that is not yet started.</returns>
		public TaskQueueBackgroundOperation<TDirective> CreateBackgroundOperation
		(
			string name,
			string taskTypeId,
			Action<Vault, TDirective> method
		)
		{
			return new TaskQueueBackgroundOperation<TDirective>
			(
				this,
				taskTypeId,
				name,
				method,
				this.CancellationTokenSource
			);
		}

	}
}