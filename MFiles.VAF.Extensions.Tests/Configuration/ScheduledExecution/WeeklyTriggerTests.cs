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
	[DataMemberRequired(nameof(WeeklyTrigger.TriggerTimes), nameof(WeeklyTrigger.TriggerDays))]
	[JsonConfEditorRequired(nameof(DailyTrigger.TriggerTimes), ChildTypeEditor = "time")]
	public class WeeklyTriggerTests
		: ConfigurationClassTestBase<WeeklyTrigger>
	{
		[TestMethod]
		[DynamicData(nameof(GetNextDayOfWeekData), DynamicDataSourceType.Method)]
		public void GetNextDayOfWeek
		(
			DateTime after,
			DayOfWeek dayOfWeek,
			DateTime?[] expected
		)
		{
			DateTimeOffset[] result = WeeklyTrigger.GetNextDayOfWeek(after, dayOfWeek)?.ToArray();
			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Length, result.Length);

			for (var i = 0; i < result.Length; i++)
			{
				Assert.AreEqual(expected[i]?.ToUniversalTime(), result[i].UtcDateTime);
			}
		}

		[TestMethod]
		[DynamicData(nameof(GetNextExecutionData), DynamicDataSourceType.Method)]
		public void GetNextExecution
		(
			IEnumerable<TimeSpan> triggerTimes,
			IEnumerable<DayOfWeek> triggerDays,
			DateTime? after,
			DateTime? expected
		)
		{
			var trigger = new WeeklyTrigger()
			{
				TriggerTimes = triggerTimes.ToList().ToList(),
				TriggerDays = triggerDays.ToList()
			};
			var execution = trigger.GetNextExecution(after);
			Assert.AreEqual(expected?.ToUniversalTime(), execution?.ToUniversalTime());
		}

		public static IEnumerable<object[]> GetNextExecutionData()
		{
			// Execution later same day.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) }, // Scheduled for Wednesday at 5pm.
				new []{ DayOfWeek.Wednesday },
				new DateTime(2021, 03, 17, 01, 00, 00, 0), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 17, 00, 00, 0), // Wednesday @ 5pm
			};

			// Execution later same day (one passed).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ DayOfWeek.Wednesday },
				new DateTime(2021, 03, 17, 01, 00, 00, 0), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 17, 00, 00, 0), // Wednesday @ 5pm
			};

			// Execution later same day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ DayOfWeek.Wednesday },
				new DateTime(2021, 03, 17, 01, 00, 00, 0), // Wednesday @ 1am
				new DateTime(2021, 03, 17, 14, 00, 00, 0), // Wednesday @ 2pm
			};

			// Execution next day.
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0) },
				new []{ DayOfWeek.Thursday },
				new DateTime(2021, 03, 17, 01, 00, 00, 0), // Wednesday @ 1am
				new DateTime(2021, 03, 18, 14, 00, 00, 0), // Thursday @ 2pm
			};

			// Execution next day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ DayOfWeek.Thursday },
				new DateTime(2021, 03, 17, 01, 00, 00, 0), // Wednesday @ 1am
				new DateTime(2021, 03, 18, 00, 00, 00, 0), // Thursday @ midnight
			};

			// Execution next week.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new []{ DayOfWeek.Monday },
				new DateTime(2021, 03, 17, 01, 00, 00, 0), // Wednesday @ 1am
				new DateTime(2021, 03, 22, 17, 00, 00, 0), // Monday @ 5pm
			};

			// Execution next week (multiple days matching, first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0) },
				new []{ DayOfWeek.Monday, DayOfWeek.Wednesday },
				new DateTime(2021, 03, 17, 01, 00, 00, 0), // Wednesday @ 1am
				new DateTime(2021, 03, 22, 00, 00, 00, 0), // Monday @ 5pm
			};

			// Execution next week (one day this week passed, returns next week's execution).
			yield return new object[]
			{
				new []{ new TimeSpan(2, 0, 0) },
				new []{ DayOfWeek.Monday, DayOfWeek.Wednesday },
				new DateTime(2021, 03, 17, 03, 00, 00, 0), // Wednesday @ 3am
				new DateTime(2021, 03, 22, 02, 00, 00, 0), // Monday @ 5pm
			};

			// No valid executions = null.
			yield return new object[]
			{
				new TimeSpan[0],
				new [] { DayOfWeek.Monday },
				new DateTime(2021, 03, 17, 18, 00, 00, 0), // Wednesday @ 6pm
				(DateTime?)null
			};

			// Exact current time returns next week.
			yield return new object[]
			{
				new []{ new TimeSpan(2, 0, 0) },
				new []{ DayOfWeek.Wednesday },
				new DateTime(2021, 03, 17, 02, 00, 00, 0),
				new DateTime(2021, 03, 24, 02, 00, 00, 0),
			};
		}

		public static IEnumerable<object[]> GetNextDayOfWeekData()
		{
			// Today is returned as today and next week.
			yield return new object[]
			{
				new DateTime(2021, 03, 17, 0, 0, 0, 0), // Wednesday
				DayOfWeek.Wednesday, // Get the next Wednesday
				new DateTime?[]
				{
					new DateTime(2021, 03, 17, 0, 0, 0, 0), // It should return the same day.
					new DateTime(2021, 03, 24, 0, 0, 0, 0), // It should return next week too.
				}
			};

			// Wednesday and want next Tuesday.
			yield return new object[]
			{
				new DateTime(2021, 03, 17, 0, 0, 0, 0), // Wednesday
				DayOfWeek.Tuesday, // Get the next Tuesday
				new DateTime ?[] { new DateTime(2021, 03, 23, 0, 0, 0, 0) }
			};

			// Wednesday and want this Thursday.
			yield return new object[]
			{
				new DateTime(2021, 03, 17, 0, 0, 0, 0), // Wednesday
				DayOfWeek.Thursday, // Get the next Thursday
				new DateTime ?[] { new DateTime(2021, 03, 18, 0, 0, 0, 0) }
			};

			// Thursday and want this Sunday.
			yield return new object[]
			{
				new DateTime(2021, 03, 18, 0, 0, 0, 0), // Thursday
				DayOfWeek.Sunday, // Get the next Sunday
				new DateTime ?[] { new DateTime(2021, 03, 21, 0, 0, 0, 0) }
			};

			// Monday and want this Saturday.
			yield return new object[]
			{
				new DateTime(2021, 03, 15, 0, 0, 0, 0), // Monday
				DayOfWeek.Saturday, // Get the next Saturday
				new DateTime ?[] { new DateTime(2021, 03, 20, 0, 0, 0, 0) }
			};
		}

		[TestMethod]
		public void WeeklyValueNotReturnedWhenDaylightsavingChanges()
		{
			var now = new DateTimeOffset(new DateTime(2024, 10, 23, 10, 27, 0), new TimeSpan(0, 0, 0));
			var expected = new DateTimeOffset(new DateTime(2024, 10, 30, 9, 0, 0), new TimeSpan(2, 0, 0));

			var trigger = new WeeklyTrigger()
			{
				TriggerTimes = new List<TimeSpan>()
				{
					new TimeSpan(9, 0, 0)
				},
				TriggerDays = new List<DayOfWeek>() { DayOfWeek.Wednesday }
			};
			var execution = trigger.GetNextExecution(now, TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));
			Assert.AreEqual(expected.ToUniversalTime(), execution?.ToUniversalTime());
		}

		[TestMethod]
		public void WeeklyValueNotReturnedWhenDaylightsavingChanges_2()
		{
			var now = new DateTimeOffset(new DateTime(2025, 3, 26, 9, 0, 0), new TimeSpan(2, 0, 0));
			var expected = new DateTimeOffset(new DateTime(2025, 4, 2, 9, 0, 0), new TimeSpan(3, 0, 0));

			var trigger = new WeeklyTrigger()
			{
				TriggerTimes = new List<TimeSpan>()
				{
					new TimeSpan(9, 0, 0)
				},
				TriggerDays = new List<DayOfWeek>() { DayOfWeek.Wednesday }
			};
			var execution = trigger.GetNextExecution(now, TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));
			Assert.AreEqual(expected.ToUniversalTime(), execution?.ToUniversalTime());
		}
	}
}
