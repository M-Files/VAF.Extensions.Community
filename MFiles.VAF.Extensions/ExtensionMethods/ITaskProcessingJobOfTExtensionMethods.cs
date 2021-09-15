using MFiles.VAF.AppTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.ExtensionMethods
{
	public static class ITaskProcessingJobOfTExtensionMethods
	{
		/// <summary>
		/// An action which can be used with <see cref="ITaskProcessingJob{TDirective}.Commit(Action{MFilesAPI.Vault})"/>
		/// to indicate that the job does not have anything to do in the commit action.
		/// </summary>
		private static Action<MFilesAPI.Vault> NoAction { get; } = (v) => { };

		/// <summary>
		/// Commits the job, but does not write anything to the vault.
		/// </summary>
		/// <typeparam name="TDirective">The type of directive this job accepts.</typeparam>
		/// <param name="job">The job to commit.</param>
		/// <remarks>
		/// The default transaction mode is <see cref="TransactionMode.Hybrid"/>, which forces the task processor to call
		/// <see cref="ITaskProcessingJob{TDirective}.Commit(Action{MFilesAPI.Vault})"/> prior to the processing completing.
		/// In some situations the task processing may not need to actually make any updates to the server.  This extension
		/// method is simply a shorthand for passing a lambda with no body to
		/// <see cref="ITaskProcessingJob{TDirective}.Commit(Action{MFilesAPI.Vault})"/>.
		/// </remarks>
		public static void CommitWithNoAction<TDirective>(this ITaskProcessingJob<TDirective> job)
			where TDirective : TaskDirective
		{
			// Sanity.
			if (null == job)
				throw new ArgumentNullException(nameof(job));

			// No action in the commit phase.
			job.Commit(ITaskProcessingJobOfTExtensionMethods.NoAction);
		}

		/// <summary>
		/// Updates the in-progress status of the task.
		/// </summary>
		/// <typeparam name="TDirective">The type of directive this job accepts.</typeparam>
		/// <param name="job">The job.</param>
		/// <param name="taskInformation">Information about the task.</param>
		public static void Update<TDirective>(this ITaskProcessingJob<TDirective> job, TaskInformation taskInformation)
			where TDirective : TaskDirective
		{
			// Sanity.
			if (null == job)
				throw new ArgumentNullException(nameof(job));
			if (null == taskInformation)
				return;

			// Use the standard method.
			job.Update
			(
				percentComplete: taskInformation.PercentageComplete,
				details: taskInformation.StatusDetails,
				data: taskInformation.ToJObject()
			);
		}
	}
}
