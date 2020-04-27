using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.Serialization;

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
		: ConfigurationClassTestBase
	{
		public override Type GetClassBeingTested()
			=> typeof(Extensions.Email.Credentials);
	}
}
