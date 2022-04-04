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

		public Mock<ISourceNamedValueItem> CreateSourceNamedValueItemMock
		(
			bool isValid,
			MFNamedValueType sourceNVSType = DefaultSourceNVSType,
			string sourceNamespace = DefaultSourceNamespace
		)
		{
			var mock = new Mock<ISourceNamedValueItem>();
			mock.Setup(m => m.IsValid()).Returns(isValid);
			mock.Setup(m => m.GetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>()))
				.Returns((INamedValueStorageManager manager, Vault vault) =>
				{
					return manager?.GetNamedValues(vault, sourceNVSType, sourceNamespace);
				});
			mock.Setup(m => m.RemoveNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), It.IsAny<string[]>()))
				.Callback((INamedValueStorageManager manager, Vault vault, string[] names) =>
				{
					manager?.RemoveNamedValues(vault, sourceNVSType, sourceNamespace, names);
				});
			return mock;
		}
		public Mock<ITargetNamedValueItem> CreateTargetNamedValueItemMock
		(
			bool isValid,
			MFNamedValueType targetNVSType = DefaultTargetNVSType,
			string targetNamespace = DefaultTargetNamespace
		)
		{
			var mock = new Mock<ITargetNamedValueItem>();
			mock.Setup(m => m.IsValid()).Returns(isValid);
			mock.Setup(m => m.SetNamedValues(It.IsAny<INamedValueStorageManager>(), It.IsAny<Vault>(), It.IsAny<NamedValues>()))
				.Callback((INamedValueStorageManager manager, Vault vault, NamedValues nv) =>
				{
					manager?.SetNamedValues(vault, targetNVSType, targetNamespace, nv);
				});
			return mock;
		}
	}
}
