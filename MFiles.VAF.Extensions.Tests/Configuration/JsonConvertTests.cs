using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.NLog.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	[TestClass]
	public class JsonConvertTests
	{
		[TestMethod]
		public void SerializesMinimumLogLevelCorrectly()
		{
			var jsonConvert = new NewtonsoftJsonConvert();
			var loggingConfiguration = jsonConvert.Deserialize<NLogLoggingConfiguration>(
				@"
					{
						""FileTargetConfigurations"": [
							{
								""MinimumLogLevel"": ""Trace""
							}
						]
					}"
			);

			Assert.AreEqual
			(
				LogLevel.Trace,
				loggingConfiguration.FileTargetConfigurations.First().MinimumLogLevel,
				"LogLevel default value 'Trace' was not retained for MinimumLogLevel during deserialization."
			);

			// Parse the serialised data into a JObject.
			// This allows us to test what was actually serialised, and ignore anything that
			// was populated/created during the serialisation process.
			var jObject = JObject.Parse(jsonConvert.Serialize(loggingConfiguration));

			Assert.AreEqual
			(
				LogLevel.Trace,
				Enum.Parse(typeof(LogLevel), ((JArray)jObject["FileTargetConfigurations"])[0].Value<string>("MinimumLogLevel")),
				"LogLevel default value 'Trace' was not retained for MinimumLogLevel during serialization."
			);
		}
	}
}
