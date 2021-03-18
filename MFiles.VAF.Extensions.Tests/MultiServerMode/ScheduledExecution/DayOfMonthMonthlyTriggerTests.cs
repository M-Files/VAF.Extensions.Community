using MFiles.VAF.Extensions.MultiServerMode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.MultiServerMode
{
	[TestClass]
	public class DayOfMonthMonthlyTriggerTests
	{
		[TestMethod]
		[DynamicData(nameof(GetNextDayOfMonthData), DynamicDataSourceType.Method)]
		public void GetNextDayOfMonth
		(
			DateTime after,
			int dayOfMonth,
			DateTime[] expected
		)
		{
			var result = DayOfMonthMonthlyTrigger.GetNextDayOfMonth(after, dayOfMonth)?.ToArray();
			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Length, result.Length);
			for(var i=0; i<result.Length; i++)
			{
				Assert.AreEqual(expected[i], result[i]);
			}
		}

		[TestMethod]
		[DynamicData(nameof(GetNextExecutionTimeData), DynamicDataSourceType.Method)]
		public void GetNextExecutionTime
		(
			IEnumerable<TimeSpan> triggerTimes,
			IEnumerable<int> triggerDays,
			DateTime after,
			DateTime expected
		)
		{
			Assert.AreEqual
			(
				expected,
				new DayOfMonthMonthlyTrigger()
				{
					TriggerTimes = triggerTimes.ToList(),
					TriggerDays = triggerDays.ToList()
				}.GetNextExecutionTime(after)
			);
		}

		public static IEnumerable<object[]> GetNextExecutionTimeData()
		{
			// Execution later same day.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new []{ 17 }, // 5pm on the 17th
				new DateTime(2021, 03, 17, 01, 00, 00), // 17th @ 1am
				new DateTime(2021, 03, 17, 17, 00, 00), // 17th @ 5pm
			};

			// Execution later same day (one passed).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 17 },
				new DateTime(2021, 03, 17, 01, 00, 00), // 17th @ 1am
				new DateTime(2021, 03, 17, 17, 00, 00), // 17th @ 5pm
			};

			// Execution later same day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 17 },
				new DateTime(2021, 03, 17, 01, 00, 00), // 17th @ 1am
				new DateTime(2021, 03, 17, 14, 00, 00), // 17th @ 2pm
			};

			// Execution next day.
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0) },
				new []{ 18 },
				new DateTime(2021, 03, 17, 01, 00, 00), // 17th @ 1am
				new DateTime(2021, 03, 18, 14, 00, 00), // 18th @ 2pm
			};

			// Execution next day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 18 },
				new DateTime(2021, 03, 17, 01, 00, 00), // 17th @ 1am
				new DateTime(2021, 03, 18, 00, 00, 00), // 18th @ midnight
			};

			// Execution next month.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new []{ 16 },
				new DateTime(2021, 03, 17, 01, 00, 00), // 17th March @ 1am
				new DateTime(2021, 04, 16, 17, 00, 00), // 16th April @ 5pm
			};

			// Execution next week (multiple days matching, first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0) },
				new []{ 14, 16 },
				new DateTime(2021, 03, 17, 01, 00, 00), // 17th March @ 1am
				new DateTime(2021, 04, 14, 00, 00, 00), // 14th April @ 5pm
			};

			// Execution next week (one day this week passed, returns next week's execution).
			yield return new object[]
			{
				new []{ new TimeSpan(2, 0, 0) },
				new []{ 16, 20 },
				new DateTime(2021, 03, 17, 03, 00, 00), // 17th @ 3am
				new DateTime(2021, 03, 20, 02, 00, 00), // 20th @ 5pm
			};
		}

		public static IEnumerable<object[]> GetNextDayOfMonthData()
		{
			// Today is returned as today and next week.
			yield return new object[]
			{
				new DateTime(2021, 03, 17), // Wednesday
				17, // Get the 17th
				new []
				{
					new DateTime(2021, 03, 17), // It should return the same day.
					new DateTime(2021, 04, 17), // It should return next month too.
				}
			};

			// 17th and want next 18th.
			yield return new object[]
			{
				new DateTime(2021, 03, 17),
				18,
				new [] { new DateTime(2021, 03, 18) }
			};

			// 17th and want 16th.
			yield return new object[]
			{
				new DateTime(2021, 03, 17),
				16,
				new [] { new DateTime(2021, 04, 16) }
			};
		}
	}
}
