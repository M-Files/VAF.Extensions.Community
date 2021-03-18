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
	public class DailyTriggerTests
	{
		[TestMethod]
		[DynamicData(nameof(GetNextExecutionData), DynamicDataSourceType.Method)]
		public void GetNextExecution
		(
			IEnumerable<TimeSpan> triggerTimes,
			DateTime? after,
			DateTime? expected
		)
		{
			Assert.AreEqual
			(
				expected,
				new DailyTrigger()
				{
					TriggerTimes = triggerTimes.ToList()
				}.GetNextExecution(after)
			);
		}

		public static IEnumerable<object[]> GetNextExecutionData()
		{
			// Execution later same day.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) }, // Scheduled for Wednesday at 5pm.
				new DateTime(2021, 03, 17, 01, 00, 00), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 17, 00, 00), // Wednesday @ 5pm
			};

			// Execution later same day (one passed).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new DateTime(2021, 03, 17, 01, 00, 00), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 17, 00, 00), // Wednesday @ 5pm
			};

			// Execution later same day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0) },
				new DateTime(2021, 03, 17, 01, 00, 00), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 14, 00, 00), // Wednesday @ 2pm
			};

			// Execution next day.
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0) },
				new DateTime(2021, 03, 17, 15, 00, 00), // Wednesday @ 3pm
				new DateTime(2021, 03, 18, 14, 00, 00), // Thursday @ 2pm
			};

			// Execution next day (multiple items, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new DateTime(2021, 03, 17, 18, 00, 00), // Wednesday @ 6pm
				new DateTime(2021, 03, 18, 00, 00, 00), // Thursday @ midnight
			};

			// No valid executions = null.
			yield return new object[]
			{
				new TimeSpan[0],
				new DateTime(2021, 03, 17, 18, 00, 00), // Wednesday @ 6pm
				(DateTime?)null
			};
		}
	}
}
