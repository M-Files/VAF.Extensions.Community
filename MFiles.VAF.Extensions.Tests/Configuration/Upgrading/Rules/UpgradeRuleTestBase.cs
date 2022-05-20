using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFilesAPI;
using Moq;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	public abstract class UpgradeRuleTestBase
	{
		protected const MFNamedValueType DefaultSourceNVSType = MFNamedValueType.MFConfigurationValue;
		protected const string DefaultSourceNamespace = "Source.Namespace";

		protected const MFNamedValueType DefaultTargetNVSType = MFNamedValueType.MFSystemAdminConfiguration;
		protected const string DefaultTargetNamespace = "Target.Namespace";

		public Mock<ISingleNamedValueItem> CreateSingleNamedValueItemMock
		(
			bool isValid,
			MFNamedValueType namedValueType = DefaultSourceNVSType,
			string @namespace = DefaultSourceNamespace,
			string name = "config"
		)
		{
			var mock = new Mock<ISingleNamedValueItem>();
			mock.SetupAllProperties();
			mock.Setup(m => m.IsValid()).Returns(isValid);
			mock.Setup(m => m.GetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>()))
				.Returns((INamedValueStorageManager manager, Vault vault) =>
				{
					return manager?.GetNamedValues(vault, namedValueType, @namespace);
				});
			mock.Setup(m => m.RemoveNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), It.IsAny<string[]>()))
				.Callback((INamedValueStorageManager manager, Vault vault, string[] names) =>
				{
					manager?.RemoveNamedValues(vault, namedValueType, @namespace, names);
				});
			mock.Setup(m => m.SetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), It.IsAny<NamedValues>()))
				.Callback((INamedValueStorageManager manager, Vault vault, NamedValues nv) =>
				{
					manager?.SetNamedValues(vault, namedValueType, @namespace, nv);
				});
			mock.Object.NamedValueType = namedValueType;
			mock.Object.Namespace = @namespace;
			mock.Object.Name = name;
			return mock;
		}
	}
}
