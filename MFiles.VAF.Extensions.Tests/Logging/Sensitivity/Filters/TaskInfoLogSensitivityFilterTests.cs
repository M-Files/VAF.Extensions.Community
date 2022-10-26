using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Extensions.Logging;
using MFiles.VAF.Extensions.Logging.Sensitivity.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Logging.Sensitivity.Filters
{
	[TestClass]
	public class TaskInfoLogSensitivityFilterTests
	{
		[TestMethod]
		public void GetSupportedCustomFlags_ReturnsHideTaskQueueAndTypeSensitivityFlag()
		{
			var filter = new TaskInfoLogSensitivityFilter();
			var flags = filter.GetSupportedCustomFlags()?.ToList() ?? new List<SensitivityFlag>();
			Assert.AreEqual(1, flags.Count(v => v.Id == ExtensionsNLogLogManager.TaskQueueAndTaskType.Id));
		}

		[TestMethod]
		public void GetSupportedCustomFlags_ReturnsShowRawDirectiveInformation()
		{
			var filter = new TaskInfoLogSensitivityFilter();
			var flags = filter.GetSupportedCustomFlags()?.ToList() ?? new List<SensitivityFlag>();
			Assert.AreEqual(1, flags.Count(v => v.Id == ExtensionsNLogLogManager.RawTaskDirective.Id));
		}

		[TestMethod]
		public void FilterValueForLogging_Maximum()
		{
			var taskInfo = new TaskInfo<TaskDirective>()
			{
				TaskId = "1234"
			};
			var filter = new TaskInfoLogSensitivityFilter();
			Assert.AreEqual
			(
				"1234",
				filter.FilterValueForLogging(taskInfo, LogSensitivity.Maximum, null, null)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_MinimumSensitivity()
		{
			var taskInfo = new TaskInfo<TaskDirective>()
			{
				TaskId = "1234",
				QueueId = "MyQueueId",
				TaskType = "MyTaskType"
			};
			var filter = new TaskInfoLogSensitivityFilter();
			Assert.AreEqual
			(
				$"{taskInfo.TaskId} (queue: {taskInfo.QueueId}, task type: {taskInfo.TaskType})",
				filter.FilterValueForLogging(taskInfo, LogSensitivity.Minimum, null, null)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_Custom_NoFlags()
		{
			var taskInfo = new TaskInfo<TaskDirective>()
			{
				TaskId = "1234",
				QueueId = "MyQueueId",
				TaskType = "MyTaskType"
			};
			var filter = new TaskInfoLogSensitivityFilter();
			Assert.AreEqual
			(
				$"{taskInfo.TaskId}",
				filter.FilterValueForLogging
				(
					taskInfo,
					LogSensitivity.Custom,
					new SensitivityFlag[]
					{
						
					},
					null)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_Custom_TaskQueueAndDirective()
		{
			var taskInfo = new TaskInfo<TaskDirective>()
			{
				TaskId = "1234",
				QueueId = "MyQueueId",
				TaskType = "MyTaskType",
				Directive = new TaskDirective() { UserId = 1 }
			};
			var filter = new TaskInfoLogSensitivityFilter();
			Assert.AreEqual
			(
				$"{taskInfo.TaskId} (queue: {taskInfo.QueueId}, task type: {taskInfo.TaskType}) (directive: {{\"UserId\":1}})",
				filter.FilterValueForLogging
				(
					taskInfo,
					LogSensitivity.Custom,
					new[]
					{
						ExtensionsNLogLogManager.TaskQueueAndTaskType,
						ExtensionsNLogLogManager.RawTaskDirective
					},
					null)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_Custom_HideTaskQueueAndTypeSensitivityFlag_ShowRawDirectiveInformation()
		{
			var taskInfo = new TaskInfo<TaskDirective>()
			{
				TaskId = "1234",
				QueueId = "MyQueueId",
				TaskType = "MyTaskType",
				Directive = new TaskDirective() { UserId = 1 }
			};
			var filter = new TaskInfoLogSensitivityFilter();
			Assert.AreEqual
			(
				$"{taskInfo.TaskId} (directive: {{\"UserId\":1}})",
				filter.FilterValueForLogging
				(
					taskInfo,
					LogSensitivity.Custom,
					new[]
					{
						ExtensionsNLogLogManager.RawTaskDirective
					},
					null
				)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_Minimum()
		{
			var taskInfo = new TaskInfo<TaskDirective>()
			{
				TaskId = "1234",
				QueueId = "MyQueueId",
				TaskType = "MyTaskType",
				Directive = new TaskDirective() { UserId = 1 }
			};
			var filter = new TaskInfoLogSensitivityFilter();
			Assert.AreEqual
			(
				$"{taskInfo.TaskId} (queue: {taskInfo.QueueId}, task type: {taskInfo.TaskType}) (directive: {{\"UserId\":1}})",
				filter.FilterValueForLogging
				(
					taskInfo,
					LogSensitivity.Minimum,
					new SensitivityFlag[]
					{
					},
					null
				)
			);
		}
	}
}
