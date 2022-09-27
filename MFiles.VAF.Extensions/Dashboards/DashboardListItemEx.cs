using MFiles.VAF.Configuration.Domain.Dashboards;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	public class DashboardListItemEx : DashboardListItem
	{
		/// <summary>
		/// If set, may be used to order the list items prior to rendering.
		/// </summary>
		public int? Order { get; set; }

		/// <summary>
		/// If true, ensures that the "white-space" CSS value is set to normal.
		/// </summary>
		public bool SetWhitespaceToNormal { get; set; } = true;

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
