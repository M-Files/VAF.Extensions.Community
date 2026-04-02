using MFiles.VAF.Extensions;
using MFiles.VAF.Extensions.Configuration.ScheduledExecution;
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
			DateTimeOffset?[] expected,
			DayOfMonthTriggerType triggerType = DayOfMonthTriggerType.SpecificDate,
			int nthWeekday = -1,
			DayOfWeek dayOfWeek = DayOfWeek.Sunday
		)
		{
			DateTimeOffset[] result;
			if (triggerType == DayOfMonthTriggerType.SpecificDate)
			{
				result = DayOfMonthTrigger
				.GetNextDayOfMonth(after, dayOfMonth, unrepresentableDateHandling)?
				.ToArray();
			}
			else
			{
				result = DayOfMonthTrigger
				.GetNextDayOfMonth(after, dayOfMonth, unrepresentableDateHandling, triggerType, nthWeekday, dayOfWeek)?
				.ToArray();
			}

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Length, result.Length);
			for (var i = 0; i < result.Length; i++)
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
			DateTimeOffset? expected,
			DayOfMonthTriggerType type = DayOfMonthTriggerType.SpecificDate,
			int nthWeekday = -1,
			DayOfWeek dayOfWeek=DayOfWeek.Sunday,
			UnrepresentableDateHandling unrep = UnrepresentableDateHandling.Skip
		)
		{
			DateTimeOffset? execution;
			if(type == DayOfMonthTriggerType.SpecificDate)
			{

				execution = new DayOfMonthTrigger()
				{
					TriggerTimes = triggerTimes.ToList(),
					TriggerDays = triggerDays.ToList()
				}.GetNextExecution(after, TimeZoneInfo.Utc);
			}
			else
			{

				execution = new DayOfMonthTrigger()
				{
					TriggerTimes = triggerTimes.ToList(),
					DayType = type, 
					NthDay = nthWeekday,
					Weekday = dayOfWeek,
					UnrepresentableDateHandling = unrep
				}.GetNextExecution(after, TimeZoneInfo.Utc);
			}
			Assert.AreEqual(expected?.ToUniversalTime(), execution?.ToUniversalTime());
		}
		[TestMethod]
		[DynamicData(nameof(GetVariableDateOfMonthData), DynamicDataSourceType.Method)]
		public void GetVariableDateOfMonth
		(
			DateTimeOffset date,
			int nthWeekday,
			DayOfWeek dayOfWeek,
			int expected
		)
		{
			var result = DayOfMonthTrigger.GetVariableDateOfMonth(
				date, nthWeekday, dayOfWeek);
			Assert.IsNotNull(result);
			Assert.AreEqual(expected, result);

		}
		[TestMethod]
		[DynamicData(nameof(GetNextMonthWithValidVariableDateData), DynamicDataSourceType.Method)]
		public void GetNextMonthWithValidVariableDate
		(
			DateTimeOffset date,
			UnrepresentableDateHandling unrepresentableDateHandling,
			int nthWeekday,
			DayOfWeek dayOfWeek,
			DateTimeOffset? expected
		)
		{
			var result = DayOfMonthTrigger.GetNextMonthWithValidVariableDate(
				date, unrepresentableDateHandling, nthWeekday, dayOfWeek);
			Assert.IsNotNull(result);
			Assert.AreEqual(expected, result);

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
			/*
			 Variable Date tests
			 Test cases roughly match up with those of GetNextDayOfMonth.
			 Three Main Cases (not including Invalids): 
				A: Cur Date < This Month Run Date - Get This Month's run (or skip/EoM if scheduled date DNE)
				B: Cur Date > This Month Run Date - Only return next month's run.  
				C: Cur Date = This Month Run Date - This Month's run (Either b/c of normal sched or because of EoM) & Next Month's Run.
			 
			 */
			
			// Case A - Normal - 1st Sun 
			yield return new object[]
			{
				new []{new TimeSpan(0, 0, 0) }, // 12AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 1, 2, 35, 0, TimeSpan.Zero), // Feb 1 2025 2:35 AM
				 new DateTimeOffset(2025, 2,2,0,0,0,TimeSpan.Zero), // Feb 2 2025 12AM
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Sunday,
				UnrepresentableDateHandling.Skip
			};
			// Case A - Normal - 2nd Tuesday
			yield return new object[]
			{
				new []{new TimeSpan(2, 0, 0) }, // 2AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 2, 3, 35, 0, TimeSpan.Zero), // Feb 2 2025 3:35AM
			    new DateTimeOffset(2025, 2,11,2,0,0,TimeSpan.Zero), // Feb 11 2025 2AM
				DayOfMonthTriggerType.VariableDate,
				2,
				DayOfWeek.Tuesday,
				UnrepresentableDateHandling.Skip
			};
			// Case A - Normal - 4th Friday (last day)
			yield return new object[]
			{
				new []{new TimeSpan(3, 0, 0) }, // 3AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 3, 4, 35, 0, TimeSpan.Zero), // Feb 3 2025 4:35AM
				new DateTimeOffset(2025, 2,28,3,0,0,TimeSpan.Zero), // Feb 28 2025 3 AM
				DayOfMonthTriggerType.VariableDate,
				4,
				DayOfWeek.Friday,
				UnrepresentableDateHandling.Skip
			};
			// Case A - DNE - EoM - 5th Saturday
			yield return new object[]
			{
				new []{new TimeSpan(4, 0, 0) }, // 4AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 4, 5, 35, 0, TimeSpan.Zero), // Feb 4 2025 5:35AM
				new DateTimeOffset(2025, 2,28,4,0,0,TimeSpan.Zero), // Feb 28 2025 4AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday,
				UnrepresentableDateHandling.LastDayOfMonth,
			};
			// Case A - DNE - EoM - 5th Friday
			yield return new object[]
			{
				new []{new TimeSpan(5, 0, 0) }, // 5AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 4, 6, 35, 0, TimeSpan.Zero), // Feb 4 2025 6:35AM
				new DateTimeOffset(2025, 2,28,5,0,0,TimeSpan.Zero), // Feb 28 2025 5AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday,
				UnrepresentableDateHandling.LastDayOfMonth,
			};
			// A - DNE - SKIP - 5th Saturday - Skip 1 Month
			yield return new object[]
			{
				new []{new TimeSpan(12, 0, 0) }, // 12PM
				new []{ -1},
				new DateTimeOffset(2025, 2, 4,7, 35, 0, TimeSpan.Zero), // Feb 4 2025 7:35AM
				new DateTimeOffset(2025, 3,29,12,0,0,TimeSpan.Zero), // Mar 29 2025 12PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday,
				UnrepresentableDateHandling.Skip,
			};
			// A - DNE - SKIP - 5th Tuesday- Skip 2 Months
			yield return new object[]
			{
				new []{new TimeSpan(13, 0, 0) }, // 1PM
				new []{ -1},
				new DateTimeOffset(2025, 2, 4, 8, 35, 0, TimeSpan.Zero), // Feb 4 2025 8:35AM
				new DateTimeOffset(2025, 4,29,13,0,0,TimeSpan.Zero), // Apr 29 2025 1PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Tuesday,
				UnrepresentableDateHandling.Skip
			};
			// A- EoY - DNE - EoM - 5th Friday
			yield return new object[]
			{
				new []{new TimeSpan(14, 0, 0) }, // 2PM
				new []{ -1},
				new DateTimeOffset(2025, 12, 4, 9, 35, 0, TimeSpan.Zero), // Dec 4 2025 9:35AM
				new DateTimeOffset(2025, 12,31,14,0,0,TimeSpan.Zero), // Dec 31 2025 2PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// A - EoY - DNE - SKIP - 5th Friday
			yield return new object[]
			{
				new []{new TimeSpan(15, 0, 0) }, // 3PM
				new []{ -1},
				new DateTimeOffset(2025, 12, 4, 10, 35, 0, TimeSpan.Zero), // Dec 4 2025 10:35AM
				new DateTimeOffset(2026, 1,30,15,0,0,TimeSpan.Zero), // Jan 30 2026 3PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday,
				UnrepresentableDateHandling.Skip
			};
			// A- EoY - DNE - SKIP - 5th Sunday - skip 3 months
			yield return new object[]
			{
				new []{new TimeSpan(16, 0, 0) }, // 4PM
				new []{ -1},
				new DateTimeOffset(2025, 12, 4, 11, 35, 0, TimeSpan.Zero), // Dec 4 2025 11:35AM
				new DateTimeOffset(2026, 3,29,16,0,0,TimeSpan.Zero), // Mar 29 2026 4PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Sunday,
				UnrepresentableDateHandling.Skip
			};
			// B - Normal - 1st Sun
			yield return new object[]
			{
				new []{new TimeSpan(17, 0, 0) }, // 5PM
				new []{ -1},
				new DateTimeOffset(2025, 1, 20, 12, 35, 0, TimeSpan.Zero), // Jan 20 2025 12:35PM
				new DateTimeOffset(2025, 2,2,17,0,0,TimeSpan.Zero), // Feb 2 2025 5PM
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Sunday,
				UnrepresentableDateHandling.Skip
			};
			// B - Normal - 2nd Tuesday
			yield return new object[]
			{
				new []{new TimeSpan(18, 0, 0) }, // 6PM
				new []{ -1},
				new DateTimeOffset(2025, 1, 20, 13, 35, 0, TimeSpan.Zero), // Jan 20 2025 1:35PM
			    new DateTimeOffset(2025, 2,11,18,0,0,TimeSpan.Zero), // Feb 11 2025 6PM
				DayOfMonthTriggerType.VariableDate,
				2,
				DayOfWeek.Tuesday,
				UnrepresentableDateHandling.Skip
			};
			// B-Normal - 4th Friday (last day)
			yield return new object[]
			{
				new []{new TimeSpan(19, 0, 0) }, // 7PM
				new []{ -1},
				new DateTimeOffset(2025, 1, 25, 14, 35, 0, TimeSpan.Zero), // Jan 25 2025 2:35PM
			    new DateTimeOffset(2025, 2,28,19,0,0,TimeSpan.Zero), // Feb 28 2025 7PM
				DayOfMonthTriggerType.VariableDate,
				4,
				DayOfWeek.Friday,
				UnrepresentableDateHandling.Skip
			};
			// B - DNE - EoM - 5th Thursday
			yield return new object[]
			{
				new []{new TimeSpan(20, 0, 0) }, // 8PM
				new []{ -1},
				new DateTimeOffset(2025, 1, 31, 15, 35, 0, TimeSpan.Zero), // Jan 31 2025 3:35PM
				new DateTimeOffset(2025, 2,28,20,0,0,TimeSpan.Zero), // Feb 28 2025 8PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// B - DNE - SKIP - 5th Wednesday - Skip 2 Months
			yield return new object[]
			{
				new []{new TimeSpan(21, 0, 0) }, // 9PM
				new []{ -1},
				new DateTimeOffset(2025, 1, 30, 16, 35, 0, TimeSpan.Zero), // Jan 30 2025 4:35PM
				new DateTimeOffset(2025, 4,30,21,0,0,TimeSpan.Zero), // Apr 30 2025 9PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Wednesday,
				UnrepresentableDateHandling.Skip
			};
			// B - EoY - Normal - 4th Thursday 
			yield return new object[]
			{
				new []{new TimeSpan(22, 0, 0) }, // 10PM
				new []{ -1},
				new DateTimeOffset(2025, 12, 28, 17, 35, 0, TimeSpan.Zero), // Dec 28 2025 5:35 PM
				new DateTimeOffset(2026, 1,22,22,0,0,TimeSpan.Zero), // Jan 22 2026 10PM 
				DayOfMonthTriggerType.VariableDate,
				4,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.Skip
			};
			// B - EoY -DNE - SKIP - 5th Saturday Skip 2 Months from Nov
			yield return new object[]
			{
				new []{new TimeSpan(23, 0, 0) }, // 11PM
				new []{ -1},
				new DateTimeOffset(2025, 11, 30, 18, 35, 0, TimeSpan.Zero), // Nov 30 2025 6:35PM
				new DateTimeOffset(2026, 1, 31,23,0,0,TimeSpan.Zero), // Jan 31 2026 11PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday,
				UnrepresentableDateHandling.Skip
			};
			// B - EoY DNE - EoM - 5th Monday
			yield return new object[]
			{
				new []{new TimeSpan(0, 0, 0) }, // 12AM
				new []{ -1},
				new DateTimeOffset(2025, 12, 30, 19, 35, 0, TimeSpan.Zero), // Dec 30 2025 7:35PM
				new DateTimeOffset(2026, 1, 31,0,0,0,TimeSpan.Zero), // Jan 31 2026 12AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// B - EoY DNE -SKIP - 5th Monday
			yield return new object[]
			{
				new []{new TimeSpan(1, 0, 0) }, // 1AM
				new []{ -1},
				new DateTimeOffset(2025, 12, 30, 20, 35, 0, TimeSpan.Zero), // Dec 30 2025 8:35PM
				new DateTimeOffset(2026, 3, 30,1,0,0,TimeSpan.Zero), // Mar 30 2026 1AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.Skip
			};
			// C - Normal - 1st Wednesday - same day
			yield return new object[]
			{
				new []{new TimeSpan(1, 0, 0) }, // 1AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 5, 0, 45, 0, TimeSpan.Zero), // Feb 5 12:45AM
				new DateTimeOffset(2025, 2, 5,1,0,0,TimeSpan.Zero), // Feb 5 1AM
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Wednesday,
				UnrepresentableDateHandling.Skip
			};
			// C - Normal - 1st Wednesday - past
			yield return new object[]
			{
				new []{new TimeSpan(1, 0, 0) }, // 1AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 5, 21, 35, 0, TimeSpan.Zero), // Feb 5 9:35PM
				new DateTimeOffset(2025, 3, 5,1,0,0,TimeSpan.Zero), // Mar 5 1AM
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Wednesday,
				UnrepresentableDateHandling.Skip
			};
			// C - Normal - 3rd Sunday - same day
			yield return new object[]
			{
				new []{new TimeSpan(3, 0, 0) }, // 3AM
				new []{ -1},
				new DateTimeOffset(2025, 3, 16, 1, 0, 0, TimeSpan.Zero), // Mar 16 1AM
				new DateTimeOffset(2025, 3, 16,3,0,0,TimeSpan.Zero), // Mar 16 3AM
				DayOfMonthTriggerType.VariableDate,
				3,
				DayOfWeek.Sunday,
				UnrepresentableDateHandling.Skip
			};
			// C - Normal - 3rd Sunday - past
			yield return new object[]
			{
				new []{new TimeSpan(3, 0, 0) }, // 3AM
				new []{ -1},
				new DateTimeOffset(2025, 3, 16, 22, 35, 0, TimeSpan.Zero), // Mar 16 10:35PM
				new DateTimeOffset(2025, 4, 20,3,0,0,TimeSpan.Zero), // Apr 20 3AM
				DayOfMonthTriggerType.VariableDate,
				3,
				DayOfWeek.Sunday,
				UnrepresentableDateHandling.Skip
			};
			// C - Next Month DNE - SKIP- 5th Thursday - same day
			yield return new object[]
			{
				new []{new TimeSpan(4, 0, 0) }, // 4AM
				new []{ -1},
				new DateTimeOffset(2025, 5, 29, 1, 35, 0, TimeSpan.Zero), // May 29 1:35AM
				new DateTimeOffset(2025, 5, 29,4,0,0,TimeSpan.Zero), // May 29 4AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.Skip
			};
			// C - Next Month DNE - SKIP- 5th Thursday - past
			yield return new object[]
			{
				new []{new TimeSpan(4, 0, 0) }, // 4AM
				new []{ -1},
				new DateTimeOffset(2025, 5, 29, 23, 35, 0, TimeSpan.Zero), // May 29 11:35PM
				new DateTimeOffset(2025, 7, 31,4,0,0,TimeSpan.Zero), // July 31 4AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.Skip
			};
			// C - 2 Months DNE - SKIP - 5th Friday - same day
			yield return new object[]
			{
				new []{new TimeSpan(6, 0, 0) }, // 6AM
				new []{ -1},
				new DateTimeOffset(2025, 5, 30, 0, 35, 0, TimeSpan.Zero), // May 30 12:35AM
				new DateTimeOffset(2025, 5, 30,6,0,0,TimeSpan.Zero), // May 30 6AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday,
				UnrepresentableDateHandling.Skip
			};
			// C - 2 Months DNE - SKIP - 5th Friday - past
			yield return new object[]
			{
				new []{new TimeSpan(6, 0, 0) }, // 6AM
				new []{ -1},
				new DateTimeOffset(2025, 5, 30, 12, 35, 0, TimeSpan.Zero), // May 30 12:35PM
				new DateTimeOffset(2025, 8, 29,6,0,0,TimeSpan.Zero), //  Aug 30 6AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday,
				UnrepresentableDateHandling.Skip
			};
			// C - Next Month DNE - EoM - 5th Thurs - same day
			yield return new object[]
			{
				new []{new TimeSpan(6, 0, 0) }, // 6AM
				new []{ -1},
				new DateTimeOffset(2025, 5,29, 1, 35, 0, TimeSpan.Zero), // May 29 1:35AM
				new DateTimeOffset(2025, 5, 29,6,0,0,TimeSpan.Zero), // May 29 6AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - Next Month DNE - EoM - 5th Thurs - past
			yield return new object[]
			{
				new []{new TimeSpan(6, 0, 0) }, // 6AM
				new []{ -1},
				new DateTimeOffset(2025, 5,29, 13, 35, 0, TimeSpan.Zero), // May 29 1:35PM
				new DateTimeOffset(2025, 6, 30,6,0,0,TimeSpan.Zero), // June 30 6AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - This  Month DNE - EoM - 5th Monday - same day
			yield return new object[]
			{
				new []{new TimeSpan(7, 0, 0) }, // 7AM
				new []{ -1},
				new DateTimeOffset(2025, 5,31, 2, 35, 0, TimeSpan.Zero), // May 31 2:35AM
				new DateTimeOffset(2025, 5, 31,7,0,0,TimeSpan.Zero), // May 31 7AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - This  Month DNE - EoM - 5th Monday - past
			yield return new object[]
			{
				new []{new TimeSpan(7, 0, 0) }, // 7AM
				new []{ -1},
				new DateTimeOffset(2025, 5,31, 14, 35, 0, TimeSpan.Zero), // May 31 2:35PM
				new DateTimeOffset(2025, 6, 30,7,0,0,TimeSpan.Zero), // June 30 7AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - EoY - Normal - 3rd Saturday - same day
			yield return new object[]
			{
				new []{new TimeSpan(8, 0, 0) }, // 8AM
				new []{ -1},
				new DateTimeOffset(2025, 12,20, 2, 35, 0, TimeSpan.Zero), // Dec 20 2:35AM
				new DateTimeOffset(2025, 12, 20,8,0,0,TimeSpan.Zero), // Dec 20 8AM
				DayOfMonthTriggerType.VariableDate,
				3,
				DayOfWeek.Saturday,
				UnrepresentableDateHandling.Skip
			};
			// C - EoY - Normal - 3rd Saturday - past
			yield return new object[]
			{
				new []{new TimeSpan(8, 0, 0) }, // 8AM
				new []{ -1},
				new DateTimeOffset(2025, 12,20, 14, 35, 0, TimeSpan.Zero), // Dec 20 2025 2:35PM
				new DateTimeOffset(2026, 1, 17,8,0,0,TimeSpan.Zero), // Jan 17 2026 8AM
				DayOfMonthTriggerType.VariableDate,
				3,
				DayOfWeek.Saturday,
				UnrepresentableDateHandling.Skip
			};
			// C - EoY - Next Month DNE - 5th Monday - same day
			yield return new object[]
			{
				new []{new TimeSpan(9, 0, 0) }, // 9AM
				new []{ -1},
				new DateTimeOffset(2025, 12,29, 3, 35, 0, TimeSpan.Zero), // Dec 29 3:35AM
				new DateTimeOffset(2025, 12, 29,9,0,0,TimeSpan.Zero), // Dec 29 9AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.Skip
			};
			// C - EoY - Next Month DNE - 5th Monday - past
			yield return new object[]
			{
				new []{new TimeSpan(9, 0, 0) }, // 9AM
				new []{ -1},
				new DateTimeOffset(2025, 12,29, 15, 35, 0, TimeSpan.Zero), // Dec 29 2025 3:35PM
				new DateTimeOffset(2026, 3, 30,9,0,0,TimeSpan.Zero), // Mar 30 2026 9AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.Skip
			};
			// C - EoY - Next Month DNE - EoM - 5th Monday - same day
			yield return new object[]
			{
				new []{new TimeSpan(10, 0, 0) }, // 10AM
				new []{ -1},
				new DateTimeOffset(2025, 12,29, 5, 35, 0, TimeSpan.Zero), // Dec 29 2025 5:35AM
				 new DateTimeOffset(2025, 12, 29,10,0,0,TimeSpan.Zero), // Dec 29 2025 10AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - EoY - Next Month DNE - EoM - 5th Monday - past
			yield return new object[]
			{
				new []{new TimeSpan(10, 0, 0) }, // 10AM
				new []{ -1},
				new DateTimeOffset(2025, 12,29, 17, 35, 0, TimeSpan.Zero), // Dec 29 2025 5:35PM
				 new DateTimeOffset(2026, 1, 31,10,0,0,TimeSpan.Zero), // Jan 31 2026 10AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - EoY  -This Month DNE -   EoM - 5th Thurday - same day
			yield return new object[]
			{
				new []{new TimeSpan(11, 0, 0) }, // 11AM
				new []{ -1},
				new DateTimeOffset(2025, 12,31, 6, 35, 0, TimeSpan.Zero), // Dec 31 2025 6:35AM
				new DateTimeOffset(2025, 12, 31,11,0,0,TimeSpan.Zero), // Dec 31 2025 11AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - EoY  -This Month DNE -   EoM - 5th Thurday - past
			yield return new object[]
			{
				new []{new TimeSpan(11, 0, 0) }, // 11AM
				new []{ -1},
				new DateTimeOffset(2025, 12,31, 18, 35, 0, TimeSpan.Zero), // Dec 31 2025 6:35PM
				new DateTimeOffset(2026, 1, 29,11,0,0,TimeSpan.Zero), // Jan 29 2026 11AM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - EoY - This AND Next Month DNE - EoM - 5th Sunday - same day
			yield return new object[]
			{
				new []{new TimeSpan(12, 0, 0) }, // 12PM
				new []{ -1},
				new DateTimeOffset(2025, 12,31, 10, 0, 0, TimeSpan.Zero),// Dec 31 10AM
				new DateTimeOffset(2025, 12, 31,12,0,0,TimeSpan.Zero), // Dec 31 12PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Sunday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - EoY - This AND Next Month DNE - EoM - 5th Sunday - past
			yield return new object[]
			{
				new []{new TimeSpan(12, 0, 0) }, // 12PM
				new []{ -1},
				new DateTimeOffset(2025, 12,31, 20, 0, 0, TimeSpan.Zero),// Dec 31 10PM
				new DateTimeOffset(2026, 1, 31,12,0,0,TimeSpan.Zero), // Jan 31 2026 12PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Sunday,
				UnrepresentableDateHandling.LastDayOfMonth
			};
			// C - EoY(Nov) - Next Month DNE - SKIP - same day
			yield return new object[]
			{
				new []{new TimeSpan(13, 0, 0) }, // 1PM
				new []{ -1},
				new DateTimeOffset(2025, 11,29, 12, 0, 0, TimeSpan.Zero), // Nov 29 12PM
				new DateTimeOffset(2025, 11, 29,13,0,0,TimeSpan.Zero), // Nov 29 1PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday,
				UnrepresentableDateHandling.Skip
			};
			// C - EoY(Nov) - Next Month DNE - SKIP - past
			yield return new object[]
			{
				new []{new TimeSpan(13, 0, 0) }, // 1PM
				new []{ -1},
				new DateTimeOffset(2025, 11,29, 14, 0, 0, TimeSpan.Zero), // Nov 29 2PM
				new DateTimeOffset(2026, 1, 31,13,0,0,TimeSpan.Zero), // Jan 31 2026 1PM
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday,
				UnrepresentableDateHandling.Skip
			};

			// Multiple times testing
			// Case A - Normal - 2nd Tuesday (multiple matching, return first)
			yield return new object[]
			{
				new []{new TimeSpan(2, 0, 0), new TimeSpan(14,0,0) }, // 2AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 2, 3, 35, 0, TimeSpan.Zero), // Feb 2 2025 3:35AM
			    new DateTimeOffset(2025, 2,11,2,0,0,TimeSpan.Zero), // Feb 11 2025 2AM
				DayOfMonthTriggerType.VariableDate,
				2,
				DayOfWeek.Tuesday,
				UnrepresentableDateHandling.Skip
			};
			// B - Normal - 2nd Tuesday (multiple matching, return first)
			yield return new object[]
			{
				new []{new TimeSpan(18, 0, 0), new TimeSpan(23,0,0) }, // 6PM
				new []{ -1},
				new DateTimeOffset(2025, 1, 20, 13, 35, 0, TimeSpan.Zero), // Jan 20 2025 1:35PM
			    new DateTimeOffset(2025, 2,11,18,0,0,TimeSpan.Zero), // Feb 11 2025 6PM
				DayOfMonthTriggerType.VariableDate,
				2,
				DayOfWeek.Tuesday,
				UnrepresentableDateHandling.Skip
			};
			// C - Normal - 1st Wednesday - both past
			yield return new object[]
			{
				new []{new TimeSpan(1, 0, 0), new TimeSpan(11, 0, 0)  }, // 1AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 5, 21, 35, 0, TimeSpan.Zero), // Feb 5 9:35PM
				new DateTimeOffset(2025, 3, 5,1,0,0,TimeSpan.Zero), // Mar 5 1AM
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Wednesday,
				UnrepresentableDateHandling.Skip
			};
			// C - Normal - 1st Wednesday - both future same day
			yield return new object[]
			{
				new []{new TimeSpan(22, 0, 0), new TimeSpan(23, 0, 0)  }, // 10PM, 11PM
				new []{ -1},
				new DateTimeOffset(2025, 2, 5, 21, 35, 0, TimeSpan.Zero), // Feb 5 9:35PM
				new DateTimeOffset(2025, 2, 5,22,0,0,TimeSpan.Zero), // Feb 5 10P
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Wednesday,
				UnrepresentableDateHandling.Skip
			};
			// C - Normal - 1st Wednesday - one past, one future same day
			yield return new object[]
			{
				new []{new TimeSpan(1, 0, 0), new TimeSpan(11, 0, 0)  }, // 1AM, 11AM
				new []{ -1},
				new DateTimeOffset(2025, 2, 5, 9, 35, 0, TimeSpan.Zero), // Feb 5 9:35AM
				new DateTimeOffset(2025, 2, 5,11,0,0,TimeSpan.Zero), // Feb 5 11AM
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Wednesday,
				UnrepresentableDateHandling.Skip
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

			/*
			 Three Main Cases (not including Invalids): 
				A: Cur Date < This Month Run Date - Get This Month's run (or skip/EoM if scheduled date DNE)
				B: Cur Date > This Month Run Date - Only return next month's run.  
				C: Cur Date = This Month Run Date - This Month's run (Either b/c of normal sched or because of EoM) & Next Month's Run.
			 
			 */
			// INVALID: -1st Sat of Month -> null
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 5, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[0],
				DayOfMonthTriggerType.VariableDate,
				-1,
				DayOfWeek.Saturday
			};
			// INVALID: 6th Sat of Month -> null
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 5, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[0],
				DayOfMonthTriggerType.VariableDate,
				6,
				DayOfWeek.Saturday
			};
			// Case A - Normal - 1st Sun 
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,2,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Sunday
			};
			// Case A - Normal - 2nd Tuesday
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 2, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,11,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				2,
				DayOfWeek.Tuesday
			};
			// Case A - Normal - 4th Friday (last day)
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 3, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,28,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				4,
				DayOfWeek.Friday
			};
			// Case A - DNE - EoM - 5th Saturday
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 4, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,28,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday
			};
			// Case A - DNE - EoM - 5th Friday
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 4, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,28,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday
			};
			// A - DNE - SKIP - 5th Saturday - Skip 1 Month
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 4,0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 3,29,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday
			};
			// A - DNE - SKIP - 5th Tuesday- Skip 2 Months
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 4, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 4,29,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Tuesday
			};
			// A- EoY - DNE - EoM - 5th Friday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 4, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 12,31,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday
			};
			// A - EoY - DNE - SKIP - 5th Friday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 4, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2026, 1,30,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday
			};
			// A- EoY - DNE - SKIP - 5th Sunday - skip 3 months
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 4, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2026, 3,29,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Sunday
			};
			// B - Normal - 1st Sun
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 20, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,2,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Sunday
			};
			// B - Normal - 2nd Tuesday
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 20, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,11,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				2,
				DayOfWeek.Tuesday
			};
			// B-Normal - 4th Friday (last day)
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 25, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,28,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				4,
				DayOfWeek.Friday
			};
			// B - DNE - EoM - 5th Thursday
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 31, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2,28,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday
			};
			// B - DNE - SKIP - 5th Wednesday - Skip 2 Months
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 30, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 4,30,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Wednesday
			};
			// B - EoY - Normal - 4th Thursday 
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 28, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2026, 1,22,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				4,
				DayOfWeek.Thursday
			};
			// B - EoY -DNE - SKIP - 5th Saturday Skip 2 Months from Nov
			yield return new object[]
			{
				new DateTimeOffset(2025, 11, 30, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2026, 1, 31,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday
			};
			// B - EoY DNE - EoM - 5th Monday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 30, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2026, 1, 31,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday
			};
			// B - EoY DNE -SKIP - 5th Monday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 30, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2026, 3, 30,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday
			};
			// C - Normal - 1st Wednesday
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 5, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 2, 5,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2025, 3, 5,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				1,
				DayOfWeek.Wednesday
			};
			// C - Normal - 3rd Sunday
			yield return new object[]
			{
				new DateTimeOffset(2025, 3, 16, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 3, 16,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2025, 4, 20,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				3,
				DayOfWeek.Sunday
			};
			// C - Next Month DNE - SKIP- 5th Thursday
			yield return new object[]
			{
				new DateTimeOffset(2025, 5, 29, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 5, 29,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2025, 7, 31,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday
			};
			// C - 2 Months DNE - SKIP - 5th Friday
			yield return new object[]
			{
				new DateTimeOffset(2025, 5, 30, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 5, 30,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2025, 8, 29,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Friday
			};
			// C - Next Month DNE - EoM - 5th Thurs
			yield return new object[]
			{
				new DateTimeOffset(2025, 5,29, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 5, 29,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2025, 6, 30,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday
			};
			// C - This  Month DNE - EoM - 5th Monday
			yield return new object[]
			{
				new DateTimeOffset(2025, 5,31, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 5, 31,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2025, 6, 30,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday
			};
			// C - EoY - Normal - 3rd Saturday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12,20, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 12, 20,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2026, 1, 17,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				3,
				DayOfWeek.Saturday
			};
			// C - EoY - Next Month DNE - 5th Monday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12,29, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 12, 29,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2026, 3, 30,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday
			};
			// C - EoY - Next Month DNE - EoM - 5th Monday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12,29, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 12, 29,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2026, 1, 31,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Monday
			};
			// C - EoY  -This Month DNE -   EoM - 5th Thurday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12,31, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 12, 31,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2026, 1, 29,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Thursday
			};
			// C - EoY - This AND Next Month DNE - EoM - 5th Sunday
			yield return new object[]
			{
				new DateTimeOffset(2025, 12,31, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 12, 31,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2026, 1, 31,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Sunday
			};
			// C - EoY(Nov) - Next Month DNE - SKIP
			yield return new object[]
			{
				new DateTimeOffset(2025, 11,29, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]{ new DateTimeOffset(2025, 11, 29,0,0,0,TimeSpan.Zero),
									   new DateTimeOffset(2026, 1, 31,0,0,0,TimeSpan.Zero)},
				DayOfMonthTriggerType.VariableDate,
				5,
				DayOfWeek.Saturday
			};
			
		}
		public static IEnumerable<object[]> GetVariableDateOfMonthData()
		{
			//Test Case #1: -1st Wednesday (Invalid) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 23, 1, 2, 3, TimeSpan.Zero),
				-1,
				DayOfWeek.Wednesday,
				-1
			};
			//Test Case #2: 6th Thursday (Invalid) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 23, 1, 2, 3, TimeSpan.Zero),
				6,
				DayOfWeek.Thursday,
				-1
			};
			//Test Case #3: 1st Friday of Jan 2025 -> 3
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 23, 1, 2, 3, TimeSpan.Zero),
				1,
				DayOfWeek.Friday,
				3
			};
			//Test Case #4 1st Wednesday of Jan 2025 (1st of the month) -> 1
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 23, 1, 2, 3, TimeSpan.Zero),
				1,
				DayOfWeek.Wednesday,
				1
			};
			//Test Case #5: 2nd Thursday of Jan 2025 -> 9
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 3, 1, 2, 3, TimeSpan.Zero),
				2,
				DayOfWeek.Thursday,
				9
			};
			//Test Case: #6 2nd Wednesday of Jan 2025 -> 8
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				2,
				DayOfWeek.Wednesday,
				8
			};
			//Test Case #7: 1st Tuesday of Jan 2025 -> 7
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				1,
				DayOfWeek.Tuesday,
				7
			};
			//Test Case #8: 4th Monday of Jan 2025 -> 27
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Monday,
				27
			};
			//Test Case #9: 5th Wednesday of Jan 2025 -> 27
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Wednesday,
				29
			};
			//Test Case #10: 5th Friday of Jan 2025 (Last day of month) -> 31
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Friday,
				31
			};
			//Test Case #11: 5th Saturday of Jan 2025 (Does not exist) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Saturday, 
				-1
			};
			//Test Case #12: 5th Monday of Jan 2025 (Does not exist) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Monday, 
				-1
			};
			//Test Case #13: Feb Testing: 4th Friday of Feb 2025 -> 28
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Friday, 
				28
			};
			//Test Case #14: Feb Testing: 4th Saturday of Feb 2025 -> 22
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Saturday,
				22
			};
			//Test Case #15: Feb Testing: 5th Saturday of Feb 2025 (Does not exist) -> -1 
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Saturday,
				-1
			};
			//Test Case #16: Feb Testing LEAP YEAR: 4th Monday of Feb 2028-> 28
			yield return new object[]
			{
				new DateTimeOffset(2028, 2, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Monday,
				28
			};
			//Test Case #17: Feb Testing LEAP YEAR: 5th Tuesday of Feb 2028-> 29
			yield return new object[]
			{
				new DateTimeOffset(2028, 2, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Tuesday,
				29
			};
			//Test Case #18: End of year: 5th Wed of Dec 2025 -> 31
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Wednesday,
				31
			};
			// End of year: 5th Thursday of Dec 2025 (DNE) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Thursday,
				-1
			};

		}
		public static IEnumerable<object[]> GetNextMonthWithValidVariableDateData()
		{
			//Test Case #1: Unrepresentable = SKIP - Jan 2025 - 1st Saturday -> 1 Feb 2025 
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				1,
				DayOfWeek.Saturday,
				new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case #2: Unrepresentable = SKIP - Jan 2025 - 1st Sunday -> 2 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				1,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 2, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case #3: Unrepresentable = SKIP - Jan 2025 - 2ND Sunday -> 9 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				2,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 9, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case #4: Unrepresentable = SKIP - Jan 2025 - 3rd Sunday -> 16 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				3,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 16, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case #5: Unrepresentable = SKIP - Jan 2025 - 4TH Sunday -> 23 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				4,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 23, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case #6: Unrepresentable = SKIP - Jan 2025 - 4TH Friday -> 28 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				4,
				DayOfWeek.Friday,
				new DateTimeOffset(2025, 2, 28, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 7: Unrepresentable = SKIP - Jan 2025 - 5TH Saturday (Invalid, Skip Feb) -> 29 March 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Saturday,
				new DateTimeOffset(2025, 3, 29, 0, 0, 0, TimeSpan.Zero)
			};

			//Test Case 8: Unrepresentable = EndOfMonth - Jan 2025 - 5TH Saturday (Invalid, use end of month) -> 28 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.LastDayOfMonth,
				5,
				DayOfWeek.Saturday,
				new DateTimeOffset(2025, 2, 28, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 9: Unrepresentable = SKIP - Jan 2025 - 5TH Wednesday (Invalid, Skip Feb AND Skip Mar) -> 30 April 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Wednesday,
				new DateTimeOffset(2025, 4, 30, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 10: LEAP YEAR - Unrepresentable = SKIP - Jan 2028 - 5th Tuesday -> 29 Feb 2028
			yield return new object[]
			{
				new DateTimeOffset(2028, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Tuesday,
				new DateTimeOffset(2028, 2, 29, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 11: LEAP YEAR - Unrepresentable = SKIP - Jan 2028 - 5th Wednesday (DNE)-> 29 Mar 2028
			yield return new object[]
			{
				new DateTimeOffset(2028, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Wednesday,
				new DateTimeOffset(2028, 3, 29, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 12: LEAP YEAR - Unrepresentable = EoM - Jan 2028 - 5th Wednesday (DNE)-> 29 Feb 2028
			yield return new object[]
			{
				new DateTimeOffset(2028, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.LastDayOfMonth,
				5,
				DayOfWeek.Wednesday,
				new DateTimeOffset(2028, 2, 29, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 13: EoY - Unrepresentable = Skip - Dec 2025 - 5th Saturday (DNE)-> 31 Jan 2026
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Saturday,
				new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 14: EoY - Unrepresentable = Skip - Nov 2025 - 5th Friday (DNE)-> 30 Jan 2026
			yield return new object[]
			{
				new DateTimeOffset(2025, 11, 10, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Friday,
				new DateTimeOffset(2026, 1, 30, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 15: EoY - Unrepresentable = Skip - DEC 2025 - 5th Tuesday (DNE)-> 31 Mar 2026
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 10, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Tuesday,
				new DateTimeOffset(2026, 3, 31, 0, 0, 0, TimeSpan.Zero)
			};
			//Test Case 16: EoY - Unrepresentable = EoM - DEC 2025 - 5th Tuesday (DNE)-> 31 Jan 2026
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 10, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.LastDayOfMonth,
				5,
				DayOfWeek.Tuesday,
				new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero)
			};



		}
	}
}
