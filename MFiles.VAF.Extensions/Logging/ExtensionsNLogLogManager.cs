using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.NLog;
using MFiles.VAF.Extensions.Logging.Sensitivity.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Logging
{
	public class ExtensionsNLogLogManager
		: NLogLogManager
	{
		/// <summary>
		/// The log category for configuration upgrading.
		/// </summary>
		public static readonly LogCategory ConfigurationUpgrading
			= LogCategory.Define
			(
				"CONFIGURATION_UPGRADING",
				null,
				new List<string>()
				{
					"MFiles.VAF.Extensions.Configuration.Upgrading.*"
				},
				ResourceMarker.Id + nameof(Resources.Logging.LogCategory_ConfigurationUpgrading_Label),
				ResourceMarker.Id + nameof(Resources.Logging.LogCategory_ConfigurationUpgrading_HelpText)
			);

		/// <summary>
		/// Whether to log the task queue and task type or not.
		/// </summary>
		public static readonly SensitivityFlag TaskQueueAndTaskType
			= SensitivityFlag.Define
			(
				"TASK_QUEUE_AND_TYPE",
				ResourceMarker.Id + nameof(Resources.Logging.SensitivityFlag_TaskQueueAndTaskType_Label),
				ResourceMarker.Id + nameof(Resources.Logging.SensitivityFlag_TaskQueueAndTaskType_HelpText)
			);

		/// <summary>
		/// Whether to log raw task directive data or not.
		/// </summary>
		public static readonly SensitivityFlag RawTaskDirective
			= SensitivityFlag.Define
			(
				"RAW_TASK_DIRECTIVE",
				ResourceMarker.Id + nameof(Resources.Logging.SensitivityFlag_RawDirective_Label),
				ResourceMarker.Id + nameof(Resources.Logging.SensitivityFlag_RawDirective_HelpText)
			);

		static ExtensionsNLogLogManager()
		{
			// Add task manager stuff to the correct category.
			LogCategory.TaskManager.Loggers.Add(typeof(TaskManagerEx).FullName);
			LogCategory.TaskManager.Loggers.Add(typeof(TaskQueueBackgroundOperationManager).FullName);
			LogCategory.TaskManager.Loggers.Add(typeof(RecurringOperationConfigurationManager).FullName);
		}

		/// <summary>
		/// Returns the default sensitivity filters to be used by this log manager.
		/// </summary>
		/// <returns>The default list of sensitivity filters.</returns>
		public override List<ILogSensitivityFilter> GetDefaultSensitivityFilters()
		{
			// Extend defaults with those from VAF.
			List<ILogSensitivityFilter> filters = base.GetDefaultSensitivityFilters();
			filters.Add(new EventHandlerEnvironmentLogSensitivityFilter());
			filters.Add(new TaskManagerEventArgsSensitivityFilter());
			filters.Add(new ObjVerExLogSensitivityFilter());
			filters.Add(new INamedValueItemSensitivityFilter());
			filters.Add(new TaskInfoLogSensitivityFilter());
			return filters;
		}

		/// <summary>
		/// Returns the default properties that should be exposed as layout renderers by this log manager.
		/// </summary>
		/// <returns>The default properties.</returns>
		public override List<string> GetDefaultMFilesScopePropertiesForLayout()
		{
			// Extend defaults with those from VAF.
			List<string> props = base.GetDefaultMFilesScopePropertiesForLayout();
			props.Add(AppTaskLogContextKeys.TaskQueue);
			props.Add(AppTaskLogContextKeys.TaskType);
			props.Add(AppTaskLogContextKeys.TaskId);
			props.Add(AppTaskLogContextKeys.BroadcastIds);
			return props;
		}
	}
}
