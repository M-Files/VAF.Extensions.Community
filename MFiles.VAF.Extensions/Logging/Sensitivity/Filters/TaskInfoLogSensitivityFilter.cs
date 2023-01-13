using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.JsonEditor;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.NLog.Configuration;
using MFiles.VAF.Configuration.Logging.SensitivityFilters;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Logging.Sensitivity.Filters
{
	/// <summary>
	/// Implements a log sensitivity filter for <see cref="TaskInfo"/>.
	/// </summary>
	public class TaskInfoLogSensitivityFilter
		: LogSensitivityFilterBase<TaskInfo>
	{
		/// <summary>
		/// Resolve the delegate filter we use.
		/// </summary>
		/// <returns>A sensitivity filter for <see cref="IObjVer"/>s.</returns>
		protected ILogSensitivityFilter<IObjID> GetObjIDFilter()
		{
			return ResolveDelegateFilter(() => new ObjIdLogSensitivityFilter());
		}

		/// <summary>
		/// Resolve the delegate filter we use.
		/// </summary>
		/// <returns>A sensitivity filter for <see cref="IObjVer"/>s.</returns>
		protected ILogSensitivityFilter<IObjVer> GetObjVerFilter()
		{
			return ResolveDelegateFilter(() => new ObjVerLogSensitivityFilter());
		}

		/// <inheritdoc />
		public override IEnumerable<SensitivityFlag> GetSupportedCustomFlags()
		{
			foreach (var f in base.GetSupportedCustomFlags())
				yield return f;

			yield return ExtensionsNLogLogManager.TaskQueueAndTaskType;
			yield return ExtensionsNLogLogManager.RawTaskDirective;
		}

		/// <inheritdoc />
		public override string FilterValueForLogging(TaskInfo value, LogSensitivity level, IEnumerable<SensitivityFlag> customFlags, string format, IFormatProvider formatProvider)
		{
			if (null == value?.DirectiveType)
				return String.Empty;

			string basicValue = null;
			try
			{
				// Is it an objverex?
				{
					if (null == basicValue 
						&& typeof(ObjVerExTaskDirective).IsAssignableFrom(value.DirectiveType))
					{
						var d = value.Directive as ObjVerExTaskDirective;
						var objVer = MFUtils.ParseObjVerString(d?.ObjVerEx);
						basicValue = this.GetObjVerFilter().FilterValueForLogging(objVer, level, customFlags, format, formatProvider);
					}
				}

				// Is it an objid?
				{
					if (null == basicValue
						&& typeof(ObjIDTaskDirective).IsAssignableFrom(value.DirectiveType))
					{
						var d = value.Directive as ObjIDTaskDirective;
						ObjID objID = null;
						if (d?.TryGetObjID(out objID) ?? false)
						{
							basicValue = this.GetObjIDFilter().FilterValueForLogging(objID, level, customFlags, format, formatProvider);
						}
						else
						{
							return $"(invalid object data: {d?.ObjectTypeID}-{d?.ObjectID})";
						}
					}
				}

				// Does it have a name?
				{
					if (null == basicValue
						&& typeof(TaskDirectiveWithDisplayName).IsAssignableFrom(value.DirectiveType))
					{
						var d = value.Directive as TaskDirectiveWithDisplayName;
						if (!string.IsNullOrWhiteSpace(d?.DisplayName))
							basicValue = d.DisplayName;
					}
				}
			}
			catch(Exception e)
			{
				this.Logger?.Error(e, $"Could not filter task info for logging");
				return String.Empty;
			}

			// If we can include the task queue then do so.
			string taskQueueAndType = null;
			if(CanLog(level, customFlags, ExtensionsNLogLogManager.TaskQueueAndTaskType))
			{
				taskQueueAndType = $" (queue: {value.QueueId}, task type: {value.TaskType})";
			}

			// If we can include the raw task directive then do so.
			string directiveValue = null;
			if(null != value.Directive
				&& CanLog(level,customFlags, ExtensionsNLogLogManager.RawTaskDirective))
			{
				directiveValue = $" (directive: {value.Directive?.ToJson()})";
			}

			// Return the log value.
			return $"{value.TaskId}{basicValue}{taskQueueAndType}{directiveValue}";
		}
	}
}