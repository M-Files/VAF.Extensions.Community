using MFiles.VAF.Extensions.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.Email
{
	[TestClass]
	[DataMemberRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.DefaultSender),
		nameof(Extensions.Email.VAFSmtpConfiguration.UseLocalPickupFolder),
		nameof(Extensions.Email.VAFSmtpConfiguration.LocalPickupFolder),
		nameof(Extensions.Email.VAFSmtpConfiguration.ServerAddress),
		nameof(Extensions.Email.VAFSmtpConfiguration.Port),
		nameof(Extensions.Email.VAFSmtpConfiguration.UseEncryptedConnection),
		nameof(Extensions.Email.VAFSmtpConfiguration.Credentials)
	)]
	[JsonConfEditorRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.UseLocalPickupFolder),
		defaultValue: false
	)]
	[JsonConfEditorRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.LocalPickupFolder),
		defaultValue: null,
		hidden: true,
		showWhen: ".parent._children{.key == 'UseLocalPickupFolder' && .value == true }"
	)]
	[JsonConfEditorRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.ServerAddress),
		defaultValue: null,
		hidden: true,
		showWhen: ".parent._children{.key == 'UseLocalPickupFolder' && .value == false }"
	)]
	[JsonConfEditorRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.Port),
		defaultValue: MFilesAPI.Extensions.Email.SmtpConfiguration.DefaultPort,
		hidden: true
	)]
	[JsonConfEditorRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.UseEncryptedConnection),
		defaultValue: true,
		hidden: true,
		showWhen: ".parent._children{.key == 'UseLocalPickupFolder' && .value == false }"
	)]
	[JsonConfEditorRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.RequiresAuthentication),
		defaultValue: true,
		hidden: true,
		showWhen: ".parent._children{.key == 'UseLocalPickupFolder' && .value == false }"
	)]
	[JsonConfEditorRequired
	(
		nameof(Extensions.Email.VAFSmtpConfiguration.Credentials),
		defaultValue: null,
		hidden: true,
		showWhen: ".parent._children{.key == 'RequiresAuthentication' && .value == true }"
	)]
	public class VAFSmtpConfiguration
		: ConfigurationClassTestBase<Extensions.Email.VAFSmtpConfiguration>
	{
	}
}
