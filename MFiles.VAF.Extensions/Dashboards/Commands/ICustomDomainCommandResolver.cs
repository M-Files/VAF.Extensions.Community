using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Dashboards.Commands
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
}
