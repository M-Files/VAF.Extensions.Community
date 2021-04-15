using MFiles.VAF.Configuration.Domain.Dashboards;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	public class DashboardProgressBar
		: DashboardContentBase
	{
		/// <summary>
		/// The current state of the task being represented by this progress bar.
		/// </summary>
		public MFTaskState? TaskState { get; set; }

		///// <summary>
		///// Commands (links/buttons) to show.
		///// </summary>
		//public List<DashboardCommand> Commands { get; }
		//	= new List<DashboardCommand>();

		/// <summary>
		/// The text to show in the progress bar.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The percentage complete.
		/// TODO: If no value will show an indeterminate progress bar.
		/// </summary>
		public int? PercentageComplete { get; set; }

		/// <inheritdoc />
		protected override XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml)
		{
			// What should the colour of the progress bar be?
			var backgroundColour = "green";
			var numberColour = "#666";
			if (this.TaskState.HasValue &&
				this.TaskState.Value == MFTaskState.MFTaskStateFailed
				)
			{
				backgroundColour = "red";
				numberColour = "red";
			}

			// Create the basic structure of the progress bar.
			XmlDocumentFragment fragment = null;
			if (this.PercentageComplete.HasValue)
			{
				// We have a percentage complete, so render a progress bar.
				fragment = DashboardHelper.CreateFragment(xml,
					  $"<div class='progress-bar' style='min-width: 200px; background-color: white; position: relative; width: 100%; height: 23px; border: 1px solid #CCC; padding: 2px 5px 5px 5px; border-radius: 2px; box-sizing: border-box; overflow: hidden; font-size: 10px;'>"
						+ $"<div style='position: absolute; top: 0px; left: 0px; bottom: 0px; width: 30px; padding: 2px 5px; text-align: center; background-color: #CCC; color: {numberColour}'>{this.PercentageComplete.Value}%</div>"
						+ $"<div class='number' style='position: absolute; width: {this.PercentageComplete.Value}%; bottom: 0px; background-color: {backgroundColour}; left: 0px; height: 4px'></div>"
						+ $"<div class='text' style='position: absolute; top: 0px; bottom: 5px; left: 40px; right: 0px; white-space: nowrap; overflow: hidden; padding: 2px 5px; text-overflow: ellipsis; color: {numberColour}'></div>"
					+ "</div>");
				XmlElement progressBarElement = (XmlElement)fragment.SelectNodes("div[@class='progress-bar']")[0];
				progressBarElement.SetAttribute("title", this.Text);
				XmlElement text = (XmlElement)progressBarElement.SelectNodes("*[@class=\"text\"]")[0];
				text.InnerText = this.Text;
			}
			else
			{
				// Render an indeterminate progress bar.
				fragment = DashboardHelper.CreateFragment(xml,
					  $"<div class='progress-bar' style='min-width: 200px; position: relative; width: 100%; height: 20px; border: 1px solid #CCC; padding: 2px 5px; border-radius: 2px; box-sizing: border-box; overflow: hidden; font-size: 10px;  background-color: {backgroundColour};  background-repeat: repeat; background-position: center center; background-size: 40px 40px; background-image: linear-gradient(45deg,rgba(255,255,255,.15) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.15) 50%,rgba(255,255,255,.15) 75%,transparent 75%,transparent); color: white;text-overflow: ellipsis; animation: progress-bar-stripes 1s linear infinite;'></div>");
				((XmlElement)fragment.FirstChild).SetAttribute("title", this.Text);
				fragment.FirstChild.InnerText = this.Text;
			}

			// Get a handle on the various elements.
			XmlElement progressBar = (XmlElement)fragment.SelectNodes("div[@class=\"progress-bar\"]")[0];

			//// Append any commands defined for the item.
			//if (this.Commands != null)
			//{
			//	foreach (DashboardCommand cmd in this.Commands)
			//		cmdBar.AppendChild(cmd.Generate(xml));
			//}

			return fragment;
		}
	}
}
