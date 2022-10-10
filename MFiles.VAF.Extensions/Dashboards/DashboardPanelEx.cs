using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	public class DashboardPanelEx
		: DashboardContentBase
	{
		protected DashboardPanel DashboardPanel { get; }
			= new DashboardPanel();

		/// <inheritdoc />
		public override string ID
		{
			get { return this.DashboardPanel.ID; }
			set { this.DashboardPanel.ID = value; }
		}

		/// <inheritdoc />
		public override string Icon
		{
			get { return this.DashboardPanel.Icon; }
			set { this.DashboardPanel.Icon = value; }
		}

		/// <summary>
		/// The title of the panel (optional).
		/// </summary>
		public string Title
		{
			get
			{
				return null == this.TitleDashboardContent
					? this.DashboardPanel.Title
					: this.TitleDashboardContent.ToString();
			}
			set
			{
				this.TitleDashboardContent = null;
				this.DashboardPanel.Title = value;
			}
		}

		/// <summary>
		/// The title of the panel.
		/// </summary>
		public IDashboardContent TitleDashboardContent { get; set; }

		/// <summary>
		/// The status of the panel (optional).
		/// If specified, rendered as a <see cref="StatusStub"/>. 
		/// </summary>
		public DomainStatusSummary StatusSummary
		{
			get { return this.DashboardPanel.StatusSummary; }
			set { this.DashboardPanel.StatusSummary = value; }
		}

		/// <summary>
		/// The top-level commands to show for the panel (optional).
		/// </summary>
		public List<DashboardCommand> Commands
		{
			get { return this.DashboardPanel.Commands; }
			set { this.DashboardPanel.Commands = value; }
		}

		/// <summary>
		/// The panel content (optional).
		/// </summary>
		public IDashboardContent InnerContent
		{
			get { return this.DashboardPanel.InnerContent; }
			set { this.DashboardPanel.InnerContent = value; }
		}

		/// <summary>
		/// Controls the panel background color.
		/// </summary>
		public PanelBackground Background
		{
			get { return this.DashboardPanel.Background; }
			set { this.DashboardPanel.Background = value; }
		}

		/// <summary>
		/// The domain tree path this list item represents (optional).
		/// If specified, the title will become a link that navigates
		/// to the tree node at the path in the configurator's navigation pane.
		/// </summary>
		public string TreePath { get; set; }

		public DashboardPanelEx()
		{
			// Collapse the spacing between the panels.
			this.Styles.Add("padding", "0 16px");
		}

		/// <inheritdoc />
		protected override XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml)
		{
			var fragment = this.DashboardPanel?.Generate(xml);

			// Do we need to be more clever with the title?
			if(null != this.TitleDashboardContent)
			{
				XmlElement panel = (XmlElement)fragment?.SelectNodes("*[@class=\"panel\"]")[0];
				XmlElement titleBar = (XmlElement)panel?.SelectNodes("*[@class=\"title-bar\"]")[0];
				XmlElement title = (XmlElement)titleBar?.SelectNodes("*[@class=\"title\"]")[0];
				if(null != title)
				{
					title.InnerXml = this.TitleDashboardContent.ToXmlString();
				}
			}

			return fragment;
		}
	}

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
