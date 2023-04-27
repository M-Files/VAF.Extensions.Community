using MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution.AttributeCustomDomainCommandResolverTests;

namespace MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution
{
	[TestClass]
	public class DefaultCustomDomainCommandResolverTests
	{

		[TestMethod]
		public void GetDefaultCustomDomainCommandResolvers_ContainsAsynchronousCustomDomainCommandResolver()
		{
			var resolver = new DefaultCustomDomainCommandResolver<object>
			(
				Mock.Of<ConfigurableVaultApplicationBase<object>>()
			);

			var internalResolvers = resolver.GetDefaultCustomDomainCommandResolvers();
			Assert.IsNotNull(internalResolvers);
			Assert.IsTrue(internalResolvers.Any(r => r is AsynchronousOperationCustomDomainCommandResolver<object>));

		}

		[TestMethod]
		public void GetDefaultCustomDomainCommandResolvers_ContainsAttributeCustomDomainCommandResolver()
		{
			var resolver = new DefaultCustomDomainCommandResolver<object>
			(
				Mock.Of<ConfigurableVaultApplicationBase<object>>()
			);

			var internalResolvers = resolver.GetDefaultCustomDomainCommandResolvers();
			Assert.IsNotNull(internalResolvers);
			Assert.IsTrue(internalResolvers.Any(r => r is AttributeCustomDomainCommandResolver));

		}

		[TestMethod]
		public void GetDefaultCustomDomainCommandResolvers_ContainsLogCustomDomainCommandResolver()
		{
			var resolver = new DefaultCustomDomainCommandResolver<object>
			(
				Mock.Of<ConfigurableVaultApplicationBase<object>>()
			);

			var internalResolvers = resolver.GetDefaultCustomDomainCommandResolvers();
			Assert.IsNotNull(internalResolvers);
			Assert.IsTrue(internalResolvers.Any(r => r is LogCustomDomainCommandResolver<object>));

		}
	}
}
