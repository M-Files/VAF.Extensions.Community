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
		/// <summary>
		/// The exception that is being represented.
		/// Note that the exception may be null if no underlying exception was thrown.
		/// </summary>
		public Exception Exception { get; }
		public ExceptionDashboardPanel(Exception e, string titleText = null)
			: this(titleText, e?.Message, e?.StackTrace)
		{
			this.Exception = e ?? throw new ArgumentNullException(nameof(e));
		}
		public ExceptionDashboardPanel
		(
			string titleText, 
			string message, 
			string stackTrace = null
		)
		{
			// Set the inner content.
			var collection = new DashboardContentCollection();
			if(!string.IsNullOrWhiteSpace(message))
				collection.Add(new DashboardCustomContentEx($"<p style='color: red; margin-left: 30px'>{message.EscapeXmlForDashboard()}</p>"));
			if (!string.IsNullOrWhiteSpace(stackTrace))
				collection.Add(new DashboardCustomContentEx($"<p><pre style='padding: 0px; color: red; margin-left: 30px'>{stackTrace.EscapeXmlForDashboard()}</pre></p>"));
			this.InnerContent = collection;

			// Set the title.
			var title = new DashboardCustomContentEx(string.IsNullOrWhiteSpace(titleText) ? "Exception" : titleText)
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
