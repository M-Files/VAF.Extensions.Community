using MFiles.VAF.Extensions.MultiServerMode;
using MFiles.VAF.Extensions.MultiServerMode.ScheduledExecution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.MultiServerMode.ScheduledExecution
{
	[TestClass]
	public class ScheduleTests
	{
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
					Triggers = new List<TriggerBase>(triggers)
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
						}
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
						}
					},
					new DailyTrigger(){
						TriggerTimes = new List<TimeSpan>()
						{
							new TimeSpan(12, 0, 0)
						}
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
		}
	}
}
