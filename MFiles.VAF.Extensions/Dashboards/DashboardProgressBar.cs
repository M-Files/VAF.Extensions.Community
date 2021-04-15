using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	public class DashboardProgressBar : IDashboardContent
	{
		/// <summary>
		/// The id of the component . Optional.
		/// Will appear in the html output, so the item can be referenced.
		/// </summary>
		public string ID { get; set; }

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

		/// <summary>
		/// Attributes to be rendered onto the table cell.
		/// </summary>
		public Dictionary<string, string> Attributes { get; }
			= new Dictionary<string, string>();

		/// <summary>
		/// CSS styles.  Keys are the names (e.g. "font-size"), values are the value (e.g. "12px").
		/// </summary>
		public Dictionary<string, string> Styles { get; }
			= new Dictionary<string, string>();

		/// <inheritdoc />
		public XmlDocumentFragment Generate(XmlDocument xml)
		{
			// Create the basic structure of the progress bar.
			XmlDocumentFragment fragment = null;
			if (this.PercentageComplete.HasValue)
			{
				// We have a percentage complete, so render a progress bar.
				fragment = DashboardHelper.CreateFragment(xml,
					  $"<div class='progress-bar' style='min-width: 200px; background-color: white; position: relative; width: 100%; height: 23px; border: 1px solid #CCC; padding: 2px 5px 5px 5px; border-radius: 2px; box-sizing: border-box; overflow: hidden; font-size: 10px;'>"
						+ $"<div style='position: absolute; top: 0px; left: 0px; bottom: 0px; width: 30px; padding: 2px 5px; text-align: center; background-color: #CCC; color: #666'>{this.PercentageComplete.Value}%</div>"
						+ $"<div style='position: absolute; width: {this.PercentageComplete.Value}%; bottom: 0px; background-color: green; left: 0px; height: 4px'></div>"
						+ $"<div class='text' style='position: absolute; top: 0px; bottom: 5px; left: 40px; right: 0px; white-space: nowrap; overflow: hidden; padding: 2px 5px; text-overflow: ellipsis'>{this.Text}</div>"
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
					  $"<div class='progress-bar' style='min-width: 200px; position: relative; width: 100%; height: 20px; border: 1px solid #CCC; padding: 2px 5px; border-radius: 2px; box-sizing: border-box; overflow: hidden; font-size: 10px;  background-color: green;  background-repeat: repeat; background-position: center center; background-size: 40px 40px; background-image: linear-gradient(45deg,rgba(255,255,255,.15) 25%,transparent 25%,transparent 50%,rgba(255,255,255,.15) 50%,rgba(255,255,255,.15) 75%,transparent 75%,transparent); color: white;text-overflow: ellipsis; animation: progress-bar-stripes 1s linear infinite;'></div>");
				((XmlElement)fragment.FirstChild).SetAttribute("title", this.Text);
				fragment.FirstChild.InnerText = this.Text;
			}

			// Get a handle on the various elements.
			XmlElement progressBar = (XmlElement)fragment.SelectNodes("div[@class=\"progress-bar\"]")[0];
			//XmlElement cmdBar = (XmlElement)progressBar.SelectNodes("*[@class=\"command-bar\"]")[0];

			// Add the id if defined.
			if (!String.IsNullOrWhiteSpace(this.ID))
				progressBar.SetAttribute("id", this.ID);

			//// Append any commands defined for the item.
			//if (this.Commands != null)
			//{
			//	foreach (DashboardCommand cmd in this.Commands)
			//		cmdBar.AppendChild(cmd.Generate(xml));
			//}

			// Add the attributes.
			foreach (var key in this.Attributes.Keys)
			{
				// Can't have style here.
				if (key == "style")
					continue;
				var attr = xml.CreateAttribute(key);
				attr.Value = this.Attributes[key];
				progressBar.Attributes.Append(attr);
			}

			// Add the style.
			if (this.Styles.Count > 0)
			{
				var attr = xml.CreateAttribute("style");
				attr.Value = string.Join(";", this.Styles.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
				progressBar.Attributes.Append(attr);
			}

			return fragment;
		}
	}
}
