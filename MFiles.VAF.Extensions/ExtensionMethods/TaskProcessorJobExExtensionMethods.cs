using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Contains extension methods for working with task processor jobs.
	/// </summary>
	public static class TaskProcessorJobExExtensionMethods
	{
		/// <summary>
		/// Retrieves the background operation name.
		/// </summary>
		public static string GetBackgroundOperationName<TDirective, TSecureConfiguration>
		(
			this TaskProcessorJobEx<TDirective, TSecureConfiguration> taskProcessorJobEx
		)
			where TDirective : BackgroundOperationTaskDirective
			where TSecureConfiguration : class, new()
		{
			// Sanity.
			if (null == taskProcessorJobEx)
				throw new ArgumentNullException(nameof(taskProcessorJobEx));

			return taskProcessorJobEx.Job?.Directive?.BackgroundOperationName;
		}
	}
}
