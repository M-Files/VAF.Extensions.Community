using MFiles.VAF.Extensions.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.Email
{
	[TestClass]
	[DataMemberRequired
	(
		nameof(Extensions.Email.Credentials.AccountName),
		nameof(Extensions.Email.Credentials.Password)
	)]
	[SecurityRequired
	(
		nameof(Extensions.Email.Credentials.Password),
		true
	)]
	public class Credentials
		: ConfigurationClassTestBase<Extensions.Email.Credentials>
	{
	}
}
