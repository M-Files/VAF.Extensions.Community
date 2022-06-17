using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	public enum DashboardCommandWithIconStyle
	{
		None = 0,
		Default = TextAndIcon,
		Text = 1,
		Icon = 2,
		TextAndIcon = Text | Icon
	}
	public class DashboardDomainCommandEx
		: DashboardDomainCommand
	{
		public DashboardDomainCommandEx()
		{
			this.Attributes.Clear();
			this.Styles.Add("display", "inline-block");
		}
		/// <summary>
		/// The icon to display in this command.
		/// </summary>
		public string Icon { get; set; }

		/// <summary>
		/// The style to use when rendering the command.
		/// </summary>
		public DashboardCommandWithIconStyle CommandWithIconStyle { get; set; } = DashboardCommandWithIconStyle.Default;

		/// <summary>
		/// CSS styles.  Keys are the names (e.g. "font-size"), values are the value (e.g. "12px").
		/// </summary>
		public Dictionary<string, string> Styles { get; }
			= new Dictionary<string, string>();

		/// <summary>
		/// Returns the CSS styles for this element.
		/// </summary>
		/// <returns></returns>
		protected virtual string GetCssStyles()
		{
			return string.Join(";", this.Styles.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
		}

		public override XmlDocumentFragment Generate(XmlDocument xml)
		{
			// Use the base implementation.
			var fragment = base.Generate(xml);

			// Otherwise, let's get the element itself.
			var element = fragment.ChildNodes[0] as XmlElement;
			if (null == element)
				return fragment;

			// Add the title if we can.
			if (false == element.HasAttribute("title"))
				element.SetAttribute("title", this.Title);

			// Add the style.
			{
				var attr = xml.CreateAttribute("style");
				attr.Value = $"{this.GetCssStyles() ?? ""};{element.GetAttribute("style") ?? ""}".Trim();
				if (attr.Value?.StartsWith(";") ?? false)
					attr.Value = attr.Value.Substring(1);
				if (attr.Value?.EndsWith(";") ?? false)
					attr.Value = attr.Value.Substring(0, attr.Value.Length - 1);
				if (attr.Value.Length > 0)
					element.Attributes.Append(attr);
			}

			// Add the element to represent the icon style.
			this.AlterElementForCommandWithIconStyle(xml, element);

			// Okay!
			return fragment;

		}

		/// <summary>
		/// Alters <paramref name="element"/> to ensure that the resulting element
		/// is formatted according to <see cref="CommandWithIconStyle"/>.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="element"></param>
		protected virtual void AlterElementForCommandWithIconStyle(XmlDocument xml, XmlElement element)
		{
			// Sanity.
			if (null == element)
				return;

			// Get the new CSS elements.
			var cssStyles = new Dictionary<string, string>();

			// Should we use the icon?
			if ((this.CommandWithIconStyle & DashboardCommandWithIconStyle.Icon) > 0
				&& false == string.IsNullOrWhiteSpace(this.Icon))
			{
				// Add the icon.
				cssStyles.Add("background-image", $"url({DashboardHelpersEx.ImageFileToDataUri(this.Icon)})");
				cssStyles.Add("background-repeat", "no-repeat");

				// The background position depends on how we're rendering...
				if (((this.CommandWithIconStyle & DashboardCommandWithIconStyle.Text) > 0))
				{
					// Including text; left-align.
					cssStyles.Add("background-position", "5px center");
					cssStyles.Add("padding-left", "24px");
				}
				else
				{
					// Just the image.  Centre-align.
					cssStyles.Add("background-position", "center center");
					cssStyles.Add("width", "24px");
					cssStyles.Add("height", "24px");
				}
			}

			// If we should NOT use the text then hide it now.
			if(false == ((this.CommandWithIconStyle & DashboardCommandWithIconStyle.Text) > 0))
			{
				element.InnerText = "";
			}

			// Update the CSS.
			var attr = xml.CreateAttribute("style");
			attr.Value = $"{string.Join(";", cssStyles.Select(kvp => $"{kvp.Key}: {kvp.Value}")) ?? ""};{element.GetAttribute("style") ?? ""}".Trim();
			if (attr.Value?.StartsWith(";") ?? false)
				attr.Value = attr.Value.Substring(1);
			if (attr.Value?.EndsWith(";") ?? false)
				attr.Value = attr.Value.Substring(0, attr.Value.Length - 1);
			if (attr.Value.Length > 0)
				element.Attributes.Append(attr);

		}
	}
}
