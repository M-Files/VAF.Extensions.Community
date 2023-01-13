using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.NLog.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	[TestClass]
	public class DeserializationAndSerialization
	{
		[TestMethod]
		public void MinimumLogLevel()
		{
			var loggingConfiguration = JsonConvert.DeserializeObject<NLogLoggingConfiguration>(@"
			{
				""Enabled"": true,

				""DefaultTargetConfiguration"": {
					""Name"": ""Default target"",
					""Enabled"": true
				},
				""FileTargetConfigurations"": [
					{
						""Name"": ""File target"",
						""Enabled"": true,
						""FolderName"": ""C:\\mfiles_logs"",
						""MinimumLogLevel"": ""Trace""
					}
				]
			}

			");

			var serializedLoggingConfiguration = JsonConvert.SerializeObject(loggingConfiguration, new NewtonsoftJsonConvert().JsonSerializerSettings);
			var newLoggingConfiguration = JsonConvert.DeserializeObject<NLogLoggingConfiguration>(serializedLoggingConfiguration);


			Assert.AreEqual(MFiles.VAF.Configuration.Logging.LogLevel.Trace, newLoggingConfiguration.FileTargetConfigurations.First().MinimumLogLevel, "Logging level was not retained during serialization.");

		}
	}
}
