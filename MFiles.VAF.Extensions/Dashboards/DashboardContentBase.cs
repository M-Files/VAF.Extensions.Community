using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	public abstract class DashboardContentBase
		: IDashboardContent
	{
		/// <summary>
		/// The id of the element. Optional.
		/// Will appear in the html output, so the item can be referenced.
		/// </summary>
		public virtual string ID { get; set; }

		/// <summary>
		/// A path to the icon to use for the element (optional).
		/// Should resolve to a file on either the server or client configurator app.
		/// </summary>
		public virtual string Icon { get; set; }

		/// <summary>
		/// Attributes to be rendered onto the top-level element.
		/// </summary>
		public Dictionary<string, string> Attributes { get; }
			= new Dictionary<string, string>();

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

		/// <inheritdoc />
		public virtual XmlDocumentFragment Generate(XmlDocument xml)
		{
			var fragment = this.GenerateXmlDocumentFragment(xml);
			if (null == fragment)
				throw new ApplicationException(String.Format(Resources.Exceptions.Dashboard.XmlFragmentNull, this.GetType().Name));

			var element = fragment.FirstChild as XmlElement;
			if (null == element)
			{
				fragment = DashboardHelper.CreateFragment(xml, $"<span>{fragment.InnerXml}</span>");
				element = fragment.FirstChild as XmlElement;
			}

			// Add the id if defined.
			if (!String.IsNullOrWhiteSpace(this.ID))
				element.SetAttribute("id", this.ID);

			// Add the attributes.
			foreach (var key in this.Attributes.Keys)
			{
				// Can't have style here.
				if (key == "style")
					continue;
				var attr = xml.CreateAttribute(key);
				attr.Value = this.Attributes[key];
				element.Attributes.Append(attr);
			}

			// Render the icon.
			this.RenderIconTo(element);

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

			return fragment;
		}

		/// <summary>
		/// Called by <see cref="Generate"/>.  Should return the XML fragment
		/// for <see cref="Generate(XmlDocument)"/> to then add ID, class, style, etc. to.
		/// </summary>
		/// <param name="xml">The document.</param>
		/// <returns>The XML fragment.</returns>
		protected abstract XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml);

		/// <summary>
		/// Renders the icon to the provided <paramref name="element"/>.
		/// </summary>
		/// <param name="element">The element to render to.</param>
		protected virtual void RenderIconTo(XmlElement element)
		{
			// Add item icon, if defined.
			if (!String.IsNullOrWhiteSpace(this.Icon))
			{
				// Add the icon class to include the padding and other background style options.
				DashboardHelper.AddClass(element, "icon");

				// Set the background image explicitly.
				DashboardHelper.AddStyle(element, "background-image", String.Format("url({0})", DashboardHelpersEx.ImageFileToDataUri(this.Icon)));
				DashboardHelper.AddStyle(element, "background-repeat", "no-repeat");
				DashboardHelper.AddStyle(element, "background-position", "0px center");
				DashboardHelper.AddStyle(element, "padding-left", "20px");
			}
		}
	}
}
