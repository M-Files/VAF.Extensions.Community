using MFiles.VAF.Extensions.ScheduledExecution;
using MFiles.VAF.Extensions.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Tests.ScheduledExecution
{
	[TestClass]
	[DataMemberRequired(nameof(DailyTrigger.TriggerTimes))]
	[JsonConfEditorRequired(nameof(DailyTrigger.TriggerTimes), ChildTypeEditor = "time")]
	public class DailyTriggerTests
		: ConfigurationClassTestBase<DailyTrigger>
	{

		[TestMethod]
		[DynamicData(nameof(GetNextExecutionData), DynamicDataSourceType.Method)]
		public void GetNextExecution
		(
			IEnumerable<TimeSpan> triggerTimes,
			DateTimeOffset? after,
			DateTimeOffset? expected,
			TimeZoneInfo timezoneInfo
		)
		{
			var execution = new DailyTrigger()
			{
				TriggerTimes = triggerTimes.ToList(),
			}.GetNextExecution(after, timezoneInfo);
			Assert.AreEqual(expected?.ToUniversalTime(), execution?.ToUniversalTime());
		}
		[TestMethod]
		public void NullAfterDoesNotThrow
		(
		)
		{
			var execution = new DailyTrigger()
			{
				TriggerTimes = new List<TimeSpan>
				{
					new TimeSpan(10, 0, 0)
				}
			}.GetNextExecution(null);
		}

		public static IEnumerable<object[]> GetNextExecutionData()
		{
			// Execution later same day.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) }, // Scheduled for Wednesday at 5pm.
				new DateTimeOffset(new DateTime(2021, 03, 17, 01, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 1am
				new DateTimeOffset(new DateTime(2021, 03, 17, 17, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 5pm
				null,
			};

			// Execution later same day (one passed).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new DateTimeOffset(new DateTime(2021, 03, 17, 01, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 1am
				new DateTimeOffset(new DateTime(2021, 03, 17, 17, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 5pm
				null,
			};

			// Execution later same day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0) },
				new DateTimeOffset(new DateTime(2021, 03, 17, 01, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 1am
				new DateTimeOffset(new DateTime(2021, 03, 17, 14, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 2pm
				null,
			};

			// Execution next day.
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0) },
				new DateTimeOffset(new DateTime(2021, 03, 17, 15, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 3pm
				new DateTimeOffset(new DateTime(2021, 03, 18, 14, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Thursday @ 2pm
				null,
			};

			// Execution next day (multiple items, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new DateTimeOffset(new DateTime(2021, 03, 17, 18, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 6pm
				new DateTimeOffset(new DateTime(2021, 03, 18, 00, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Thursday @ midnight
				null,
			};

			// No valid executions = null.
			yield return new object[]
			{
				new TimeSpan[0],
				new DateTimeOffset(new DateTime(2021, 03, 17, 18, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 6pm
				(DateTimeOffset?)null,
				null,
			};

			// Exact current time now.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new DateTimeOffset(new DateTime(2021, 03, 17, 17, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 6pm
				new DateTimeOffset(new DateTime(2021, 03, 17, 17, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Thursday @ midnight
				null,
			};

			// One minute past returns next day.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new DateTimeOffset(new DateTime(2021, 03, 17, 17, 01, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 6pm
				new DateTimeOffset(new DateTime(2021, 03, 18, 17, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Thursday @ midnight
				null,
			};

			// Daylight savings changes
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new DateTimeOffset(new DateTime(2021, 03, 17, 17, 01, 00), TimeZoneInfo.Local.BaseUtcOffset), // Wednesday @ 6pm
				new DateTimeOffset(new DateTime(2021, 03, 18, 17, 00, 00), TimeZoneInfo.Local.BaseUtcOffset), // Thursday @ midnight
				null,
			};

			// Custom Timezone different day in UTC
			var auTimezone = TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time");
			yield return new object[]
			{
				new []{ new TimeSpan(10, 0, 0) },
				TimeZoneInfo.ConvertTime(new DateTimeOffset(2024, 04, 16, 22, 00, 00, TimeSpan.Zero), auTimezone), // Tuesday @ 6am
				TimeZoneInfo.ConvertTime(new DateTimeOffset(2024, 04, 17, 2, 00, 00, TimeSpan.Zero), auTimezone), // Wednesday @ 10am
				auTimezone,
			};

			// Custom Timezone different day in local
			yield return new object[]
			{
				new []{ new TimeSpan(10, 0, 0) },
				TimeZoneInfo.ConvertTime(new DateTimeOffset(2024, 04, 16, 14, 00, 00,TimeSpan.Zero), auTimezone), // Tuesday @ 6am
				TimeZoneInfo.ConvertTime(new DateTimeOffset(2024, 04, 17, 2, 00, 00, TimeSpan.Zero), auTimezone), // Wednesday @ 10am
				auTimezone,
			};

			// UTC Timezone different day
			yield return new object[]
			{
				new []{ new TimeSpan(10, 0, 0) },
				new DateTimeOffset(2024, 04, 16, 14, 00, 00,TimeSpan.Zero), // Tuesday @ 6am
				new DateTimeOffset(2024, 04, 17, 10, 00, 00, TimeSpan.Zero), // Wednesday @ 10am
				TimeZoneInfo.Utc,
			};
		}
	}
}
