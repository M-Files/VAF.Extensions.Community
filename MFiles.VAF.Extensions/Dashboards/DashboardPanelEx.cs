using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.Dashboards;
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
			get { return this.DashboardPanel.Title; }
			set { this.DashboardPanel.Title = value; }
		}

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
			var fragment = this.DashboardPanel.Generate(xml);

			// Get a handle on the various elements.
			XmlElement panel = (XmlElement)fragment.SelectNodes("*[@class=\"panel\"]")[0];

			// Add the attributes.
			foreach (var key in this.Attributes.Keys)
			{
				// Can't have style here.
				if (key == "style")
					continue;
				var attr = xml.CreateAttribute(key);
				attr.Value = this.Attributes[key];
				panel.Attributes.Append(attr);
			}

			// Add the style.
			{
				var attr = xml.CreateAttribute("style");
				attr.Value = $"{this.GetCssStyles() ?? ""} {panel.GetAttribute("style") ?? ""}";
				panel.Attributes.Append(attr);
			}

			return fragment;
		}
	}
}
