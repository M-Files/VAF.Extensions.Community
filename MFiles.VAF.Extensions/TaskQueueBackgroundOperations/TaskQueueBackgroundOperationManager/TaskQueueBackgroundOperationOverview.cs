using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Linq;

namespace MFiles.VAF.Extensions
{
	public class TaskQueueBackgroundOperationOverview
	{
		private TaskQueueBackgroundOperationOverview()
		{
		}

		/// <summary>
		/// Returns the namespace for the background operation's overview configuration.
		/// </summary>
		/// <param name="backgroundOperation">The background operation to refer to.</param>
		/// <returns>The namespace location.</returns>
		private static string GetNamedValueStorageNamespaceFor(TaskQueueBackgroundOperation backgroundOperation)
		{
			// Sanity.
			if (null == backgroundOperation)
				throw new ArgumentNullException(nameof(backgroundOperation));

			// Get references to the background operation, manager and vault application.
			if (null == backgroundOperation)
				return null;
			var backgroundOperationManager = backgroundOperation.BackgroundOperationManager;
			if (null == backgroundOperationManager)
				return null;
			var vaultApplication = backgroundOperationManager?
				.VaultApplication;
			if (null == vaultApplication)
				return null;

			return $"{backgroundOperationManager.QueueId}-{backgroundOperation.Name}";
		}

		/// <summary>
		/// Loads an overview of the <paramref name="backgroundOperation"/> from NVS.
		/// </summary>
		/// <param name="backgroundOperation">The background operation to load the overview for.</param>
		/// <returns>The overview, or null if background operation is invalid.</returns>
		public static TaskQueueBackgroundOperationOverview Load(TaskQueueBackgroundOperation backgroundOperation)
		{
			// Sanity.
			if (null == backgroundOperation)
				throw new ArgumentNullException(nameof(backgroundOperation));

			// Get references to the background operation, manager and vault application.
			if (null == backgroundOperation)
				return null;
			var backgroundOperationManager = backgroundOperation.BackgroundOperationManager;
			if (null == backgroundOperationManager)
				return null;
			var vaultApplication = backgroundOperationManager?
				.VaultApplication;
			if (null == vaultApplication)
				return null;

			// Get the configuration data.
			var configuration = vaultApplication?
				.PermanentVault?
				.NamedValueStorageOperations?
				.GetNamedValues
				(
					MFNamedValueType.MFAdminConfiguration,
					GetNamedValueStorageNamespaceFor(backgroundOperation)
				) ?? new NamedValues();

			// Copy the configuration data to the overview.
			return new TaskQueueBackgroundOperationOverview(backgroundOperation, configuration);
		}

		/// <summary>
		/// Saves the current state of the background operation overview to NVS.
		/// </summary>
		public void Save()
		{
			// Get references to the background operation, manager and vault application.
			if (null == this.BackgroundOperation)
				return;
			var backgroundOperationManager = this.BackgroundOperation.BackgroundOperationManager;
			if (null == backgroundOperationManager)
				return;
			var vaultApplication = backgroundOperationManager?
				.VaultApplication;
			if (null == vaultApplication)
				return;

			// Get the configuration data.
			var configuration = new NamedValues();
			configuration["status"] = this.Status.ToString();
			configuration["lastRun"] = this.LastRun.HasValue
				? this.LastRun.Value.ToUniversalTime().ToString("O")
				: "";
			configuration["nextRun"] = this.NextRun.HasValue
				? this.NextRun.Value.ToUniversalTime().ToString("O")
				: "";

			// Set the configuration data.
			vaultApplication?
			   .PermanentVault?
			   .NamedValueStorageOperations?
			   .SetNamedValues
			   (
				   MFNamedValueType.MFAdminConfiguration,
				   GetNamedValueStorageNamespaceFor(this.BackgroundOperation),
				   configuration
			   );
		}

		/// <summary>
		/// Creates a background operation overview from the provided <paramref name="backgroundOperation"/> and <paramref name="configuration"/>.
		/// Note: use <see cref="TaskQueueBackgroundOperationOverview.Load(TaskQueueBackgroundOperation)"/> to create.
		/// </summary>
		/// <param name="backgroundOperation">The background operation the <paramref name="configuration"/> refers to.</param>
		/// <param name="configuration">The current configuration from NVS.</param>
		private TaskQueueBackgroundOperationOverview(TaskQueueBackgroundOperation backgroundOperation, NamedValues configuration)
		{
			// Sanity.
			if (null == backgroundOperation)
				throw new ArgumentNullException(nameof(backgroundOperation));
			if (null == configuration)
				return;

			this.BackgroundOperation = backgroundOperation;

			// Extract status.
			if (false == configuration.Names.Contains("status"))
				this.Status = TaskQueueBackgroundOperationStatus.Stopped;
			else if (Enum.TryParse(configuration["status"]?.ToString(), out TaskQueueBackgroundOperationStatus val))
				this.Status = val;
			else 
				this.Status = TaskQueueBackgroundOperationStatus.Stopped;

			// Extract last run.
			if (false == configuration.Names.Contains("lastRun"))
				this.LastRun = null;
			else if (DateTime.TryParse(configuration["lastRun"]?.ToString(), out DateTime val))
				this.LastRun = val;
			else
				this.LastRun = null;

			// Extract next run.
			if (false == configuration.Names.Contains("nextRun"))
				this.NextRun = null;
			else if (DateTime.TryParse(configuration["nextRun"]?.ToString(), out DateTime val))
				this.NextRun = val;
			else
				this.NextRun = null;
		}

		/// <summary>
		/// The background operation that is running.
		/// </summary>
		public TaskQueueBackgroundOperation BackgroundOperation { get; set; }

		/// <summary>
		/// The current status of this background operation.
		/// </summary>
		public TaskQueueBackgroundOperationStatus Status { get; set; }

		/// <summary>
		/// The last time that this background operation ran.
		/// </summary>
		public DateTime? LastRun { get; set; }

		/// <summary>
		/// The next time that this background operation will run.
		/// </summary>
		public DateTime? NextRun { get; set; }

	}
}