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
			var expected = JToken.Parse("{\"RecurrenceType\":1,\"Interval\":{\r\n  \"Interval\": \"01:02:03\",\r\n  \"RunOnVaultStartup\": true\r\n}}");
			var output = JToken.Parse(JsonConvert.SerializeObject(frequency));

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
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
			var output = JToken.Parse(JsonConvert.SerializeObject(frequency));

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Basic test to see that deserialization happens correctly with the <see cref="FrequencyJsonConverter"/>
		/// </summary>
		[TestMethod]
		public void DeserializesCorrectly()
		{
			string json = @"{""RecurrenceType"":1,""Interval"":{ ""Interval"": ""01:02:03"",  ""RunOnVaultStartup"": true}}";
			Frequency output = JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpan(1, 2, 3),
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExCorrectly_WithoutRunOnVaultStartup()
		{
			string json = @"{ ""Interval"": ""01:02:03""}";
			Frequency output = JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpanEx()
				{
					Interval = new TimeSpan(1, 2, 3),
					RunOnVaultStartup = true
				},
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExCorrectly_RunOnVaultStartupEqualsFalse()
		{
			string json = @"{ ""Interval"": ""01:02:03"", ""RunOnVaultStartup"": false}";
			Frequency output = JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpanEx()
				{
					Interval = new TimeSpan(1, 2, 3),
					RunOnVaultStartup = false
				},
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExCorrectly_RunOnVaultStartupEqualsTrue()
		{
			string json = @"{ ""Interval"": ""01:02:03"",  ""RunOnVaultStartup"": true}";
			Frequency output = JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpan(1, 2, 3),
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check if <see cref="FrequencyJsonConverter"/> can deserialize a TimeSpan
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpan()
		{
			TimeSpan timespan = new TimeSpan(1, 2, 3);
			string json = JsonConvert.SerializeObject(timespan);

			var output = JsonConvert.DeserializeObject<Frequency>(json);

			Frequency expected = new Frequency()
			{
				Interval = new TimeSpan(1, 2, 3),
				RecurrenceType = RecurrenceType.Interval
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
			//output.ShouldBeEquivalentTo(expected);
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesFrequencyScheduleCorrectly_Daily()
		{
			string json = "{\"RecurrenceType\":2,\"Schedule\":{\"Triggers\":[{\"Type\":1,\"DailyTriggerConfiguration\":{\"TriggerTimes\":[\"02:03:04\"]}}]}}";
			Frequency output = JsonConvert.DeserializeObject<Frequency>(json);

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

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check that deserialisation works with only basic information.
		/// </summary>
		[TestMethod]
		public void DeserializesScheduleCorrectly_Daily()
		{
			string json = "{\"Triggers\":[{\"Type\":1,\"DailyTriggerConfiguration\":{\"TriggerTimes\":[\"02:03:04\"]}}]}";
			Frequency output = JsonConvert.DeserializeObject<Frequency>(json);

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

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
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

			string json = JsonConvert.SerializeObject(schedule);
			var output = JsonConvert.DeserializeObject<Frequency>(json);
			var expected = new Frequency()
			{
				Schedule = schedule,
				RecurrenceType = RecurrenceType.Schedule,
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
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

			string json = JsonConvert.SerializeObject(oldWrapper);

			var output = JsonConvert.DeserializeObject<FrequencyWrapper>(json);
			var expected = new FrequencyWrapper()
			{
				Frequency = new Frequency()
				{
					Interval = new TimeSpan(1, 2, 3),
					RecurrenceType = RecurrenceType.Interval
				}
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}

		/// <summary>
		/// Check if <see cref="FrequencyJsonConverter"/> can deserialize a TimeSpanEx contained within a wrapper class
		/// </summary>
		[TestMethod]
		public void DeserializesTimeSpanExWrapper()
		{
			var timeSpanEx = new TimeSpanEx()
			{
				Interval = new TimeSpan(1, 2, 3),
				RunOnVaultStartup = false
			};
			var oldWrapper = new TimeSpanExWrapper()
			{
				Frequency = timeSpanEx
			};

			string json = JsonConvert.SerializeObject(oldWrapper);

			var output = JsonConvert.DeserializeObject<FrequencyWrapper>(json);
			var expected = new FrequencyWrapper()
			{
				Frequency = new Frequency()
				{
					Interval = timeSpanEx,
					RecurrenceType = RecurrenceType.Interval
				}
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
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

			string json = JsonConvert.SerializeObject(oldWrapper);

			var output = JsonConvert.DeserializeObject<FrequencyWrapper>(json);
			var expected = new FrequencyWrapper()
			{
				Frequency = new Frequency()
				{
					Schedule = schedule,
					RecurrenceType = RecurrenceType.Schedule
				}
			};

			Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(output));
		}
	}
}
