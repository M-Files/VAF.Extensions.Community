using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	[DataContract]
	internal class Wrapper
	{
		[DataMember]
		public TimeSpanEx TimeSpanEx { get; set; }
	}
	[TestClass]
	public class TimeSpanExTests
	{
		[TestMethod]
		public void DeserializesTimeSpanString()
		{
			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : ""11:01:02"" }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.AreEqual(new TimeSpan(11, 01, 02), output.TimeSpanEx.GetInterval());
		}

		[TestMethod]
		public void DeserializesTimeSpanExString()
		{
			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : { ""Interval"" : ""01:02:03"" } }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.AreEqual(new TimeSpan(01, 02, 03), output.TimeSpanEx.GetInterval());
		}

		[TestMethod]
		public void DeserializesTimeSpanExWithRunOnVaultStartup_True()
		{
			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : { ""Interval"" : ""01:02:03"", ""RunOnVaultStartup"" : true } }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.IsTrue(output.TimeSpanEx.RunOnVaultStartup.HasValue);
			Assert.IsTrue(output.TimeSpanEx.RunOnVaultStartup.Value);
		}

		[TestMethod]
		public void DeserializesTimeSpanExWithRunOnVaultStartup_False()
		{
			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : { ""Interval"" : ""01:02:03"", ""RunOnVaultStartup"" : false } }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.IsTrue(output.TimeSpanEx.RunOnVaultStartup.HasValue);
			Assert.IsFalse(output.TimeSpanEx.RunOnVaultStartup.Value);
		}

		[TestMethod]
		public void DeserializesSeparateIntervalMembersString()
		{
			var output = Newtonsoft.Json.JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : { ""Hours"" : 1, ""Minutes"" : 2, ""Seconds"" : 3, ""RunOnVaultStartup"" : false } }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.AreEqual(new TimeSpan(01, 02, 03), output.TimeSpanEx.GetInterval());
		}

		[TestMethod]
		public void SerializesCorrectly()
		{
			var input = new TimeSpanEx(new TimeSpan(1, 2, 3));
			// Note: we force RunOnVaultStartup to true for TimeSpanEx/TimeSpan, for legacy compatibility.
			Assert.AreEqual(@"{""Hours"":1,""Minutes"":2,""Seconds"":3,""RunOnVaultStartup"":true}", Newtonsoft.Json.JsonConvert.SerializeObject(input));
		}

		[TestMethod]
		public void SerializesCorrectlyWithRunOnVaultStartup_Null()
		{
			var input = new TimeSpanEx(new TimeSpan(1, 2, 3)) { RunOnVaultStartup = null };
			Assert.AreEqual(@"{""Hours"":1,""Minutes"":2,""Seconds"":3}", Newtonsoft.Json.JsonConvert.SerializeObject(input));
		}

		[TestMethod]
		public void SerializesCorrectlyWithRunOnVaultStartup_True()
		{
			var input = new TimeSpanEx(new TimeSpan(1, 2, 3)) { RunOnVaultStartup = true };
			Assert.AreEqual(@"{""Hours"":1,""Minutes"":2,""Seconds"":3,""RunOnVaultStartup"":true}", Newtonsoft.Json.JsonConvert.SerializeObject(input));
		}

		[TestMethod]
		public void SerializesCorrectlyWithRunOnVaultStartup_False()
		{
			var input = new TimeSpanEx(new TimeSpan(1, 2, 3)) { RunOnVaultStartup = false };
			Assert.AreEqual(@"{""Hours"":1,""Minutes"":2,""Seconds"":3,""RunOnVaultStartup"":false}", Newtonsoft.Json.JsonConvert.SerializeObject(input));
		}
	}
}
