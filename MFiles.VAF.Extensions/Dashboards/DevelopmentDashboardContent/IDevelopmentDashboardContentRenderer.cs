using MFiles.VAF.Configuration.Domain.Dashboards;

namespace MFiles.VAF.Extensions.Dashboards.DevelopmentDashboardContent
{
#if DEBUG
	public interface IDevelopmentDashboardContentRenderer
	{
		/// <summary>
		/// Gets the development dashboard content.
		/// </summary>
		/// <returns>The content, or null if nothing to render.</returns>
		IDashboardContent GetDashboardContent();

		/// <summary>
		/// Populates internal dictionaries with referenced assemblies.
		/// Should be called once at startup.
		/// </summary>
		/// <typeparam name="TConfigurationType"></typeparam>
		void PopulateReferencedAssemblies<TConfigurationType>();
	}
#endif

}
