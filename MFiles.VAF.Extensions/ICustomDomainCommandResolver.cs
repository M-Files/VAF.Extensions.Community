using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
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
		IEnumerable<CustomDomainCommandEx> GetCustomDomainCommands();

		/// <summary>
		/// Returns a dashboard domain command for the given command Id.
		/// </summary>
		/// <param name="commandId">The ID of the command to return.</param>
		/// <param name="style">The style for the command.</param>
		/// <returns>The command, or null if not found.</returns>
		DashboardDomainCommandEx GetDashboardDomainCommand
		(
			string commandId,
			DashboardCommandStyle style = DashboardCommandStyle.Link
		);
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
		protected Dictionary<Type, object> Included { get; } = new Dictionary<Type, object>();

		/// <inheritdoc />
		public DashboardDomainCommandEx GetDashboardDomainCommand
		(
			string commandId,
			DashboardCommandStyle style = DashboardCommandStyle.Link
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(commandId))
				return null;

			// Try to get the domain command for this method.
			var command = this.GetCustomDomainCommands()
				.FirstOrDefault(c => c.ID == commandId);
			
			// Sanity.
			if (null == command)
				return null;

			// Return the command.
			return new DashboardDomainCommandEx
			{
				DomainCommandID = command.ID,
				Title = command.DisplayName,
				Style = style
			};
		}

		/// <inheritdoc />
		public virtual IEnumerable<CustomDomainCommandEx> GetCustomDomainCommands()
		{
			// Get everything from the included data.
			return this.Included?
				.AsNotNull()
				.SelectMany(t => this.GetCustomDomainCommands(t.Key, t.Value));
		}

		/// <summary>
		/// Scans <paramref name="type"/> for methods decorated with <see cref="CustomCommandAttribute"/>.
		/// Returns custom domain commands.
		/// </summary>
		/// <param name="type">The type to look for methods in.</param>
		/// <param name="instance">The instance to use when calling the method.</param>
		/// <returns>Any and all custom domain commands.</returns>
		public virtual IEnumerable<CustomDomainCommandEx> GetCustomDomainCommands(Type type, object instance = null)
		{
			// Sanity.
			if (null == type)
				yield break;

			// Find methods with the correct attribute.
			var methods = type
				.GetMethods(this.BindingFlags)
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
			=> this.Included.Add(type, instance);

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
		public virtual IEnumerable<CustomDomainCommandEx> GetCustomDomainCommands()
			=> this.CustomDomainCommandResolvers?
				.SelectMany(r => r.GetCustomDomainCommands()?.AsNotNull())?
				.AsNotNull();

		/// <inheritdoc />
		public virtual DashboardDomainCommandEx GetDashboardDomainCommand(string commandId, DashboardCommandStyle style = DashboardCommandStyle.Link)
			=> this.CustomDomainCommandResolvers?
			.Select(c => c.GetDashboardDomainCommand(commandId, style))
			.FirstOrDefault(c => c != null);
	}
}
