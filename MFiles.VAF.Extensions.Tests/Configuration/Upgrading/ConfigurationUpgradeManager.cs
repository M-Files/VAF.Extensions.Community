using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	[TestClass]
	public partial class ConfigurationUpgradeManager
		: TestBaseWithVaultMock
	{
		delegate void readConfigurationDataCallback(Vault v, string @namespace, string @name, out string value);

		protected IConfigurationStorage GetConfigurationStorage<TConfigurationType>(TConfigurationType configuration)
			=> GetConfigurationStorage(configuration == null ? (string)null : Newtonsoft.Json.JsonConvert.SerializeObject(configuration));

		protected IConfigurationStorage GetConfigurationStorage(string configurationData = null)
		{
			var storage = new Mock<IConfigurationStorage>();

			// Reading of data.
			storage
				.Setup(m => m.ReadConfigurationData(It.IsAny<Vault>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(() => configurationData);
			storage
				.Setup(m => m.ReadConfigurationData(It.IsAny<Vault>(), It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string>.IsAny))
				.Callback(new readConfigurationDataCallback((Vault v, string @namespace, string @name, out string value) =>
				{
					value = configurationData;
				}))
				.Returns(configurationData != null);

			// Writing of data.
			storage
				.Setup(m => m.SaveConfigurationData(It.IsAny<Vault>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback((Vault v, string @namespace, string @data, string @key) =>
				{
					configurationData = @data;
				});

			// Deserialisation.
			// Note: this isn't a true mock, but we'll assume that Newtonsoft is capable of doing these actions as it makes the method more flexible.
			storage
				.Setup(m => m.Deserialize(It.IsAny<Type>(), It.IsAny<string>()))
				.Returns((Type t, string s) => Newtonsoft.Json.JsonConvert.DeserializeObject(s, t));
			storage
				.Setup(m => m.Deserialize<It.IsAnyType>(It.IsAny<string>()))
				.Returns
				(
					// The generic method is a bit awkward to mock...
					new InvocationFunc((invocation) =>
					{
						return Newtonsoft.Json.JsonConvert.DeserializeObject(invocation.Arguments[0] as string, invocation.Method.GetGenericArguments()[0]);
					})
				);

			// Serialisation.
			storage
				.Setup(m => m.Serialize(It.IsAny<object>()))
				.Returns((object o) => Newtonsoft.Json.JsonConvert.SerializeObject(o));

			//Okay.
			return storage.Object;
		}

	}
}
