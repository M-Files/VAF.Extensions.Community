using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Xml;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Renders a <see cref="DashboardListItem"/>, but sets the "style" attribute to remove the "white-space: pre-line"
	/// which is included by default in the standard stylesheet (and makes things look odd).
	/// </summary>
	internal class DashboardListItemWithNormalWhitespace : DashboardListItem
	{

		/// <inheritdoc />
		public override XmlDocumentFragment Generate(XmlDocument xml)
		{
			var fragment = base.Generate(xml);

			// Get a handle on the various elements.
			XmlElement listItem = (XmlElement)fragment.SelectNodes("li")[0];
			XmlElement content = (XmlElement)listItem.SelectNodes("*[@class=\"content\"]")[0];

			// Explicitly set the whitespace to normal.
			content.SetAttribute("style", "white-space: normal");

			return fragment;
		}

	}
}
