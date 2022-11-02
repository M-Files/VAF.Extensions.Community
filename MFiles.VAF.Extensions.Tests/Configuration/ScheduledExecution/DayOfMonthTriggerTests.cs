using MFiles.VAF.Extensions;
using MFiles.VAF.Extensions.ScheduledExecution;
using MFiles.VAF.Extensions.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ScheduledExecution
{
	[TestClass]
	[DataMemberRequired(nameof(DayOfMonthTrigger.TriggerTimes), nameof(DayOfMonthTrigger.UnrepresentableDateHandling))]
	[JsonConfEditorRequired(nameof(DailyTrigger.TriggerTimes), ChildTypeEditor = "time")]
	public class DayOfMonthTriggerTests
		: ConfigurationClassTestBase<DayOfMonthTrigger>
	{
		[TestMethod]
		[DynamicData(nameof(GetNextDayOfMonthData), DynamicDataSourceType.Method)]
		public void GetNextDayOfMonth
		(
			DateTimeOffset after,
			int dayOfMonth,
			UnrepresentableDateHandling unrepresentableDateHandling,
			DateTimeOffset?[] expected
		)
		{
			var result = DayOfMonthTrigger
				.GetNextDayOfMonth(after, dayOfMonth, unrepresentableDateHandling)?
				.ToArray();
			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Length, result.Length);
			for(var i=0; i<result.Length; i++)
			{
				Assert.AreEqual(expected[i], result[i]);
			}
		}

		[TestMethod]
		[DynamicData(nameof(GetNextExecutionData), DynamicDataSourceType.Method)]
		public void GetNextExecution
		(
			IEnumerable<TimeSpan> triggerTimes,
			IEnumerable<int> triggerDays,
			DateTimeOffset? after,
			DateTimeOffset? expected
		)
		{
			var execution = new DayOfMonthTrigger()
			{
				TriggerTimes = triggerTimes.ToList(),
				TriggerDays = triggerDays.ToList()
			}.GetNextExecution(after, TimeZoneInfo.Utc);
			Assert.AreEqual(expected?.ToUniversalTime(), execution?.ToUniversalTime());
		}

		public static IEnumerable<object[]> GetNextExecutionData()
		{
			// Execution later same day.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new []{ 17 }, // 5pm on the 17th
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 17, 17, 00, 00, TimeSpan.Zero), // 17th @ 5pm
			};

			// Execution later same day (one passed).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 17 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 17, 17, 00, 00, TimeSpan.Zero), // 17th @ 5pm
			};

			// Execution later same day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 17 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 17, 14, 00, 00, TimeSpan.Zero), // 17th @ 2pm
			};

			// Execution next day.
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0) },
				new []{ 18 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 18, 14, 00, 00, TimeSpan.Zero), // 18th @ 2pm
			};

			// Execution next day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 18 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 18, 00, 00, 00, TimeSpan.Zero), // 18th @ midnight
			};

			// Execution next month.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new []{ 16 },
				new DateTimeOffset(2021, 04, 17, 01, 00, 00, TimeSpan.Zero), // 17th April @ 1am
				new DateTimeOffset(2021, 05, 16, 17, 00, 00, TimeSpan.Zero), // 16th May @ 5pm
			};

			// Execution next week (multiple days matching, first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0) },
				new []{ 14, 16 },
				new DateTimeOffset(2021, 04, 17, 01, 00, 00, TimeSpan.Zero), // 17th April @ 1am
				new DateTimeOffset(2021, 05, 14, 00, 00, 00, TimeSpan.Zero), // 14th May @ 5pm
			};

			// Execution next week (one day this week passed, returns next week's execution).
			yield return new object[]
			{
				new []{ new TimeSpan(2, 0, 0) },
				new []{ 16, 20 },
				new DateTimeOffset(2021, 04, 17, 03, 00, 00, TimeSpan.Zero), // 17th @ 3am
				new DateTimeOffset(2021, 04, 20, 02, 00, 00, TimeSpan.Zero), // 20th @ 5pm
			};

			// No valid executions = null.
			yield return new object[]
			{
				new TimeSpan[0],
				new [] { 20 },
				new DateTimeOffset(2021, 03, 17, 18, 00, 00, TimeSpan.Zero), // Wednesday @ 6pm
				(DateTimeOffset?)null
			};

			// Exact current time returns next month.
			yield return new object[]
			{
				new []{ new TimeSpan(2, 0, 0) },
				new []{ 17 },
				new DateTimeOffset(2021, 04, 17, 02, 00, 00, TimeSpan.Zero),
				new DateTimeOffset(2021, 05, 17, 02, 00, 00, TimeSpan.Zero),
			};
		}

		public static IEnumerable<object[]> GetNextDayOfMonthData()
		{
			// Today is returned as today and next week.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero), // Wednesday
				17, // Get the 17th
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]
				{
					new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero), // It should return the same day.
					new DateTimeOffset(2021, 04, 17, 0, 0, 0, 0, TimeSpan.Zero), // It should return next month too.
				}
			};

			// 17th and want next 18th.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				18,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 03, 18, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// 17th and want 16th.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				16,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 04, 16, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// Invalid day of the month.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[0]
			};
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				45,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[0]
			};

			// 30th of February -> 30th March (no valid day in Feb).
			yield return new object[]
			{
				new DateTimeOffset(2021, 02, 17, 0, 0, 0, 0, TimeSpan.Zero),
				30,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 03, 30, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// 30th of February -> 28th Feb (no valid day in Feb).
			yield return new object[]
			{
				new DateTimeOffset(2021, 02, 17, 0, 0, 0, 0, TimeSpan.Zero),
				30,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 02, 28, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// 30th of February -> 29th Feb (leap year, and no valid day in Feb).
			yield return new object[]
			{
				new DateTimeOffset(2024, 02, 17, 0, 0, 0, 0, TimeSpan.Zero),
				30,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[] { new DateTimeOffset(2024, 02, 29, 0, 0, 0, 0, TimeSpan.Zero) }
			};
		}
	}
}
