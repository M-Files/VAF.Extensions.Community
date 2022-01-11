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
	public class DashboardTableTests
		: DashboardContentBaseTests<DashboardTable>
	{

		public override DashboardTable CreateDashboardContent()
		{
			return new DashboardTable();
		}

		[TestMethod]
		// Does not support icons.
		public override void Icon_PathToFile()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Icon = "/some/file.png";
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;

			// This component does not support icons.
			// We should have an element, but the class should not be set.
			Assert.IsNotNull(element);
			Assert.AreEqual("table-wrapper", element.Attributes["class"]?.Value ?? "");
			Assert.IsFalse
			(
				(element.Attributes["style"]?.Value ?? "").Contains("background-image:url('")
			);
		}

		[TestMethod]
		// Does not support icons.
		public override void Icon_FromResource()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Icon = "/Resources/Images/Completed.png";
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;

			// This component does not support icons.
			// We should have an element, but the class should not be set.
			Assert.IsNotNull(element);
			Assert.AreEqual("table-wrapper", element.Attributes["class"]?.Value ?? "");
			Assert.IsFalse
			(
				(element.Attributes["style"]?.Value ?? "").Contains("background-image:url(data:image/png;base64")
			);
		}

	}
}
