using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.Dashboards
{
	[TestClass]
	public class DashboardListItemWithNormalWhitespaceTests
	{
		[TestMethod]
		public void WhiteSpaceExplicitlySet()
		{
			var innerContent = new DashboardCustomContent("<p>hello world.</p>");
			var content = new DashboardListItemWithNormalWhitespace()
			{
				InnerContent = innerContent
			};

			// Get the "content" div in the list item.
			var element = content.ToXmlFragment()?.FirstChild?.SelectSingleNode("div[@class='content']");
			Assert.IsNotNull(element);

			// Ensure that the whitespace is overridden.
			Assert.AreEqual("white-space: normal", element.Attributes["style"]?.Value);

		}
	}
}
