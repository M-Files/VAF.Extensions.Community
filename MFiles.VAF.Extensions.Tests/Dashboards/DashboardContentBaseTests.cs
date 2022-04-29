using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.Dashboards
{
	/// <summary>
	/// Tests that <typeparamref name="T"/> does not break any standard
	/// <see cref="DashboardContentBase"/> functionality.
	/// </summary>
	/// <typeparam name="T">The type to test.</typeparam>
	public abstract class DashboardContentBaseTests<T>
		where T : DashboardContentBase
	{
		/// <summary>
		/// Returns a non-null instance of <typeparamref name="T"/>
		/// whose <see cref="IDashboardContent.Generate(System.Xml.XmlDocument)"/> method returns
		/// an item with at least one child.
		/// </summary>
		/// <returns>The instance</returns>
		public abstract T CreateDashboardContent();

		[TestMethod]
		public virtual void AttributeAdded()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Attributes.Add("hello", "world");
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("world", element.Attributes["hello"]?.Value);
		}

		[TestMethod]
		public virtual void StyleAdded()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Styles.AddOrUpdate("font-weight", "bold");
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;
			Assert.IsNotNull(element);
			Assert.IsTrue
			(
				new StyleComparisonHelper("font-weight: bold")
				.TestAgainstString(element.Attributes["style"]?.Value)
			);
		}

		[TestMethod]
		public virtual void IDAdded()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.ID = "myElement";
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("myElement", element.Attributes["id"]?.Value);
		}

		[TestMethod]
		public virtual void Icon_PathToFile()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Icon = "/some/file.png";
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("icon", element.Attributes["class"]?.Value);
			Assert.IsTrue
			(
				new StyleComparisonHelper("background-image:url('/some/file.png');background-repeat:no-repeat;background-position:0px center;padding-left:20px")
				.TestAgainstString(element.Attributes["style"]?.Value)
			);
		}

		[TestMethod]
		public virtual void Icon_FromResource()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Icon = "/Resources/Images/Completed.png";
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("icon", element.Attributes["class"]?.Value);
			Assert.IsTrue
			(
				new StyleComparisonHelper("background-image:url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAACsSURBVHgB7Y/NDYJAEEa/HXXlpFuCHWgJejMGEjoAOtAO7GTABjTKndiBHRhb8AKJcVFMSFQU3Tsv2WTn581kgAYjOjwbFe85R//KFk8HRO0EkDAeUMggmQidLy7B5mA0oJTv3ygLduF7nV6bHd9aOVzGisdKkFw/ZG+7/LRAVDZGdihAw64+T9JWnym/nlIvnuMLlRMyL/a1xj6l3hEaqJNrkWy7il2Fhp/cANlQMAnId1ieAAAAAElFTkSuQmCC);background-repeat:no-repeat;background-position:0px center;padding-left:20px")
				.TestAgainstString(element.Attributes["style"]?.Value)
			);
		}
	}
}
