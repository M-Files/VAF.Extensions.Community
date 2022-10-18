using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.JsonEditor;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// By placing this in the MFiles.VaultApplications.Logging namespace, the "RenderInternalLogs" will apply to this too.
namespace MFiles.VaultApplications.Logging.Sensitivity.Filters
{
	/// <summary>
	/// Implements a log sensitivity filter for <see cref="TaskInfo"/>.
	/// </summary>
	public class TaskInfoLogSensitivityFilter
		: LogSensitivityFilterBase<TaskInfo>
	{
		/// <summary>
		/// The sensivitity flag for whether to hide the task queue and type.
		/// </summary>
		public const string HideTaskQueueAndTypeSensitivityFlag = "HideTaskQueueAndType";

		/// <summary>
		/// The sensivitity flag for whether to show the task directive information.
		/// </summary>
		public const string ShowRawDirectiveInformation = "ShowRawDirectiveInformation";

		/// <inheritdoc />
		public override IEnumerable<ValueOption> GetSupportedCustomFlags()
		{
			foreach (var f in base.GetSupportedCustomFlags())
				yield return f;

			yield return new ValueOption()
			{
				Label = VAF.Extensions.Resources.Logging.MFiles_VaultApplication_Logging_Sensitivity_CustomFlags_HideTaskQueueAndType_Label,
				HelpText = VAF.Extensions.Resources.Logging.MFiles_VaultApplication_Logging_Sensitivity_CustomFlags_HideTaskQueueAndType_HelpText,
				Value = HideTaskQueueAndTypeSensitivityFlag
			};

			yield return new ValueOption()
			{
				Label = VAF.Extensions.Resources.Logging.MFiles_VaultApplication_Logging_Sensitivity_CustomFlags_ShowRawDirectiveInformation_Label,
				HelpText = VAF.Extensions.Resources.Logging.MFiles_VaultApplication_Logging_Sensitivity_CustomFlags_ShowRawDirectiveInformation_HelpText,
				Value = ShowRawDirectiveInformation
			};
		}

		/// <inheritdoc />
		public override string FilterValueForLogging(TaskInfo value, Sensitivity level, IEnumerable<string> customFlags, string format, IFormatProvider formatProvider)
		{
			if (null == value)
				return String.Empty;

			customFlags = customFlags ?? Enumerable.Empty<string>();
			switch (level)
			{
				case Sensitivity.Custom:
					{
						if (customFlags.Contains(HideTaskQueueAndTypeSensitivityFlag))
						{
							if (customFlags.Contains(ShowRawDirectiveInformation))
							{
								return $"{value.TaskId} (directive: {value.Directive?.ToJson()})"; // No task queue and type, but directive.
							}
							else
							{
								return value.TaskId; // Just the ID.
							}
						}
						else
						{
							if (customFlags.Contains(ShowRawDirectiveInformation))
							{
								return $"{value.TaskId} (queue: {value.QueueId}, task type: {value.TaskType}, directive: {value.Directive?.ToJson()})"; // Everything.
							}
							else
							{
								return $"{value.TaskId} (queue: {value.QueueId}, task type: {value.TaskType})"; // ID, queue, type, no directive.
							}
						}
					}
				case Sensitivity.MinimumSensitivity:
					if (customFlags.Contains(ShowRawDirectiveInformation))
					{
						return $"{value.TaskId} (queue: {value.QueueId}, task type: {value.TaskType}, directive: {value.Directive?.ToJson()})"; // Everything.
					}
					else
					{
						return $"{value.TaskId} (queue: {value.QueueId}, task type: {value.TaskType})"; // ID, queue, type, no directive.
					}


				default: // For maximum, or anything else, just return the ID.
					return value.TaskId; // Just the ID.
			}
		}
	}
}
