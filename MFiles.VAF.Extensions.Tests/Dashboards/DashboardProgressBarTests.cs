using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Dashboards
{
	[TestClass]
	public class DashboardProgressBarTests
		: DashboardContentBaseTests<DashboardProgressBar>
	{
		[TestMethod]
		public void NoProgress_NotFailed_WithText()
		{
			var progressBar = new DashboardProgressBar()
			{
				PercentageComplete = null,
				Text = "Processing",
				TaskState = MFilesAPI.MFTaskState.MFTaskStateInProgress
			};
			var element = progressBar.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("progress-bar", element.Attributes["class"]?.Value);
			Assert.IsTrue
			(
				new StyleComparisonHelper( new Dictionary<string, string>
				{
					{ "min-width", "200px" },
					{ "position", "relative"},
					{ "width", "100%"},
					{ "height", "20px"},
					{ "border", "1px solid #CCC"},
					{ "padding", "2px 5px"},
					{ "border-radius", "2px"},
					{ "box-sizing", "border-box"},
					{ "overflow", "hidden"},
					{ "font-size", "10px"},
					{ "background-color", "green"},
					{ "background-repeat", "repeat"},
					{ "background-position", "center center"},
					{ "background-size", "40px 40px"},
					{ "background-image", "linear-gradient(45deg,rgba(255,255,255,.15) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.15) 50%,rgba(255,255,255,.15) 75%,transparent 75%,transparent)"},
					{ "color", "white"},
					{ "text-overflow", "ellipsis"},
					{ "animation", "progress-bar-stripes 1s linear infinite"},
				}).TestAgainstString(element.Attributes["style"]?.Value)
			);
			Assert.AreEqual("Processing", element.InnerText);
		}
		[TestMethod]
		public void Progress_NotFailed_WithText()
		{
			var expectedBackgroundColour = "green";
			var expectedNumberColour = "#666";
			var percentageComplete = 50;
			var progressBar = new DashboardProgressBar()
			{
				PercentageComplete = percentageComplete,
				Text = "Processing > This",
				TaskState = MFilesAPI.MFTaskState.MFTaskStateInProgress
			};

			// Get the top-level item.
			var output = progressBar.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(output);
			Assert.AreEqual("progress-bar", output.Attributes["class"]?.Value);
			Assert.AreEqual(progressBar.Text, output.Attributes["title"]?.Value);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"min-width: 200px; background-color: white; position: relative; width: 100%; height: 23px; border: 1px solid #CCC; padding: 2px 5px 5px 5px; border-radius: 2px; box-sizing: border-box; overflow: hidden; font-size: 10px;")
				.TestAgainstString(output.Attributes["style"]?.Value)
			);

			// Validate the number.
			var number = output.SelectSingleNode("div[@class='number']");
			Assert.IsNotNull(number);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"position: absolute; top: 0px; left: 0px; bottom: 0px; width: 30px; padding: 2px 5px; text-align: center; background-color: #EEE; color: {expectedNumberColour}")
				.TestAgainstString(number.Attributes["style"]?.Value)
			);
			Assert.AreEqual($"{progressBar.PercentageComplete.Value}%", number.InnerText);

			// Validate the bar.
			var bar = output.SelectSingleNode("div[@class='bar']");
			Assert.IsNotNull(bar);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"position: absolute; width: {progressBar.PercentageComplete.Value}%; bottom: 0px; background-color: {expectedBackgroundColour}; left: 0px; height: 4px")
				.TestAgainstString(bar.Attributes["style"]?.Value)
			);

			// Validate the text.
			var text = output.SelectSingleNode("div[@class='text']");
			Assert.IsNotNull(text);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"position: absolute; top: 0px; bottom: 5px; left: 40px; right: 0px; white-space: nowrap; overflow: hidden; padding: 2px 5px; text-overflow: ellipsis; color: {expectedNumberColour}")
				.TestAgainstString(text.Attributes["style"]?.Value)
			);
			Assert.AreEqual("Processing &gt; This", text.InnerXml); // Ensure encoded!
		}
		[TestMethod]
		public void Progress_Failed_WithText()
		{
			var expectedBackgroundColour = "red";
			var expectedNumberColour = "red";
			var percentageComplete = 70;
			var progressBar = new DashboardProgressBar()
			{
				PercentageComplete = percentageComplete,
				Text = "Exception processing item",
				TaskState = MFilesAPI.MFTaskState.MFTaskStateFailed
			};

			// Get the top-level item.
			var output = progressBar.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(output);
			Assert.AreEqual("progress-bar", output.Attributes["class"]?.Value);
			Assert.AreEqual(progressBar.Text, output.Attributes["title"]?.Value);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"min-width: 200px; background-color: white; position: relative; width: 100%; height: 23px; border: 1px solid #CCC; padding: 2px 5px 5px 5px; border-radius: 2px; box-sizing: border-box; overflow: hidden; font-size: 10px;")
				.TestAgainstString(output.Attributes["style"]?.Value)
			);

			// Validate the number.
			var number = output.SelectSingleNode("div[@class='number']");
			Assert.IsNotNull(number);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"position: absolute; top: 0px; left: 0px; bottom: 0px; width: 30px; padding: 2px 5px; text-align: center; background-color: #EEE; color: {expectedNumberColour}")
				.TestAgainstString(number.Attributes["style"]?.Value)
			);
			Assert.AreEqual($"{progressBar.PercentageComplete.Value}%", number.InnerText);

			// Validate the bar.
			var bar = output.SelectSingleNode("div[@class='bar']");
			Assert.IsNotNull(bar);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"position: absolute; width: {progressBar.PercentageComplete.Value}%; bottom: 0px; background-color: {expectedBackgroundColour}; left: 0px; height: 4px")
				.TestAgainstString(bar.Attributes["style"]?.Value)
			);

			// Validate the text.
			var text = output.SelectSingleNode("div[@class='text']");
			Assert.IsNotNull(text);
			Assert.IsTrue
			(
				new StyleComparisonHelper($"position: absolute; top: 0px; bottom: 5px; left: 40px; right: 0px; white-space: nowrap; overflow: hidden; padding: 2px 5px; text-overflow: ellipsis; color: {expectedNumberColour}")
				.TestAgainstString(text.Attributes["style"]?.Value)
			);
			Assert.AreEqual("Exception processing item", text.InnerXml); // Ensure encoded!
		}

		public override DashboardProgressBar CreateDashboardContent()
		{
			return new DashboardProgressBar();
		}

		[TestMethod]
		public override void Icon_PathToFile()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Icon = "/some/file.png";
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;

			// This component does not support icons.
			// We should have an element, but the class should not be set.
			Assert.IsNotNull(element);
			Assert.AreEqual("progress-bar", element.Attributes["class"]?.Value ?? "");
			Assert.IsFalse
			(
				(element.Attributes["style"]?.Value ?? "").Contains("background-image:url('")
			);
		}

		[TestMethod]
		public override void Icon_FromResource()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Icon = "/Resources/Completed.png";
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;

			// This component does not support icons.
			// We should have an element, but the class should not be set.
			Assert.IsNotNull(element);
			Assert.AreEqual("progress-bar", element.Attributes["class"]?.Value ?? "");
			Assert.IsFalse
			(
				(element.Attributes["style"]?.Value ?? "").Contains("background-image:url(data:image/png;base64")
			);
		}

		[TestMethod]
		public override void StyleAdded()
		{
			var dashboardContent = this.CreateDashboardContent();
			dashboardContent.Styles.Add("font-size", "12px");
			var element = dashboardContent.Generate(new System.Xml.XmlDocument())?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual
			(
				"font-size: 12px;min-width: 200px; position: relative; width: 100%; height: 20px; border: 1px solid #CCC; padding: 2px 5px; border-radius: 2px; box-sizing: border-box; overflow: hidden; font-size: 10px;  background-color: green;  background-repeat: repeat; background-position: center center; background-size: 40px 40px; background-image: linear-gradient(45deg,rgba(255,255,255,.15) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.15) 50%,rgba(255,255,255,.15) 75%,transparent 75%,transparent); color: white;text-overflow: ellipsis; animation: progress-bar-stripes 1s linear infinite", 
				element.Attributes["style"]?.Value
			);
		}

	}
}
