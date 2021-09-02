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
			var output = JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : ""11:01:02"" }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.AreEqual(new TimeSpan(11, 01, 02), output.TimeSpanEx.Interval);
		}

		[TestMethod]
		public void DeserializesTimeSpanExString()
		{
			var output = JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : { ""Interval"" : ""01:02:03"" } }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.AreEqual(new TimeSpan(01, 02, 03), output.TimeSpanEx.Interval);
		}

		[TestMethod]
		public void DeserializesTimeSpanExWithRunOnVaultStartup_True()
		{
			var output = JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : { ""Interval"" : ""01:02:03"", ""RunOnVaultStartup"" : true } }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.IsTrue(output.TimeSpanEx.RunOnVaultStartup);
		}

		[TestMethod]
		public void DeserializesTimeSpanExWithRunOnVaultStartup_False()
		{
			var output = JsonConvert.DeserializeObject<Wrapper>(@"{ ""TimeSpanEx"" : { ""Interval"" : ""01:02:03"", ""RunOnVaultStartup"" : false } }");
			Assert.IsNotNull(output?.TimeSpanEx);
			Assert.IsFalse(output.TimeSpanEx.RunOnVaultStartup);
		}

		[TestMethod]
		public void SerializesCorrectly()
		{
			var input = new TimeSpanEx() { Interval = new TimeSpan(1, 2, 3) };
			Assert.AreEqual(@"{""Interval"":""01:02:03"",""RunOnVaultStartup"":true}", JsonConvert.SerializeObject(input));
		}

		[TestMethod]
		public void SerializesCorrectlyWithRunOnVaultStartup_False()
		{
			var input = new TimeSpanEx() { Interval = new TimeSpan(1, 2, 3), RunOnVaultStartup = false };
			Assert.AreEqual(@"{""Interval"":""01:02:03"",""RunOnVaultStartup"":false}", JsonConvert.SerializeObject(input));
		}
	}
}
