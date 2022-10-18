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

		/// <summary>
		/// CSS styles for the innercontent node.  Keys are the names (e.g. "font-size"), values are the value (e.g. "12px").
		/// </summary>
		public Dictionary<string, string> InnerContentStyles { get; }
			= new Dictionary<string, string>();

		/// <summary>
		/// Returns the CSS styles for the inner content.
		/// </summary>
		/// <returns></returns>
		protected virtual string GetInnerContentCssStyles()
		{
			return string.Join(";", this.InnerContentStyles.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
		}

		/// <summary>
		/// Sets any styles defined in <see cref="DashboardContentBase.InnerContentStyles"/> 
		/// to <paramref name="element"/>.
		/// </summary>
		/// <param name="xml">The XML document that <paramref name="element"/> comes from.</param>
		/// <param name="element">The element to alter.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="xml"/> or <paramref name="element"/> are null.</exception>
		protected virtual void ApplyInnerContentStyles(XmlDocument xml, XmlElement element)
		{
			// Sanity.
			if (null == element)
				throw new ArgumentNullException(nameof(xml));
			if (null == element)
				throw new ArgumentNullException(nameof(element));

			var attr = xml.CreateAttribute("style");
			attr.Value = $"{this.GetInnerContentCssStyles() ?? ""};{element.GetAttribute("style") ?? ""}".Trim();
			if (attr.Value?.StartsWith(";") ?? false)
				attr.Value = attr.Value.Substring(1);
			if (attr.Value?.EndsWith(";") ?? false)
				attr.Value = attr.Value.Substring(0, attr.Value.Length - 1);
			if (attr.Value.Length > 0)
				element.Attributes?.Append(attr);
		}

		public DashboardPanelEx()
		{
			// Collapse the spacing between the panels.
			this.Styles.Add("padding", "0 16px");
		}

		/// <inheritdoc />
		protected override XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml)
		{
			var fragment = this.DashboardPanel?.Generate(xml);


			XmlElement panel = (XmlElement)fragment?.SelectNodes("*[@class=\"panel\"]")[0];
			XmlElement titleBar = (XmlElement)panel?.SelectNodes("*[@class=\"title-bar\"]")[0];
			XmlElement title = (XmlElement)titleBar?.SelectNodes("*[@class=\"title\"]")[0];
			XmlElement cmdBar = (XmlElement)titleBar.SelectNodes("*[@class=\"command-bar\"]")[0];
			XmlElement content = (XmlElement)panel.SelectNodes("*[@class=\"content\"]")[0];

			// Do we need to be more clever with the title?
			if (null != this.TitleDashboardContent)
			{
				if(null != title)
				{
					title.InnerXml = this.TitleDashboardContent.ToXmlString();
				}
			}

			// Any styles for the content?
			this.ApplyInnerContentStyles(xml, content);

			return fragment;
		}
	}
}
