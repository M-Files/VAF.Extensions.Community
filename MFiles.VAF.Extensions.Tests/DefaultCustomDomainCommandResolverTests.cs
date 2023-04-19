using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests
{
	[TestClass]
	public class DefaultCustomDomainCommandResolverTests
	{
		[TestMethod]
		public void NoIncludedTypesReturnsEmptyCollection()
		{
			var resolver = new DefaultCustomDomainCommandResolver();
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(false, commands.Any());
		}

		public class ValidInstanceMethod
		{
			[CustomCommand("hello world")]
			public virtual void Method(IConfigurationRequestContext context, ClientOperations operations){}
		}

		[TestMethod]
		public void ValidInstanceMethod_ReturnsValidData()
		{
			var resolver = new DefaultCustomDomainCommandResolver();
			resolver.Include(new ValidInstanceMethod());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.DefaultCustomDomainCommandResolverTests+ValidInstanceMethod.Method", command.ID);
			Assert.AreEqual(default, command.Blocking);
			Assert.AreEqual(default, command.ConfirmMessage);
			Assert.AreEqual(default, command.HelpText);
			Assert.IsNotNull(command.Locations);
			Assert.AreEqual(false, command.Locations.Any());

		}

		public class ValidInstanceMethodAndAdditionalData
		{
			[CustomCommand("hello world", Blocking = true, ConfirmMessage = "abc", HelpText ="def")]
			[ButtonBarCommandLocation(Priority = 1)]
			[DomainMenuCommandLocation]
			[ConfigurationMenuCommandLocation]
			public virtual void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		[TestMethod]
		public void ValidInstanceMethodAndAdditionalData_ReturnsValidData()
		{
			var resolver = new DefaultCustomDomainCommandResolver();
			resolver.Include(new ValidInstanceMethodAndAdditionalData());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.DefaultCustomDomainCommandResolverTests+ValidInstanceMethodAndAdditionalData.Method", command.ID);
			Assert.AreEqual(true, command.Blocking);
			Assert.AreEqual("abc", command.ConfirmMessage);
			Assert.AreEqual("def", command.HelpText);
			Assert.IsNotNull(command.Locations);
			Assert.AreEqual(3, command.Locations.Count());
			Assert.AreEqual(1, command.Locations.Count(l => l is ButtonBarCommandLocation));
			Assert.AreEqual(1, command.Locations.Count(l => l is DomainMenuCommandLocation));
			Assert.AreEqual(1, command.Locations.Count(l => l is ConfigurationMenuCommandLocation));

		}

		public class ValidStaticMethod
		{
			[CustomCommand("hello world")]
			public virtual void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		[TestMethod]
		public void ValidStaticMethod_ReturnsValidData()
		{
			var resolver = new DefaultCustomDomainCommandResolver();
			resolver.Include(new ValidStaticMethod());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.DefaultCustomDomainCommandResolverTests+ValidStaticMethod.Method", command.ID);
			Assert.AreEqual(default, command.Blocking);
			Assert.AreEqual(default, command.ConfirmMessage);
			Assert.AreEqual(default, command.HelpText);
			Assert.IsNotNull(command.Locations);
			Assert.AreEqual(false, command.Locations.Any());

		}

		//public class ValidInstanceMethod_Overridden
		//	: ValidInstanceMethod
		//{
		//	public override void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		//}

		//[TestMethod]
		//public void ValidInstanceMethod_Overridden_ReturnsValidData()
		//{
		//	var resolver = new DefaultCustomDomainCommandResolver();
		//	resolver.Include(new ValidInstanceMethod_Overridden());
		//	var commands = resolver.GetCustomDomainCommands();
		//	Assert.IsNotNull(commands);
		//	Assert.AreEqual(1, commands.Count());

		//	var command = commands.ElementAt(0);
		//	Assert.IsNotNull(command);
		//	Assert.AreEqual("hello world", command.DisplayName);
		//	Assert.AreEqual("MFiles.VAF.Extensions.Tests.DefaultCustomDomainCommandResolverTests+ValidInstanceMethod.Method", command.ID);
		//	Assert.AreEqual(default, command.Blocking);
		//	Assert.AreEqual(default, command.ConfirmMessage);
		//	Assert.AreEqual(default, command.HelpText);
		//	Assert.IsNotNull(command.Locations);
		//	Assert.AreEqual(false, command.Locations.Any());

		//}
	}
}
