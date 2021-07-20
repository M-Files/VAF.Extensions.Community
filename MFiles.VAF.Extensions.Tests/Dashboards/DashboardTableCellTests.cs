using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Dashboards
{
	[TestClass]
	public class DashboardTableCellTests
		: DashboardContentBaseTests<DashboardTableCell>
	{
		[TestMethod]
		public void NullInnerContentDoesNotThrow()
		{
			new DashboardTableCell(innerContent: null);
		}
		[TestMethod]
		public void NullInnerContentDoesNotThrow2()
		{
			new DashboardTableCell(htmlContent: null);
		}

		[TestMethod]
		public void InnerContentSetCorrectly()
		{
			var innerContent = new DashboardTableCell("<p>hello world.</p>");
			var content = new DashboardTableCell(innerContent);
			Assert.AreEqual(innerContent, content?.InnerContent);
		}

		[TestMethod]
		public void StringInnerContentWrapped()
		{
			var innerContent = "<p>hello world.</p>";
			var content = new DashboardTableCell(innerContent);
			Assert.IsNotNull(content?.InnerContent as DashboardCustomContentEx);
			Assert.AreEqual(innerContent, content?.InnerContent?.ToXmlString());
		}

		[TestMethod]
		public void ElementNameCorrect_TD()
		{
			var content = new DashboardTableCell("hello");
			var output = content.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(output);
			Assert.AreEqual("td", output.LocalName);
		}

		[TestMethod]
		public void ElementNameCorrect_TH()
		{
			var content = new DashboardTableCell("hello") { DashboardTableCellType = DashboardTableCellType.Header };
			var output = content.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(output);
			Assert.AreEqual("th", output.LocalName);
		}

		[TestMethod]
		public void DefaultStyles_TD()
		{
			var content = new DashboardTableCell("hello");
			var output = content.ToXmlFragment()?.FirstChild;
			Assert.IsTrue
			(
				new StyleComparisonHelper("font-size: 12px; padding: 2px 3px; text-align: left;")
				.TestAgainstString(output?.Attributes["style"]?.Value)
			);
		}

		[TestMethod]
		public void DefaultStyles_TH()
		{
			var content = new DashboardTableCell("hello") { DashboardTableCellType = DashboardTableCellType.Header };
			var output = content.ToXmlFragment()?.FirstChild;
			Assert.IsTrue
			(
				new StyleComparisonHelper("font-size: 12px; padding: 2px 3px; text-align: left;border-bottom: 1px solid #CCC;")
				.TestAgainstString(output?.Attributes["style"]?.Value)
			);
		}

		public override DashboardTableCell CreateDashboardContent()
		{
			return new DashboardTableCell("<p>hello world.</p>");
		}

	}
}
