using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Logging
{
	[TestClass]
	public class ConfigurationTests
	{
		[TestMethod]
		public void Serialization()
		{
			var configuration = new ConfigurationProxy();
			var newString = Newtonsoft.Json.JsonConvert.SerializeObject
			(
				configuration,
				Newtonsoft.Json.Formatting.None,
				NewtonsoftJsonConvert.DefaultJsonSerializerSettings
			);
			Assert.AreEqual("{}", newString);
		}
		[TestMethod]
		public void Serialization_WithVersion()
		{
			var configuration = new ConfigurationProxy() { Version = new Version("1.0" )};
			var newString = Newtonsoft.Json.JsonConvert.SerializeObject
			(
				configuration,
				Newtonsoft.Json.Formatting.None,
				NewtonsoftJsonConvert.DefaultJsonSerializerSettings
			);
			Assert.AreEqual("{\"Version\":\"1.0\"}", newString);
		}
		public class ConfigurationProxy
			: MFiles.VAF.Extensions.Configuration.ConfigurationBase
		{

		}
	}
}
