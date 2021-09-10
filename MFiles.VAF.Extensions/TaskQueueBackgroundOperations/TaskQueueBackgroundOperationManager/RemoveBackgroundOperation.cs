using System;
using MFiles.VAF;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	public partial class TaskQueueBackgroundOperationManager
	{

		/// <summary>
		/// Removes a background operation by the name.
		/// </summary>
		/// <param name="name">The name of the operation.</param>
		/// <returns><see langword="true"/>if the operation could be removed.</returns>
		public bool RemoveBackgroundOperation
		(
			string name
		)
		{
			lock (TaskQueueBackgroundOperationManager._lock)
			{
				if (this.BackgroundOperations.ContainsKey(name) == false)
					throw new ArgumentException(
						$"No background operation with the name {name} in queue {this.QueueId} exists.",
						nameof(name));

				// Cancel all Future Executions
				this.BackgroundOperations[name].CancelFutureExecutions();

				// Remove it from the dictionary.
				return this.BackgroundOperations.Remove(name);
			}
		}
	}
}