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
	public class DashboardCustomContentExTests
		: DashboardContentBaseTests<DashboardCustomContentEx>
	{
		[TestMethod]
		public void NullInnerContentDoesNotThrow()
		{
			new DashboardCustomContentEx(innerContent: null);
		}
		[TestMethod]
		public void NullInnerContentDoesNotThrow2()
		{
			new DashboardCustomContentEx(htmlContent: null);
		}
		[TestMethod]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void UnencodedContentDoesThrow()
		{
			var content = new DashboardCustomContentEx(htmlContent: "hello & world ");
			content.ToXmlString();
		}
		[TestMethod]
		public void EncodedContentDoesNotThrow()
		{
			var content = new DashboardCustomContentEx(htmlContent: "hello &amp; world ");
			content.ToXmlString();
		}

		[TestMethod]
		public void InnerContentSetCorrectly()
		{
			var innerContent = new DashboardCustomContent("<p>hello world.</p>");
			var content = new DashboardCustomContentEx(innerContent);
			Assert.AreEqual(innerContent, content?.InnerContent);
		}

		[TestMethod]
		public void StringInnerContentWrapped()
		{
			var innerContent = "<p>hello world.</p>";
			var content = new DashboardCustomContentEx(innerContent);
			Assert.IsInstanceOfType(content?.InnerContent, typeof(DashboardCustomContent));
			Assert.AreEqual(innerContent, content?.InnerContent?.ToXmlString());
		}

		[TestMethod]
		public void ToXmlStringReturnsInnerContent()
		{
			var innerContent = "<p>hello world.</p>";
			var content = new DashboardCustomContentEx(innerContent);
			Assert.IsInstanceOfType(content?.InnerContent, typeof(DashboardCustomContent));
			Assert.AreEqual(content?.InnerContent?.ToXmlString(), content.ToXmlString());
		}


		public override DashboardCustomContentEx CreateDashboardContent()
		{
			return new DashboardCustomContentEx("<p>hello world.</p>");
		}

	}
}
