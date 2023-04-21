using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Configuration.Interfaces.Domain;
using MFiles.VAF.Extensions.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Declares that the associated method should be exposed via a command,
	/// and run when the command is executed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class CustomCommandAttribute
		: Attribute
	{
		/// <inheritdoc cref="CustomDomainCommand.ID"/>
		public string CommandId { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.Locations"/>
		public List<ICommandLocation> Locations { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.Label"/>
		public string Label { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.HelpText"/>
		public string HelpText { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.ConfirmMessage"/>
		public string ConfirmMessage { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.Blocking"/>
		public bool Blocking { get; set; }

		/// <summary>
		/// <see langword="true"/> if <see cref="Configure"/> is successfully called
		/// (and hence this object can be converted to a custom domain command), 
		/// <see langword="false"/> otherwise.
		/// </summary>
		public bool IsConfigured { get; protected set; }

		/// <inheritdoc cref="CustomDomainCommand.Execute"/>
		public Action<IConfigurationRequestContext, ClientOperations> Execute { get; protected set; }

		public CustomCommandAttribute(string label)
		{
			this.Label = label;
		}

		/// <summary>
		/// Configures the command for use.
		/// Called typically by <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.GetCommands(IConfigurationRequestContext)"/>
		/// once usages of this attribute have been identified.
		/// </summary>
		/// <param name="execute">The method to execute when the command is clicked.</param>
		/// <param name="commandId">The Id of the command.  Must be unique in this application.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
		public virtual void Configure
		(
			Action<IConfigurationRequestContext, ClientOperations> execute,
			ICommandLocation[] locations = null,
			string commandId = null
		)
		{
			this.Execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.CommandId = commandId ?? throw new ArgumentNullException(nameof(commandId));
			this.Locations = new List<ICommandLocation>(locations ?? Enumerable.Empty<ICommandLocation>());
			this.IsConfigured = true;
		}

		/// <summary>
		/// Configures the command for use.
		/// Called typically by <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.GetCommands(IConfigurationRequestContext)"/>
		/// once usages of this attribute have been identified.
		/// </summary>
		/// <param name="method">The method to execute.</param>
		/// <param name="instance">The instance that declared the method.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="method"/> is null.</exception>
		/// <remarks>
		/// If <paramref name="instance"/> is null then <paramref name="method"/> must point to a static method.
		/// If <paramref name="instance"/> is not null then either it must be of a type that can be cast to the type that declared <paramref name="method"/>, or <paramref name="method"/> must point to a static method.
		/// </remarks>
		public virtual void Configure(MethodInfo method, object instance = null)
		{
			// Sanity.
			if (null == method)
				throw new ArgumentNullException(nameof(method));

			// Validate the method signature.
			{
				var invalidSignatureString = $"The method signature must be `void {method.Name}(IConfigurationRequestContext c, ClientOperations o)`";
				if (typeof(void) != method.ReturnType)
					throw new ArgumentException(invalidSignatureString, nameof(method));
				if(method.ContainsGenericParameters)
					throw new ArgumentException(invalidSignatureString, nameof(method));
				var parameters = method.GetParameters();
				if(parameters.Length != 2)
					throw new ArgumentException(invalidSignatureString, nameof(method));
				if (parameters[0].ParameterType != typeof(IConfigurationRequestContext))
					throw new ArgumentException(invalidSignatureString, nameof(method));
				if (parameters[1].ParameterType != typeof(ClientOperations))
					throw new ArgumentException(invalidSignatureString, nameof(method));
			}

			// Validate the instance.
			if(null == instance)
			{
				// If the instance is null then the method must be static.
				if (!method.IsStatic)
					throw new ArgumentException("The method must be static if an instance is not provided.", nameof(method));
			}
			else
			{
				// If the instance is not null then the instance type must be valid OR the method must be static.
				if (!method.IsStatic && !instance.GetType().IsAssignableFrom(method.DeclaringType))
					throw new ArgumentException($"The instance type ('{instance.GetType().FullName}') must be assignable to the declaring type ({method.DeclaringType.FullName}).", nameof(method));
			}

			// Find the locations.
			var locations = new List<ICommandLocation>();
			{
				var locationAttributes = method
					.GetCustomAttributes<CommandLocationAttribute>()
					?? Enumerable.Empty<CommandLocationAttribute>();
				foreach(var a in locationAttributes.Where(l => l != null))
				{
					locations.Add(a.ToCommandLocation());
				}
			}

			// Get the command Id and use the other overload.
			var commandId = string.IsNullOrWhiteSpace(this.CommandId)
				? this.GetDefaultCommandId(method)
				: this.CommandId;

			// Set us all up.
			this.Configure
			(
				// Invoke the method when executed.
				(c, o) =>
				{
					method.Invoke
					(
						method.IsStatic ? null : instance,
						new object[] { c, o }
					);
				},
				locations.ToArray(),
				commandId
			);

		}

		/// <summary>
		/// Returns the command ID for the provided <paramref name="method"/>.
		/// </summary>
		/// <param name="method">The method that will be run by the command.</param>
		/// <returns>The command ID.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="method"/> is <see langword="null"/>.</exception>
		public virtual string GetDefaultCommandId(MethodInfo method)
		{
			if(null == method)
				throw new ArgumentNullException(nameof(method));
			return $"{method.DeclaringType.FullName}.{method.Name}";
		}

		/// <summary>
		/// Converts <paramref name="attribute"/> to a <see cref="CustomDomainCommand"/>.
		/// </summary>
		/// <param name="attribute">The attribute to convert.</param>
		public static explicit operator CustomDomainCommandEx(CustomCommandAttribute attribute)
		{
			// Sanity.
			if(null == attribute)
				throw new ArgumentNullException(nameof(attribute));
			if (!attribute.IsConfigured)
				throw new InvalidOperationException("You must call CustomCommandAttribute.Configure prior to casting to a CustomDomainCommand.");

			// Convert all the data we have to something usable.
			return new CustomDomainCommandEx()
			{
				Execute = attribute.Execute,
				ID = attribute.CommandId,
				Blocking = attribute.Blocking,
				ConfirmMessage = attribute.ConfirmMessage,
				DisplayName = attribute.Label,
				HelpText = attribute.HelpText,
				Locations = attribute.Locations ?? new List<ICommandLocation>(),
				CustomCommandAttribute = attribute
			};
		}

		/// <summary>
		/// Converts this attribute to a <see cref="CustomDomainCommandEx"/>.
		/// </summary>
		/// <param name="method">The method to execute.</param>
		/// <param name="instance">The instance that declared the method.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="method"/> is null.</exception>
		/// <returns>The domain command.</returns>
		/// <remarks>
		/// If <paramref name="instance"/> is null then <paramref name="method"/> must point to a static method.
		/// If <paramref name="instance"/> is not null then either it must be of a type that can be cast to the type that declared <paramref name="method"/>, or <paramref name="method"/> must point to a static method.
		/// </remarks>
		public virtual CustomDomainCommandEx ToCustomDomainCommand(MethodInfo method, object instance = null)
		{
			// Configure it.
			this.Configure(method, instance);
			return (CustomDomainCommandEx)this;
		}

		/// <summary>
		/// Converts this attribute to a <see cref="CustomDomainCommand"/>.
		/// </summary>
		/// <returns>The domain command.</returns>
		public virtual CustomDomainCommandEx ToCustomDomainCommand()
		{
			return (CustomDomainCommandEx) this;
		}
	}
	
	/// <summary>
	/// An extension of <see cref="CustomDomainCommand"/> that allows
	/// reference to the <see cref="CustomCommandAttribute"/> that declared it.
	/// </summary>
	public class CustomDomainCommandEx
		: CustomDomainCommand
	{
		/// <summary>
		/// The attribute that declared this custom domain command.
		/// </summary>
		public CustomCommandAttribute CustomCommandAttribute { get; set; }
	}
}
