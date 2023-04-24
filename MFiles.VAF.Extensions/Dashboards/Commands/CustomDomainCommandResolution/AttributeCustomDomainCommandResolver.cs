using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Interfaces.Domain;
using MFiles.VAF.Configuration.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using static MFiles.VAF.Configuration.ResourceEditor.ResourceEditorPlugin;

namespace MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution
{
	/// <summary>
	/// A default implementation of <see cref="ICustomDomainCommandResolver"/>
	/// which uses reflection to find methods decorated with <see cref="CustomCommandAttribute"/>.
	/// </summary>
	public class AttributeCustomDomainCommandResolver
		: CustomDomainCommandResolverBase
	{
		/// <summary>
		/// Create our logger.
		/// </summary>
		private ILogger Logger { get; }
			= LogManager.GetLogger(typeof(AttributeCustomDomainCommandResolver));

		/// <summary>
		/// The default binding flags to use when finding methods.
		/// </summary>
		public static BindingFlags DefaultBindingFlags
			= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

		/// <summary>
		/// The binding flags to use when finding methods.
		/// Defaults to <see cref="DefaultBindingFlags"/>.
		/// </summary>
		public BindingFlags BindingFlags { get; set; }
			= DefaultBindingFlags;

		/// <summary>
		/// The list of types (and instances) to scan.
		/// </summary>
		protected Dictionary<Type, object> Included { get; } = new Dictionary<Type, object>();

		/// <summary>
		/// Creates an instance of <see cref="AttributeCustomDomainCommandResolver{TSecureConfiguration}"/>
		/// and includes the provided <paramref name="vaultApplication"/> in the list of things to resolve against.
		/// </summary>
		/// <param name="vaultApplication">The vault application this is running within.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="vaultApplication"/> is <see langword="null"/>.</exception>
		public AttributeCustomDomainCommandResolver(params object[] include)
			: base()
		{
			if (null != include)
				foreach (var i in include.Where(i => i != null))
					Include(i);
		}

		/// <inheritdoc />
		public override IEnumerable<CustomDomainCommand> GetCustomDomainCommands()
		{
			// Get everything from the included data.
			return Included?
				.AsNotNull()
				.SelectMany(t => GetCustomDomainCommandsFromType(t.Key, t.Value));
		}

		/// <summary>
		/// Scans <paramref name="type"/> for methods decorated with <see cref="CustomCommandAttribute"/>.
		/// Returns custom domain commands.
		/// </summary>
		/// <param name="type">The type to look for methods in.</param>
		/// <param name="instance">The instance to use when calling the method.</param>
		/// <returns>Any and all custom domain commands.</returns>
		protected virtual IEnumerable<CustomDomainCommand> GetCustomDomainCommandsFromType(Type type, object instance = null)
		{
			// Sanity.
			if (null == type)
				yield break;

			// Find methods with the correct attribute.
			var methods = type
				.GetMethods(BindingFlags)
				?? Enumerable.Empty<MethodInfo>();
			foreach (var method in methods)
			{
				// Attempt to get the command from the method.
				var command = this.GetCustomDomainCommandFromMethod(method, instance);
				if (null != command)
					yield return command;
			}
		}

		/// <summary>
		/// If <paramref name="method"/> is decorated with <see cref="CustomCommandAttribute"/>
		/// then returns a <see cref="CustomDomainCommand"/> representing this method call.
		/// </summary>
		/// <param name="method">The method to check.</param>
		/// <param name="instance">The instance to use when calling the method.</param>
		/// <returns>The domain command, or <see langword="null"/> if this method does not use this attribute.</returns>
		protected virtual CustomDomainCommand GetCustomDomainCommandFromMethod(MethodInfo method, object instance = null)
		{
			// Sanity.
			if (null == method)
				return null;

			// If we cannot get the attribute then die.
			var attribute = method?.GetCustomAttribute<CustomCommandAttribute>();
			if (null == attribute)
				return null;

			Logger?.Trace($"[CustomCommand] found on {method.DeclaringType.FullName}.{method.Name}.  Attempting to use.");

			// Set the command ID if one is not set.
			attribute.CommandId = string.IsNullOrWhiteSpace(attribute.CommandId)
				? this.GetCustomDomainCommandId(method)
				: attribute.CommandId;

			// Convert the attribute to a custom domain command.
			try
			{

				// Validate the method signature.
				{
					var invalidSignatureString = $"The method signature must be `void {method.Name}(IConfigurationRequestContext c, ClientOperations o)`";
					if (typeof(void) != method.ReturnType)
						throw new ArgumentException(invalidSignatureString, nameof(method));
					if (method.ContainsGenericParameters)
						throw new ArgumentException(invalidSignatureString, nameof(method));
					var parameters = method.GetParameters();
					if (parameters.Length != 2)
						throw new ArgumentException(invalidSignatureString, nameof(method));
					if (parameters[0].ParameterType != typeof(IConfigurationRequestContext))
						throw new ArgumentException(invalidSignatureString, nameof(method));
					if (parameters[1].ParameterType != typeof(ClientOperations))
						throw new ArgumentException(invalidSignatureString, nameof(method));
				}

				// Validate the instance.
				if (null == instance)
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

				// Convert it to a domain command.
				return new CustomDomainCommand()
				{
					Execute = (c, o) =>
					{
						Logger?.Trace($"Command {attribute.CommandId} invoked.");
						try
						{
							method.Invoke
							(
								method.IsStatic ? null : instance,
								new object[] { c, o }
							);
						}
						catch (Exception e)
						{
							Logger?.Error(e, $"Exception whilst executing {attribute.CommandId}.");
							throw;
						}
					},
					ID = attribute.CommandId,
					Blocking = attribute.Blocking,
					ConfirmMessage = attribute.ConfirmMessage,
					DisplayName = attribute.Label,
					HelpText = attribute.HelpText,
					Locations = GetCommandLocationsFromMethodAttributes(method)?.ToList()
						?? new List<ICommandLocation>()
				};
			}
			catch (Exception e)
			{
				Logger?.Error(e, $"{method.DeclaringType.FullName}.{method.Name} cannot be used with [CustomCommand]; the method signature may not be correct.");
				return null;
			}
		}

		/// <summary>
		/// Reflects any <see cref="CommandLocationAttribute"/> instances on the given
		/// <paramref name="method"/> then calls <see cref="CommandLocationAttribute.ToCommandLocation"/>
		/// on each and returns the value.
		/// </summary>
		/// <param name="method">The method to reflect.</param>
		/// <returns>Any locations.</returns>
		protected virtual IEnumerable<ICommandLocation> GetCommandLocationsFromMethodAttributes(MethodInfo method)
		{
			// Sanity.
			if (null == method)
				throw new ArgumentNullException(nameof(method));

			// Get the locations via reflection.
			var locationAttributes = method
				.GetCustomAttributes<CommandLocationAttribute>()
				?? Enumerable.Empty<CommandLocationAttribute>();
			foreach (var a in locationAttributes.Where(l => l != null))
			{
				yield return a.ToCommandLocation();
			}
		}

		/// <summary>
		/// Returns the command ID for the provided <paramref name="method"/>.
		/// </summary>
		/// <param name="method">The method that will be run by the command.</param>
		/// <returns>The command ID.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="method"/> is <see langword="null"/>.</exception>
		protected virtual string GetCustomDomainCommandId(MethodInfo method)
		{
			if (null == method)
				throw new ArgumentNullException(nameof(method));
			return $"{method.DeclaringType.FullName}.{method.Name}";
		}

		/// <summary>
		/// Adds <paramref name="type"/> and <paramref name="instance"/> to the list of items
		/// to be scanned for custom domain commands.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
		/// <param name="type">The type to scan.</param>
		/// <param name="instance">The instance of the object to use when calling methods.</param>
		public virtual void Include(Type type, object instance = null)
			=> Included.Add(type, instance);

		/// <summary>
		/// Adds <paramref name="type"/> and <paramref name="instance"/> to the list of items
		/// to be scanned for custom domain commands.
		/// </summary>
		/// <typeparam name="TType">The type to scan.</typeparam>
		/// <param name="instance">The instance of the object to use when calling methods.</param>
		public virtual void Include<TType>(TType instance)
			=> Include
			(
				instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)),
				instance
			);

		/// <summary>
		/// Adds <paramref name="type"/> and <paramref name="instance"/> to the list of items
		/// to be scanned for custom domain commands.
		/// </summary>
		/// <param name="instance">The instance of the object to scan and use when calling methods.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
		public virtual void Include(object instance)
			=> Include
			(
				instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)),
				instance
			);
	}
}
