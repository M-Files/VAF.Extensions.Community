using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using MFilesAPI;
using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent;
using MFiles.VAF.Extensions.Dashboards.LoggingDashboardContent;
using MFiles.VAF.Extensions.Dashboards.DevelopmentDashboardContent;
using MFiles.VAF.Extensions.Dashboards.ApplicationOverviewDashboardContent;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		/// <summary>
		/// The logo to render to the dashboard.
		/// If null, no logo is rendered.
		/// </summary>
		public ILogoSource DashboardLogo { get; set; }

		#region Top-level dashboard generation

		/// <summary>
		/// The default dashboard refresh interval (in seconds).
		/// </summary>
		public const int DefaultDashboardRefreshInterval = 30;

		/// <summary>
		/// The interval (in seconds) to refresh the dashboard.  If null or <=0, no refresh will be done.
		/// </summary>
		public int? DashboardRefreshInterval = DefaultDashboardRefreshInterval;

		/// <inheritdoc />
		/// <remarks>
		/// Calls <see cref="CreateStatusDashboard()"/> to create the dashboard,
		/// then <see cref="GetStatusDashboardRootItems(IConfigurationRequestContext)"/> to get the items to display,
		/// then returns <see cref="StatusDashboard.ToString()"/>.
		/// </remarks>
		public override string GetDashboardContent(IConfigurationRequestContext context)
		{
			// Create a new dashboard.
			StatusDashboard dashboard;
			try
			{
				dashboard = this.CreateStatusDashboard();
				if (null == dashboard)
					return "";
			}
			catch(Exception e)
			{
				// Log the exception and render an exception panel.
				var exception = new Exception("Could not create status dashboard", e);
				FormattableString message = $"Could not create status dashboard.";
				this.Logger?.Error(e, message);
				return new ExceptionDashboardPanel(exception, $"Could not create status dashboard")?.ToXmlString();
			}

			// Add all content in turn.
			foreach (var content in this.GetStatusDashboardRootItems(context) ?? Enumerable.Empty<IDashboardContent>())
				if (null != content)
					dashboard.AddContent(content);

			// Return the dashboard.
			try
			{
				var dashboardContent = dashboard.ToString();

				// If we have no dashboard content and the base implementation
				// of StartApplication was not called then this is likely a bug.
				if(dashboard.Contents.Count == 0
					&& !this.startApplicationCalled)
				{
					var content = new ExceptionDashboardPanel("Exception rendering dashboard", "If you override StartApplication then ensure that you call base.StartApplication so that the dashboard can correctly render.");
					content.Styles.Add("margin", "10px");
					dashboardContent = content.ToXmlString();
				}

				return dashboardContent;
			}
			catch (Exception e)
			{
				// Log the exception and render an exception panel.
				var exception = new Exception("Could not render dashboard to a string", e);
				FormattableString message = $"Could not render dashboard to a string.";
				this.Logger?.Error(e, message);
				return new ExceptionDashboardPanel(exception, $"Could not render dashboard")?.ToString();
			}
		}

		/// <summary>
		/// Creates the status dashboard object that will be populated by <see cref="GetDashboardContent(IConfigurationRequestContext)"/>.
		/// </summary>
		/// <returns>The status dashboard into which the dashboard content will be populated.</returns>
		public virtual StatusDashboard CreateStatusDashboard()
		{
			return new StatusDashboard()
			{
				RefreshInterval = this.DashboardRefreshInterval ?? 0
			};
		}

		/// <summary>
		/// Gets the root items for the dashboard.
		/// </summary>
		/// <param name="context">The request context for this dashboard generation.</param>
		/// <returns>The dashboard content.</returns>
		public virtual IEnumerable<IDashboardContent> GetStatusDashboardRootItems(IConfigurationRequestContext context)
		{
			// All of the section generators, in order.
			var funcs = new List<Func<IConfigurationRequestContext, IDashboardContent>>()
			{
				// Application overview.
				this.GetApplicationOverviewDashboardContent,
				
				// Any asynchronous operation content.
				this.GetAsynchronousOperationDashboardContent,

				// Logging content.
				this.GetLoggingDashboardContent

#if DEBUG
				// Output any data that may be useful for development of the extensions library.
				,this.GetDevelopmentDashboardData
#endif
			};

			// Generate each in turn.
			foreach(var f in funcs)
			{
				IDashboardContent content = null;
				try
				{
					// Call the section generator.
					content = f(context);
				}
				catch(Exception e)
				{
					// Log the exception and render an exception panel.
					FormattableString message = $"Could not render dashboard section ({f.Method.DeclaringType}.{f.Method.Name}).";
					this.Logger?.Error(e, message);
					content = new ExceptionDashboardPanel(e, $"Could not render {f.Method.Name}");
				}
				if (content != null)
					yield return content;
			}

		}

		#endregion

		#region Application overview

		/// <summary>
		/// Gets the <see cref="IApplicationOverviewDashboardContentRenderer"/> that will render the application overview dashboard section.
		/// If null, nothing will be returned for this section.
		/// </summary>
		protected internal IApplicationOverviewDashboardContentRenderer ApplicationOverviewDashboardContentRenderer { get; private set; }

		/// <summary>
		/// Returns the implementation for rendering application overview dashboard content.
		/// By default this is an instance of <see cref="DefaultApplicationOverviewDashboardContentRenderer"/>.
		/// </summary>
		/// <returns>The renderer, or null if nothing should be rendered.</returns>
		protected virtual IApplicationOverviewDashboardContentRenderer GetApplicationOverviewDashboardContentRenderer()
			=> new DefaultApplicationOverviewDashboardContentRenderer<TSecureConfiguration>(this);

		/// <summary>
		/// Gets the content showing the application name, version, etc.
		/// </summary>
		/// <param name="context">The request context for this dashboard generation.</param>
		/// <returns>The dashboard content, or null if none should be rendered.</returns>
		public virtual IDashboardContent GetApplicationOverviewDashboardContent(IConfigurationRequestContext context)
			=> this.ApplicationOverviewDashboardContentRenderer?.GetDashboardContent();

		#endregion

		#region Asynchronous dashboard data

		/// <summary>
		/// Gets the <see cref="IAsynchronousDashboardContentRenderer"/> that will render the asynchronous dashboard section.
		/// If null, nothing will be returned for this section.
		/// </summary>
		protected internal IAsynchronousDashboardContentRenderer AsynchronousDashboardContentRenderer { get; private set; }

		/// <summary>
		/// Gets the collection of <see cref="IAsynchronousDashboardContentProvider"/> instances that will return asynchronous
		/// dashboard data.
		/// </summary>
		protected internal List<IAsynchronousDashboardContentProvider> AsynchronousDashboardContentProviders { get; private set; }
			= new List<IAsynchronousDashboardContentProvider>();

		/// <summary>
		/// Returns the implementation for rendering asynchronous dashboard content.
		/// By default this is an instance of <see cref="DashboardListAsynchronousDashboardContentRenderer"/>.
		/// </summary>
		/// <returns>The renderer, or null if nothing should be rendered.</returns>
		protected virtual IAsynchronousDashboardContentRenderer GetAsynchronousDashboardContentRenderer()
			=> new DashboardListAsynchronousDashboardContentRenderer();

		/// <summary>
		/// Returns all providers of asynchronous content.
		/// By default this returns an instance of <see cref="TaskQueueBackgroundOperationManagerAsynchronousDashboardContentProvider{TConfiguration}"/>
		/// and an instance of <see cref="TaskManagerExAsynchronousDashboardContentProvider{TConfiguration}"/>.
		/// </summary>
		/// <returns>The providers, or an empty collection if nothing should be rendered.</returns>
		protected virtual IEnumerable<IAsynchronousDashboardContentProvider> GetAsynchronousDashboardContentProviders()
		{
			// Return the data from task queue background operation manager.
			yield return new TaskQueueBackgroundOperationManagerAsynchronousDashboardContentProvider<TSecureConfiguration>
			(
				this
			);

			// Return the data from the task manager.
			yield return new TaskManagerExAsynchronousDashboardContentProvider<TSecureConfiguration>
			(
				this.PermanentVault,
				this.TaskManager,
				this.TaskQueueResolver,
				this.RecurringOperationConfigurationManager
			);
		}

		/// <summary>
		/// Returns the dashboard content showing asynchronous operation status.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no background operation managers, background operations or task processors.</returns>
		public virtual IDashboardContent GetAsynchronousOperationDashboardContent(IConfigurationRequestContext context)
			=> this.AsynchronousDashboardContentRenderer?.GetDashboardContent(this.AsynchronousDashboardContentProviders);

		#endregion

		#region Logging dashboard data

		/// <summary>
		/// Gets the <see cref="ILoggingDashboardContentRenderer"/> that will render the logging dashboard section.
		/// If null, nothing will be returned for this section.
		/// </summary>
		protected internal ILoggingDashboardContentRenderer LoggingDashboardContentRenderer { get; private set; }

		/// <summary>
		/// Returns the implementation for rendering logging dashboard content.
		/// By default this is an instance of <see cref="DashboardListLoggingDashboardContentRenderer"/>.
		/// </summary>
		/// <returns>The renderer, or null if nothing should be rendered.</returns>
		protected virtual ILoggingDashboardContentRenderer GetLoggingDashboardContentRenderer()
			=> new DashboardListLoggingDashboardContentRenderer();

		/// <summary>
		/// Returns the dashboard content showing logging status.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no logging data is available or configured.</returns>
		public virtual IDashboardContent GetLoggingDashboardContent(IConfigurationRequestContext context)
			=> this.LoggingDashboardContentRenderer?.GetDashboardContent(context, this.GetLoggingConfiguration());

		#endregion

		#region Development dashboard data

#if DEBUG

		/// <summary>
		/// Gets the <see cref="IDevelopmentDashboardContentRenderer"/> that will render the development dashboard section.
		/// If null, nothing will be returned for this section.
		/// </summary>
		protected internal IDevelopmentDashboardContentRenderer DevelopmentDashboardContentRenderer { get; private set; }

		public virtual IDashboardContent GetDevelopmentDashboardData(IConfigurationRequestContext context)
			=> this.DevelopmentDashboardContentRenderer?.GetDashboardContent();

		/// <summary>
		/// Returns the implementation for rendering development  dashboard content.
		/// By default this is an instance of <see cref="DashboardListDevelopmentDashboardContentRenderer"/>.
		/// </summary>
		/// <returns>The renderer, or null if nothing should be rendered.</returns>
		protected virtual IDevelopmentDashboardContentRenderer GetDevelopmentDashboardContentRenderer()
			=> new DashboardListDevelopmentDashboardContentRenderer();
#endif

		#endregion

	}
}
