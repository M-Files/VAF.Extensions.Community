using MFiles.VAF.AppTasks;
using MFiles.VaultApplications.Logging.Sensitivity.Filters;
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
			var flags = filter.GetSupportedCustomFlags()?.ToList() ?? new List<VAF.Configuration.JsonEditor.ValueOption>();
			Assert.AreEqual(1, flags.Count(v => v.Value as string == TaskInfoLogSensitivityFilter.HideTaskQueueAndTypeSensitivityFlag));
		}

		[TestMethod]
		public void GetSupportedCustomFlags_ReturnsShowRawDirectiveInformation()
		{
			var filter = new TaskInfoLogSensitivityFilter();
			var flags = filter.GetSupportedCustomFlags()?.ToList() ?? new List<VAF.Configuration.JsonEditor.ValueOption>();
			Assert.AreEqual(1, flags.Count(v => v.Value as string == TaskInfoLogSensitivityFilter.ShowRawDirectiveInformation));
		}

		[TestMethod]
		public void FilterValueForLogging_MaximumSensitivity()
		{
			var taskInfo = new TaskInfo<TaskDirective>()
			{
				TaskId = "1234"
			};
			var filter = new TaskInfoLogSensitivityFilter();
			Assert.AreEqual
			(
				"1234",
				filter.FilterValueForLogging(taskInfo, VaultApplications.Logging.Sensitivity.Sensitivity.MaximumSensitivity, null, null)
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
				filter.FilterValueForLogging(taskInfo, VaultApplications.Logging.Sensitivity.Sensitivity.MinimumSensitivity, null, null)
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
				$"{taskInfo.TaskId} (queue: {taskInfo.QueueId}, task type: {taskInfo.TaskType})",
				filter.FilterValueForLogging
				(
					taskInfo,
					VaultApplications.Logging.Sensitivity.Sensitivity.Custom,
					new string[0],
					null)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_Custom_HideTaskQueueAndTypeSensitivityFlag()
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
					VaultApplications.Logging.Sensitivity.Sensitivity.Custom,
					new[]
					{
						TaskInfoLogSensitivityFilter.HideTaskQueueAndTypeSensitivityFlag
					},
					null)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_Custom_ShowRawDirectiveInformation()
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
				$"{taskInfo.TaskId} (queue: {taskInfo.QueueId}, task type: {taskInfo.TaskType}, directive: {{\"UserId\":1}})",
				filter.FilterValueForLogging
				(
					taskInfo,
					VaultApplications.Logging.Sensitivity.Sensitivity.Custom,
					new[]
					{
						TaskInfoLogSensitivityFilter.ShowRawDirectiveInformation
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
					VaultApplications.Logging.Sensitivity.Sensitivity.Custom,
					new[]
					{
						TaskInfoLogSensitivityFilter.HideTaskQueueAndTypeSensitivityFlag,
						TaskInfoLogSensitivityFilter.ShowRawDirectiveInformation
					},
					null
				)
			);
		}

		[TestMethod]
		public void FilterValueForLogging_MinimumSensitivity_ShowRawDirectiveInformation()
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
				$"{taskInfo.TaskId} (queue: {taskInfo.QueueId}, task type: {taskInfo.TaskType}, directive: {{\"UserId\":1}})",
				filter.FilterValueForLogging
				(
					taskInfo,
					VaultApplications.Logging.Sensitivity.Sensitivity.MinimumSensitivity,
					new[]
					{
						TaskInfoLogSensitivityFilter.ShowRawDirectiveInformation
					},
					null
				)
			);
		}
	}
}
