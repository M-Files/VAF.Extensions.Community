using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MFiles.VAF.Extensions.ScheduledExecution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	[DataContract]
	internal class TimespanWrapper
	{
		[DataMember]
		public TimeSpan Frequency { get; set; }
	}

	[DataContract]
	internal class FrequencyWrapper
	{
		[DataMember]
		public Frequency Frequency { get; set; }
	}

	[DataContract]
	internal class TimeSpanExWrapper
	{
		[DataMember]
		public TimeSpanEx Frequency { get; set; }
	}

	[DataContract]
	internal class ScheduleWrapper
	{
		[DataMember]
		public Schedule Frequency { get; set; }
	}

	[TestClass]
	public class FrequencyTests
	{
		public static IEnumerable<object[]> SplitTriggerType_Data()
		{
			yield return new object[]
			{
				new DateTime(2022, 10, 06, 20, 01, 00, DateTimeKind.Utc),
				new DateTime(2022, 10, 06, 20, 30, 00, DateTimeKind.Utc)
			};
			yield return new object[]
			{
				TimeZoneInfo.ConvertTimeBySystemTimeZoneId
				(
					new DateTime(2022, 10, 26, 20, 31, 00, DateTimeKind.Local),
					"GMT Standard Time"
				),
				new DateTime(2022, 10, 26, 20, 00, 00, DateTimeKind.Utc)
			};
		}
		public static IEnumerable<object[]> DaylightSaving_ClocksGoBackwards_Data()
		{
			yield return new object[]
			{
				"Before the clocks change",
				new DateTimeOffset(2022, 10, 27, 20, 01, 01, TimeSpan.FromHours(3)),
				new DateTimeOffset(2022, 10, 28, 20, 00, 00, TimeSpan.FromHours(3))
			};
			yield return new object[]
			{
				"Calculated the day before, and then should run on, the day the clocks change",
				new DateTimeOffset(2022, 10, 29, 20, 01, 01, TimeSpan.FromHours(3)),
				new DateTimeOffset(2022, 10, 30, 20, 00, 00, TimeSpan.FromHours(2))
			};
			yield return new object[]
			{
				"Calculated on the day that the clocks change, after they have changed",
				new DateTimeOffset(2022, 10, 30, 20, 01, 01, TimeSpan.FromHours(2)),
				new DateTimeOffset(2022, 10, 31, 20, 00, 00, TimeSpan.FromHours(2))
			};
		}

		[TestMethod]
		[DynamicData(nameof(DaylightSaving_ClocksGoBackwards_Data), DynamicDataSourceType.Method)]
		public void DaylightSaving_ClocksGoBackwards
		(
			string message,
			DateTimeOffset now, 
			DateTimeOffset expected
		)
		{
			var frequency = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(@"{
    ""Triggers"": [
        {
            ""Type"": ""Daily"",
            ""DailyTriggerConfiguration"": {
                ""TriggerTimes"": [
                    ""20:00:00""
                ]
            }
        }
    ],
    ""TriggerTimeType"": ""Custom"",
    ""TriggerTimeCustomTimeZone"": ""FLE Standard Time""
}
");
			{
				var nextRun = frequency.GetNextExecution(now);
				Assert.IsNotNull(nextRun.Value, message);
				Assert.AreEqual(expected, nextRun.Value, message);
			}
		}

		[DynamicData(nameof(SplitTriggerType_Data), DynamicDataSourceType.Method)]
		[TestMethod]
		public void SplitTriggerType(DateTime now, DateTime expected)
		{
			var frequency = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(@"{
    ""Triggers"": [
        {
            ""Type"": ""Weekly"",
            ""WeeklyTriggerConfiguration"": {
                ""TriggerDays"": [
                    ""Monday"",
                    ""Tuesday"",
                    ""Wednesday"",
                    ""Thursday"",
                    ""Friday""
                ],
                ""TriggerTimes"": [
                    ""00:00:00"",
                    ""00:30:00"",
                    ""01:00:00"",
                    ""01:30:00"",
                    ""02:00:00"",
                    ""02:30:00"",
                    ""03:00:00"",
                    ""03:30:00"",
                    ""04:00:00"",
                    ""04:30:00"",
                    ""05:00:00"",
                    ""05:30:00"",
                    ""06:00:00"",
                    ""19:00:00"",
                    ""19:30:00"",
                    ""20:00:00"",
                    ""20:30:00"",
                    ""21:00:00"",
                    ""21:30:00"",
                    ""22:00:00"",
                    ""22:30:00"",
                    ""23:00:00"",
                    ""23:30:00""
                ]
            }
        },
        {
            ""Type"": ""Weekly"",
            ""WeeklyTriggerConfiguration"": {
                ""TriggerDays"": [
                    ""Saturday"",
                    ""Sunday""
                ],
                ""TriggerTimes"": [
                    ""01:30:00"",
                    ""02:00:00"",
                    ""02:30:00"",
                    ""03:00:00"",
                    ""03:30:00"",
                    ""04:00:00"",
                    ""04:30:00"",
                    ""05:00:00"",
                    ""05:30:00"",
                    ""06:00:00"",
                    ""06:30:00"",
                    ""07:00:00"",
                    ""07:30:00"",
                    ""08:00:00"",
                    ""08:30:00"",
                    ""09:00:00"",
                    ""09:30:00"",
                    ""10:00:00"",
                    ""10:30:00"",
                    ""11:00:00"",
                    ""11:30:00"",
                    ""12:00:00"",
                    ""12:30:00"",
                    ""13:00:00"",
                    ""13:30:00"",
                    ""14:00:00"",
                    ""14:30:00"",
                    ""15:00:00"",
                    ""15:30:00"",
                    ""16:00:00"",
                    ""16:30:00"",
                    ""17:00:00"",
                    ""17:30:00"",
                    ""18:00:00"",
                    ""18:30:00"",
                    ""19:00:00"",
                    ""19:30:00"",
                    ""20:00:00"",
                    ""20:30:00"",
                    ""21:00:00"",
                    ""21:30:00"",
                    ""22:00:00"",
                    ""22:30:00""
                ]
            }
        }
    ],
    ""TriggerTimeType"": ""UTC""
}
");
			{
				var nextRun = frequency.GetNextExecution(now);
				Assert.IsNotNull(nextRun.Value);
				Assert.AreEqual(expected, nextRun.Value.ToUniversalTime());
			}
		}

		/// <summary>
		/// Basic test to see that serialization happens correctly with the <see cref="FrequencyJsonConverter"/>
		/// </summary>
		[TestMethod]
		public void SerializesCorrectly_Interval()
		{
			var frequency = new Frequency()
			{
				Interval = new TimeSpan(1, 2, 3),
				RecurrenceType = RecurrenceType.Interval
			};
			var expected = JToken.Parse("{\"RecurrenceType\":1,\"Interval\":{\r\n  \"Hours\" : 1, \"Minutes\" : 2, \"Seconds\" : 3 , \"RunOnVaultStartup\": true\r\n} }");
			var output = JToken.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(frequency));

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Basic test to see that serialization happens correctly with the <see cref="FrequencyJsonConverter"/>
		/// </summary>
		[TestMethod]
		public void SerializesCorrectly_Schedule()
		{
			var frequency = new Frequency()
			{
				Schedule = new Schedule()
				{
					Enabled = true,
					RunOnVaultStartup = false,
					Triggers = new List<Trigger>()
					{
						new DailyTrigger()
						{
							TriggerTimes = new List<TimeSpan>()
							{
								new TimeSpan(2, 3, 4)
							}
						}
					}
				},
				RecurrenceType = RecurrenceType.Schedule
			};
			var expected = JToken.Parse("{\"RecurrenceType\":2,\"Schedule\":{\"Enabled\":true,\"Triggers\":[{\"Type\":1,\"DailyTriggerConfiguration\":{\"TriggerTimes\":[\"02:03:04\"]},\"WeeklyTriggerConfiguration\":{\"TriggerDays\":[],\"TriggerTimes\":[]},\"DayOfMonthTriggerConfiguration\":{\"UnrepresentableDateHandling\":0,\"TriggerDays\":[],\"TriggerTimes\":[]}}],\"RunOnVaultStartup\":false}}");
			var output = JToken.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(frequency));

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Basic test to see that deserialization happens correctly with the <see cref="FrequencyJsonConverter"/>
		/// </summary>
		[TestMethod]
		public void DeserializesCorrectly()
		{
			string json = @"{""RecurrenceType"":1,""Interval"":{ ""Interval"": ""01:02:03"",  ""RunOnVaultStartup"": true}}";
			Frequency output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpan(1, 2, 3),
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExCorrectly_WithoutRunOnVaultStartup()
		{
			string json = @"{ ""Interval"": ""01:02:03""}";
			Frequency output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpanEx(new TimeSpan(1, 2, 3))
				{
					RunOnVaultStartup = true
				},
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExCorrectly_RunOnVaultStartupEqualsFalse()
		{
			string json = @"{ ""Interval"": ""01:02:03"", ""RunOnVaultStartup"": false}";
			Frequency output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpanEx(new TimeSpan(1, 2, 3))
				{
					RunOnVaultStartup = false
				},
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExCorrectly_RunOnVaultStartupEqualsTrue()
		{
			string json = @"{ ""Interval"": ""01:02:03"",  ""RunOnVaultStartup"": true}";
			Frequency output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpan(1, 2, 3),
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check if <see cref="FrequencyJsonConverter"/> can deserialize a TimeSpan
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpan()
		{
			TimeSpan timespan = new TimeSpan(1, 2, 3);
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(timespan);

			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpan(1, 2, 3),
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
			//output.ShouldBeEquivalentTo(expected);
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesFrequencyScheduleCorrectly_Daily()
		{
			string json = "{\"RecurrenceType\":2,\"Schedule\":{\"Triggers\":[{\"Type\":1,\"DailyTriggerConfiguration\":{\"TriggerTimes\":[\"02:03:04\"]}}]}}";
			Frequency output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Schedule = new Schedule()
				{
					Enabled = true,
					Triggers = new List<Trigger>()
					{
						new DailyTrigger()
						{
							TriggerTimes = new List<TimeSpan>()
							{
								new TimeSpan(2, 3, 4)
							}
						}
					}
				},
				RecurrenceType = RecurrenceType.Schedule
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesScheduleCorrectly_Daily()
		{
			string json = "{\"Triggers\":[{\"Type\":1,\"DailyTriggerConfiguration\":{\"TriggerTimes\":[\"02:03:04\"]}}]}";
			Frequency output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Schedule = new Schedule()
				{
					Enabled = true,
					Triggers = new List<Trigger>()
					{
						new DailyTrigger()
						{
							TriggerTimes = new List<TimeSpan>()
							{
								new TimeSpan(2, 3, 4)
							}
						}
					}
				},
				RecurrenceType = RecurrenceType.Schedule
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check if <see cref="FrequencyJsonConverter"/> can deserialize a Schedule
		/// </summary>
		[TestMethod]
		public void DeserializesSchedule()
		{
			Schedule schedule = new Schedule()
			{
				Enabled = true,
				Triggers = new List<Trigger>()
				{
					new Trigger(ScheduleTriggerType.Daily)
					{
						DailyTriggerConfiguration = new DailyTrigger()
						{
							TriggerTimes = new List<TimeSpan>()
							{
								new TimeSpan(1,2,3)
							}
						}
					}
				},
				RunOnVaultStartup = false //Picked false because it's not the default value
			};

			string json = Newtonsoft.Json.JsonConvert.SerializeObject(schedule);
			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<Frequency>(json);
			var expected = new Frequency()
			{
				Schedule = schedule,
				RecurrenceType = RecurrenceType.Schedule,
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check if <see cref="FrequencyJsonConverter"/> can deserialize a TimeSpan contained within a wrapper class
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanWrapper()
		{
			var oldWrapper = new TimespanWrapper()
			{
				Frequency = new TimeSpan(1, 2, 3)
			};

			string json = Newtonsoft.Json.JsonConvert.SerializeObject(oldWrapper);

			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<FrequencyWrapper>(json);
			var expected = new FrequencyWrapper()
			{
				Frequency = new Frequency()
				{
					Interval = new TimeSpan(1, 2, 3),
					RecurrenceType = RecurrenceType.Interval
				}
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check if <see cref="FrequencyJsonConverter"/> can deserialize a TimeSpanEx contained within a wrapper class
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExWrapper()
		{
			var timeSpanEx = new TimeSpanEx(new TimeSpan(1, 2, 3))
			{
				RunOnVaultStartup = false
			};
			var oldWrapper = new TimeSpanExWrapper()
			{
				Frequency = timeSpanEx
			};

			string json = Newtonsoft.Json.JsonConvert.SerializeObject(oldWrapper);

			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<FrequencyWrapper>(json);
			var expected = new FrequencyWrapper()
			{
				Frequency = new Frequency()
				{
					Interval = timeSpanEx,
					RecurrenceType = RecurrenceType.Interval
				}
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check if <see cref="FrequencyJsonConverter"/> can deserialize a Schedule contained within a wrapper class
		/// </summary>
		[TestMethod]
		public void DeserializesScheduleWrapper()
		{
			Schedule schedule = new Schedule()
			{
				Enabled = true,
				Triggers = new List<Trigger>()
				{
					new Trigger(ScheduleTriggerType.Daily)
					{
						DailyTriggerConfiguration = new DailyTrigger()
						{
							TriggerTimes = new List<TimeSpan>()
							{
								new TimeSpan(1,2,3)
							}
						}
					}
				},
				RunOnVaultStartup = false //Picked false because it's not the default value
			};
			var oldWrapper = new ScheduleWrapper()
			{
				Frequency = schedule
			};

			string json = Newtonsoft.Json.JsonConvert.SerializeObject(oldWrapper);

			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<FrequencyWrapper>(json);
			var expected = new FrequencyWrapper()
			{
				Frequency = new Frequency()
				{
					Schedule = schedule,
					RecurrenceType = RecurrenceType.Schedule
				}
			};

			Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(expected), Newtonsoft.Json.JsonConvert.SerializeObject(output));
		}

		[TestMethod]
		public void SerializesDefaultFrequency()
		{
			var config = new FrequencyWrapper();
			var serializedConfig = JsonConvert.SerializeObject(config, new NewtonsoftJsonConvert().JsonSerializerSettings);

			Assert.IsTrue(!serializedConfig.Contains("Frequency"), "Frequency default was included in serialized output.");

		}

	}
}
