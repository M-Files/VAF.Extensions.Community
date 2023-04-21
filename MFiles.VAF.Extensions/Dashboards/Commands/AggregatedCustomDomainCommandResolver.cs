using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
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
			=> CustomDomainCommandResolvers?
				.SelectMany(r => r.GetCustomDomainCommands()?.AsNotNull())?
				.AsNotNull();

		/// <inheritdoc />
		public virtual DashboardDomainCommandEx GetDashboardDomainCommand(string commandId, DashboardCommandStyle style = DashboardCommandStyle.Link)
			=> CustomDomainCommandResolvers?
			.Select(c => c.GetDashboardDomainCommand(commandId, style))
			.FirstOrDefault(c => c != null);
	}
}
