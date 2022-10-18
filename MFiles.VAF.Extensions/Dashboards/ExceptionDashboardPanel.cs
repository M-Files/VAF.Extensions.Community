using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	/// <summary>
	/// A specialised implementation of <see cref="DashboardPanelEx"/> to render an exception.
	/// Primary use-case is called from <see cref="ConfigurableVaultApplicationBase{TSecureConfiguration}.GetStatusDashboardRootItems(VAF.Configuration.AdminConfigurations.IConfigurationRequestContext)"/>
	/// to render out the fact that a dashboard exception threw whilst rendering.
	/// </summary>
	public class ExceptionDashboardPanel
		: DashboardPanelEx
	{
		public Exception Exception { get; }
		public ExceptionDashboardPanel(Exception e, string titleText = null)
		{
			this.Exception = e ?? throw new ArgumentNullException(nameof(e));

			// Set the inner content.
			this.InnerContent = new DashboardContentCollection()
			{
				new DashboardCustomContentEx($"<p style='color: red; margin-left: 30px'>{e.Message}</p>"),
				new DashboardCustomContentEx($"<p><pre style='padding: 0px; color: red; margin-left: 30px'>{e.StackTrace.EscapeXmlForDashboard()}</pre></p>")
			};

			// Set the title.
			var title = new DashboardCustomContentEx(titleText ?? "Exception")
			{
				Icon = "Resources/Images/Failed.png"
			};
			title.Styles.AddOrUpdate("color", "red");
			this.TitleDashboardContent = title;

			// General styling.
			this.Styles.AddOrUpdate("padding", "5px 0px");
			this.Styles.AddOrUpdate("border-top", "1px solid red");
			this.Styles.AddOrUpdate("border-bottom", "1px solid red");
		}
	}
}
