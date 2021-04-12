using MFiles.VAF.Extensions;
using MFiles.VAF.Extensions.ScheduledExecution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ScheduledExecution
{
	[TestClass]
	public class ScheduleTests
	{
		[TestMethod]
		public void ScheduleIsEnabledByDefault()
		{
			Assert.IsTrue(new Schedule().Enabled);
		}

		[TestMethod]
		public void ScheduleTriggersAreNotNullByDefault()
		{
			Assert.IsNotNull(new Schedule().Triggers);
		}

		[TestMethod]
		[DynamicData(nameof(GetNextExecutionData), DynamicDataSourceType.Method)]
		public void GetNextExecution
		(
			IEnumerable<TriggerBase> triggers,
			DateTime? after,
			DateTime? expected
		)
		{
			Assert.AreEqual
			(
				expected,
				new Schedule()
				{
					Enabled = true,
					Triggers = triggers
						.Select(t => new Trigger(t))
						.Where(t => t != null)
						.ToList()
				}.GetNextExecution(after)
			);
		}

		[TestMethod]
		[DynamicData(nameof(GetNextExecutionData), DynamicDataSourceType.Method)]
		public void GetNextExecution_NotEnabled
		(
			IEnumerable<TriggerBase> triggers,
			DateTime? after,
			DateTime? expected
		)
		{
			// We use the same data as the GetNextExecution, but
			// set the schedule to be disabled.
			// In this case the next execution date should be
			// overridden to be null in all cases.
			Assert.IsNull
			(
				new Schedule()
				{
					Enabled = false,
					Triggers = triggers
						.Select(t => new Trigger(t))
						.Where(t => t != null)
						.ToList()
				}.GetNextExecution(after)
			);
		}

		public static IEnumerable<object[]> GetNextExecutionData()
		{
			// Single trigger.
			yield return new object[]
			{
				new TriggerBase[]
				{
					new DailyTrigger(){
						TriggerTimes = new List<TimeSpan>()
						{
							new TimeSpan(17, 0, 0)
						}.Select(t => new TriggerTime() { Time = t }).ToList()
					}
				},
				new DateTime(2021, 03, 17, 01, 00, 00), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 17, 00, 00), // Wednesday @ 5pm
			};

			// Multiple triggers returns earliest.
			yield return new object[]
			{
				new TriggerBase[]
				{
					new DailyTrigger(){
						TriggerTimes = new List<TimeSpan>()
						{
							new TimeSpan(17, 0, 0)
						}.Select(t => new TriggerTime() { Time = t }).ToList()
					},
					new DailyTrigger(){
						TriggerTimes = new List<TimeSpan>()
						{
							new TimeSpan(12, 0, 0)
						}.Select(t => new TriggerTime() { Time = t }).ToList()
					}
				},
				new DateTime(2021, 03, 17, 01, 00, 00), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 12, 00, 00), // Wednesday @ 5pm
			};

			// No triggers = null.
			yield return new object[]
			{
				new TriggerBase[0],
				new DateTime(2021, 03, 17, 01, 00, 00), // Wednesday @ 1am
				(DateTime?)null
			};

			// Trigger at exact current time returns next day.
			yield return new object[]
			{
				new TriggerBase[]
				{
					new DailyTrigger(){
						TriggerTimes = new List<TimeSpan>()
						{
							new TimeSpan(17, 0, 0)
						}.Select(t => new TriggerTime() { Time = t }).ToList()
					}
				},
				new DateTime(2021, 03, 17, 17, 00, 00), // Wednesday @ 1am
				new DateTime(2021, 03, 18, 17, 00, 00)
			};
		}
	}
}
