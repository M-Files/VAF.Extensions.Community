using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.NLog.Configuration;
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
	[TestClass]
	public class EnumTests
	{
		[TestMethod]
		public void SerializesMinimumLogLevelCorrectly()
		{
			var loggingConfiguration = JsonConvert.DeserializeObject<NLogLoggingConfiguration>(
				@"
					{
						""FileTargetConfigurations"": [
							{
								""MinimumLogLevel"": ""Trace""
							}
						]
					}"
			);

			Assert.AreEqual(MFiles.VAF.Configuration.Logging.LogLevel.Trace, loggingConfiguration.FileTargetConfigurations.First().MinimumLogLevel, "LogLevel default value 'Trace' was not retained for MinimumLogLevel during deserialization.");

			var serializedLoggingConfiguration = JsonConvert.SerializeObject(loggingConfiguration, new NewtonsoftJsonConvert().JsonSerializerSettings);
			var newLoggingConfiguration = JsonConvert.DeserializeObject<NLogLoggingConfiguration>(serializedLoggingConfiguration);

			Assert.AreEqual(MFiles.VAF.Configuration.Logging.LogLevel.Trace, newLoggingConfiguration.FileTargetConfigurations.First().MinimumLogLevel, "LogLevel default value 'Trace' was not retained for MinimumLogLevel during serialization.");
		}
	}
}
