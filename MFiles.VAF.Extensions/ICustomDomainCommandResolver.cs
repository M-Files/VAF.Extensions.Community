using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Returns custom domain commands.
	/// </summary>
	public interface ICustomDomainCommandResolver
	{
		/// <summary>
		/// Gets all custom domain commands.
		/// </summary>
		/// <returns>The found domain commands.</returns>
		IEnumerable<CustomDomainCommand> GetCustomDomainCommands();
	}

	/// <summary>
	/// A default implementation of <see cref="ICustomDomainCommandResolver"/>
	/// which uses reflection to find methods decorated with <see cref="CustomCommandAttribute"/>.
	/// </summary>
	public class DefaultCustomDomainCommandResolver
		: ICustomDomainCommandResolver
	{
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
		protected List<Tuple<Type, object>> Included { get; } = new List<Tuple<Type, object>>();

		/// <inheritdoc />
		public virtual IEnumerable<CustomDomainCommand> GetCustomDomainCommands()
		{
			// Get everything from the included data.
			return this.Included?
				.AsNotNull()
				.SelectMany(t => this.GetCustomDomainCommands(t.Item1, t.Item2));
		}

		/// <summary>
		/// Scans <paramref name="type"/> for methods decorated with <see cref="CustomCommandAttribute"/>.
		/// Returns custom domain commands.
		/// </summary>
		/// <param name="type">The type to look for methods in.</param>
		/// <param name="instance">The instance to use when calling the method.</param>
		/// <returns>Any and all custom domain commands.</returns>
		public virtual IEnumerable<CustomDomainCommand> GetCustomDomainCommands(Type type, object instance = null)
		{
			// Sanity.
			if (null == type)
				yield break;

			// Find methods with the correct attribute.
			var methods = type
				.GetMethods()
				?? Enumerable.Empty<MethodInfo>();
			foreach (var m in methods)
			{
				// If we cannot get the attribute then die.
				var attr = m?.GetCustomAttribute<CustomCommandAttribute>();
				if (null == attr)
					continue;

				// Convert the attribute to a custom domain command.
				yield return attr?.ToCustomDomainCommand(m, instance);
			}
		}

		/// <summary>
		/// Adds <paramref name="type"/> and <paramref name="instance"/> to the list of items
		/// to be scanned for custom domain commands.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
		/// <param name="type">The type to scan.</param>
		/// <param name="instance">The instance of the object to use when calling methods.</param>
		public virtual void Include(Type type, object instance = null)
			=> this.Included.Add(new Tuple<Type, object>(type, instance));

		/// <summary>
		/// Adds <paramref name="type"/> and <paramref name="instance"/> to the list of items
		/// to be scanned for custom domain commands.
		/// </summary>
		/// <typeparam name="TType">The type to scan.</typeparam>
		/// <param name="instance">The instance of the object to use when calling methods.</param>
		public virtual void Include<TType>(TType instance)
			=> this.Include
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
			=> this.Include
			(
				instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)),
				instance
			);
	}

	/// <summary>
	/// An implementation of <see cref="ICustomDomainCommandResolver"/> that 
	/// can be used to return custom domain commands from multiple other instances
	/// of <see cref="ICustomDomainCommandResolver"/>.
	/// </summary>
	public class AggregatedCustomDomainCommandResolver
		: ICustomDomainCommandResolver
	{
		public List<ICustomDomainCommandResolver> CustomDomainCommandResolvers { get; }
			= new List<ICustomDomainCommandResolver>();

		/// <inheritdoc />
		public virtual IEnumerable<CustomDomainCommand> GetCustomDomainCommands()
			=> this.CustomDomainCommandResolvers?
				.SelectMany(r => r.GetCustomDomainCommands()?.AsNotNull())?
				.AsNotNull();
	}
}
