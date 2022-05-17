using MFiles.VAF.Core;
using MFiles.VAF;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using MFiles.VAF.Configuration;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Tests
{

	[TestClass]
	public partial class ConfigurableVaultApplicationBaseTests
		: TestBaseWithVaultMock
	{
		delegate void readConfigurationDataCallback(Vault v, string @namespace, string @name, out string value);

		protected IConfigurationStorage GetConfigurationStorage<TConfigurationType>(TConfigurationType configuration)
			=> GetConfigurationStorage(configuration == null ? (string)null : Newtonsoft.Json.JsonConvert.SerializeObject(configuration));

		protected IConfigurationStorage GetConfigurationStorage(string configurationData = null)
		{
			var storage = new Mock<IConfigurationStorage>();
			storage
				.Setup(m => m.ReadConfigurationData(It.IsAny<Vault>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(configurationData);
			storage
				.Setup(m => m.ReadConfigurationData(It.IsAny<Vault>(), It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string>.IsAny))
				.Callback(new readConfigurationDataCallback((Vault v, string @namespace, string @name, out string value) =>
				{
					value = configurationData;
				}))
				.Returns(configurationData != null);

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

	public class ConfigurableVaultApplicationBaseProxy<TSecureConfigurationType>
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<TSecureConfigurationType>
		where TSecureConfigurationType : class, new()
	{
		public new Vault PermanentVault
		{
			get => base.PermanentVault;
			set => base.PermanentVault = value;
		}
		public new VAF.Configuration.IConfigurationStorage ConfigurationStorage
		{
			get => base.ConfigurationStorage;
			set => base.ConfigurationStorage = value;
		}
	}
	public class ConfigurableVaultApplicationBaseProxy
		: ConfigurableVaultApplicationBaseProxy<object>
	{
	}
}
