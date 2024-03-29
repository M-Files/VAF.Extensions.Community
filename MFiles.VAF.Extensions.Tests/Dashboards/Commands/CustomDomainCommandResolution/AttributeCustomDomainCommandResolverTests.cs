﻿using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Extensions.Dashboards.Commands;
using MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution
{
	[TestClass]
	public class AttributeCustomDomainCommandResolverTests
	{
		/// <summary>
		/// By default there should be no commands returned.
		/// </summary>
		[TestMethod]
		public void NoIncludedTypesReturnsEmptyCollection()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(false, commands.Any());
		}

		public class InvalidInstanceMethod
		{
			[CustomCommand("hello world")]
			public virtual bool Method(IConfigurationRequestContext context, ClientOperations operations)
				=> false;
		}

		/// <summary>
		/// If there's an invalid method we should log, not throw.
		/// </summary>
		[TestMethod]
		public void InvalidMethodDoesNotThrow()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new InvalidInstanceMethod());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(false, commands.Any());
		}

		public class ValidInstanceMethod
		{
			[CustomCommand("hello world")]
			public virtual void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		/// <summary>
		/// A simple instance method with CustomCommandAttribute.
		/// </summary>
		[TestMethod]
		public void ValidInstanceMethod_ReturnsValidData()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new ValidInstanceMethod());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution.AttributeCustomDomainCommandResolverTests+ValidInstanceMethod.Method", command.ID);
			Assert.AreEqual(default, command.Blocking);
			Assert.AreEqual(default, command.ConfirmMessage);
			Assert.AreEqual(default, command.HelpText);
			Assert.IsNotNull(command.Locations);
			Assert.AreEqual(false, command.Locations.Any());

		}

		public class ValidInstanceMethodAndAdditionalData
		{
			[CustomCommand("hello world", Blocking = true, ConfirmMessage = "abc", HelpText = "def")]
			[ButtonBarCommandLocation(Priority = 1)]
			[DomainMenuCommandLocation]
			[ConfigurationMenuCommandLocation]
			public virtual void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		/// <summary>
		/// A simple instance method with CustomCommandAttribute and lots of optional data.
		/// </summary>
		[TestMethod]
		public void ValidInstanceMethodAndAdditionalData_ReturnsValidData()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new ValidInstanceMethodAndAdditionalData());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution.AttributeCustomDomainCommandResolverTests+ValidInstanceMethodAndAdditionalData.Method", command.ID);
			Assert.AreEqual(true, command.Blocking);
			Assert.AreEqual("abc", command.ConfirmMessage);
			Assert.AreEqual("def", command.HelpText);
			Assert.IsNotNull(command.Locations);
			Assert.AreEqual(3, command.Locations.Count());
			Assert.AreEqual(1, command.Locations.Count(l => l is ButtonBarCommandLocation));
			Assert.AreEqual(1, command.Locations.Count(l => l is DomainMenuCommandLocation));
			Assert.AreEqual(1, command.Locations.Count(l => l is ConfigurationMenuCommandLocation));

		}

		public class ValidInstanceMethodWithCustomCommandId
		{
			[CustomCommand("hello world", CommandId = "helloWorld")]
			public void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		/// <summary>
		/// A valid instance command with custom command ID is returned correctly.
		/// </summary>
		[TestMethod]
		public void ValidInstanceMethodWithCustomCommandId_ReturnsValidData()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new ValidInstanceMethodWithCustomCommandId());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("helloWorld", command.ID);

		}

		public class ValidStaticMethod
		{
			[CustomCommand("hello world")]
			public static void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		/// <summary>
		/// A simple static method with CustomCommandAttribute.
		/// </summary>
		[TestMethod]
		public void ValidStaticMethod_ReturnsValidData()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new ValidStaticMethod());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution.AttributeCustomDomainCommandResolverTests+ValidStaticMethod.Method", command.ID);
			Assert.AreEqual(default, command.Blocking);
			Assert.AreEqual(default, command.ConfirmMessage);
			Assert.AreEqual(default, command.HelpText);
			Assert.IsNotNull(command.Locations);
			Assert.AreEqual(false, command.Locations.Any());

		}

		public class ValidInstanceMethod_Overridden
			: ValidInstanceMethod
		{
			public override void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		/// <summary>
		/// The tested class inherits from a class that defined a custom domain command,
		/// but it overrides the base virtual method.
		/// Ensure that the new method is found along with the base custom command data.
		/// </summary>
		[TestMethod]
		public void ValidInstanceMethod_Overridden_ReturnsValidData()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new ValidInstanceMethod_Overridden());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);

			// NOTE: the command ID is changed by the overriding.
			// This is "expected" (as the method is now the overriding one), but is it actually expected?
			// For this reason people should not make assumptions about the command ID, but retrieve it from...  Something?
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution.AttributeCustomDomainCommandResolverTests+ValidInstanceMethod_Overridden.Method", command.ID);

			Assert.AreEqual(default, command.Blocking);
			Assert.AreEqual(default, command.ConfirmMessage);
			Assert.AreEqual(default, command.HelpText);
			Assert.IsNotNull(command.Locations);
			Assert.AreEqual(false, command.Locations.Any());

		}

		public class ValidInstanceMethod_OverriddenWithNewAttribute
			: ValidInstanceMethod
		{
			[CustomCommand("overridden label")]
			public override void Method(IConfigurationRequestContext context, ClientOperations operations) { }
		}

		/// <summary>
		/// The tested class inherits from a class that defined a custom domain command,
		/// but it overrides the base virtual method and additionally defines its own custom command.
		/// Ensure that the new method is found and the updated custom command data.
		/// </summary>
		[TestMethod]
		public void ValidInstanceMethod_OverriddenWithNewAttribute_ReturnsValidData()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new ValidInstanceMethod_OverriddenWithNewAttribute());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("overridden label", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution.AttributeCustomDomainCommandResolverTests+ValidInstanceMethod_OverriddenWithNewAttribute.Method", command.ID);

		}

		public class DerivedClass
			: ValidInstanceMethod
		{
		}

		/// <summary>
		/// The tested class inherits from a class that defined a custom domain command,
		/// but it overrides the base virtual method and additionally defines its own custom command.
		/// Ensure that the new method is found and the updated custom command data.
		/// </summary>
		[TestMethod]
		public void DerivedClass_ReturnsValidData()
		{
			var resolver = new AttributeCustomDomainCommandResolver();
			resolver.Include(new DerivedClass());
			var commands = resolver.GetCustomDomainCommands();
			Assert.IsNotNull(commands);
			Assert.AreEqual(1, commands.Count());

			var command = commands.ElementAt(0);
			Assert.IsNotNull(command);
			Assert.AreEqual("hello world", command.DisplayName);
			Assert.AreEqual("MFiles.VAF.Extensions.Tests.Dashboards.Commands.CustomDomainCommandResolution.AttributeCustomDomainCommandResolverTests+ValidInstanceMethod.Method", command.ID);

		}
	}
}
