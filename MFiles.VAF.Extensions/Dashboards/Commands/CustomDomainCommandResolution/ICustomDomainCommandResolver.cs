using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution
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

		/// <summary>
		/// Returns a dashboard domain command for the given command Id.
		/// </summary>
		/// <param name="commandId">The ID of the command to return.</param>
		/// <param name="style">The style for the command.</param>
		/// <returns>The command, or null if not found.</returns>
		DashboardDomainCommandEx GetDashboardDomainCommand
		(
			string commandId,
			DashboardCommandStyle style = default
		);
	}

	/// <summary>
	/// A base implementation of <see cref="ICustomDomainCommandResolver"/>
	/// that provides an implementation of <see cref="ICustomDomainCommandResolver.GetDashboardDomainCommand(string, DashboardCommandStyle)"/>
	/// that iterates over <see cref="ICustomDomainCommandResolver.GetCustomDomainCommands"/> to find a command with the supplied ID.
	/// </summary>
	public abstract class CustomDomainCommandResolverBase
		: ICustomDomainCommandResolver
	{

		/// <inheritdoc />
		public abstract IEnumerable<CustomDomainCommand> GetCustomDomainCommands();

		/// <inheritdoc />
		public virtual DashboardDomainCommandEx GetDashboardDomainCommand
		(
			string commandId,
			DashboardCommandStyle style = default
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(commandId))
				return null;

			// Try to get the domain command for this method.
			var command = GetCustomDomainCommands()
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
	}
}
