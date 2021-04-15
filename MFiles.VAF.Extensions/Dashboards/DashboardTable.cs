using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	/// <summary>
	/// Types of row that a table can support.
	/// </summary>
	public enum DashboardTableRowType
	{
		Body = 0,
		Header = 1,
		Footer = 2
	}

	/// <summary>
	/// Represents a row in a table.
	/// </summary>
	public class DashboardTableRow
		: DashboardContentBase
	{
		/// <summary>
		/// Commands (links/buttons) to show.
		/// </summary>
		public List<DashboardCommand> Commands { get; }
			= new List<DashboardCommand>();

		/// <summary>
		/// Cells in this row.
		/// </summary>
		public List<DashboardTableCell> Cells { get; }
			= new List<DashboardTableCell>();

		/// <summary>
		/// The type of the row.
		/// </summary>
		public DashboardTableRowType DashboardTableRowType { get; set; }
			= DashboardTableRowType.Body;

		/// <summary>
		/// Adds a cell to <see cref="Cells"/> and returns it.
		/// </summary>
		/// <returns>The new cell.</returns>
		public DashboardTableCell AddCell
		(
			IDashboardContent content = null,
			DashboardTableCellType? type = null
		)
		{
			// Create the cell and set the type depending on whether we're in a header row.
			var cell = new DashboardTableCell()
			{
				DashboardTableCellType = type.HasValue
				? type.Value
				: this.DashboardTableRowType == DashboardTableRowType.Header
					? DashboardTableCellType.Header
					: DashboardTableCellType.Standard,
				InnerContent = content
			};
			this.Cells.Add(cell);
			return cell;
		}

		/// <summary>
		/// Adds a cell to <see cref="Cells"/> and returns it.
		/// </summary>
		/// <returns>The new cell.</returns>
		public DashboardTableCell AddCell
		(
			string content,
			DashboardTableCellType? type = null
		)
		{
			return this.AddCell
			(
				new DashboardCustomContent(content),
				type
			);
		}

		public List<DashboardTableCell> AddCells(params IDashboardContent[] cellContent)
		{
			// Sanity.
			var cells = new List<DashboardTableCell>();
			if (null == cellContent)
				return cells;

			// Add cells in turn.
			foreach (var content in cellContent)
				cells.Add(this.AddCell(content: content));

			// Return the cells.
			return cells;
		}

		public DashboardTableRow(DashboardTableRowType dashboardTableRowType)
		{
			this.DashboardTableRowType = dashboardTableRowType;
		}

		/// <inheritdoc />
		protected override XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml)
		{
			var fragment = DashboardHelper.CreateFragment(xml, "<tr></tr>");
			var element = fragment.FirstChild;

			foreach (var cell in this.Cells)
			{
				element.AppendChild(cell.Generate(xml));
			}

			return fragment;
		}
	}

	/// <summary>
	/// Types of dashboard cell.
	/// </summary>
	public enum DashboardTableCellType
	{
		/// <summary>
		/// A standard table cell ("td").
		/// </summary>
		Standard = 0,

		/// <summary>
		/// A table cell header ("th").
		/// </summary>
		Header = 1
	}

	/// <summary>
	/// Represents a cell in a table.
	/// </summary>
	public class DashboardTableCell
		: DashboardContentBase
	{
		/// <summary>
		/// The type of cell (header/standard).
		/// </summary>
		public DashboardTableCellType DashboardTableCellType { get; set; }
			= DashboardTableCellType.Standard;

		/// <summary>
		/// The content to show in the cell.
		/// </summary>
		public IDashboardContent InnerContent { get; set; }

		/// <summary>
		/// CSS styles only applied to the header cells.  Keys are the names (e.g. "font-size"), values are the value (e.g. "12px").
		/// </summary>
		public Dictionary<string, string> HeaderStyles { get; }
			= new Dictionary<string, string>();

		public DashboardTableCell()
		{
			this.Styles.Add("font-size", "12px");
			this.Styles.Add("padding", "2px 3px");
			this.Styles.Add("text-align", "left");
			this.HeaderStyles.Add("border-bottom", "1px solid #CCC");
		}

		protected override string GetCssStyles()
		{
			// Add the style.
			var styles = this.Styles.AsEnumerable();
			if (this.DashboardTableCellType == DashboardTableCellType.Header)
			{
				styles = styles.Union(this.HeaderStyles);
			}
			return string.Join(";", styles.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
		}

		/// <inheritdoc />
		protected override XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml)
		{
			var elementName = this.DashboardTableCellType == DashboardTableCellType.Header
				? "th"
				: "td";
			var fragment = DashboardHelper.CreateFragment(xml, $"<{elementName}></{elementName}>");
			var element = fragment.FirstChild;

			// Add the content.
			element.AppendChild
			(
				this.InnerContent?.Generate(xml) ?? DashboardHelper.CreateFragment(xml, "&nbsp;")
			);

			return fragment;
		}
	}
	public class DashboardTable
		: DashboardContentBase
	{

		/// <summary>
		/// Commands (links/buttons) to show.
		/// </summary>
		public List<DashboardCommand> Commands { get; }
			= new List<DashboardCommand>();

		/// <summary>
		/// Thr rows in the table.
		/// </summary>
		public List<DashboardTableRow> Rows { get; }
			= new List<DashboardTableRow>();

		public DashboardTable()
		{
			this.Styles.Add("width", "100%");
			this.Styles.Add("background-color", "white");
			this.Styles.Add("border", "1px solid #CCC");
			this.Attributes.Add("cellspacing", "0");
			this.Attributes.Add("cellpadding", "0");
		}

		/// <summary>
		/// Adds a row to <see cref="Rows"/> and returns it.
		/// </summary>
		/// <returns>The new row.</returns>
		public DashboardTableRow AddRow(DashboardTableRowType type = DashboardTableRowType.Body)
		{
			// Create the row.
			var row = new DashboardTableRow(type);
			this.Rows.Add(row);
			return row;
		}

		/// <inheritdoc />
		public override XmlDocumentFragment Generate(XmlDocument xml)
		{
			var fragment = this.GenerateXmlDocumentFragment(xml);
			// Get a handle on the various elements.
			XmlElement tableWrapper = (XmlElement)fragment.SelectNodes("div[@class=\"table-wrapper\"]")[0];
			XmlElement table = (XmlElement)fragment.SelectNodes("//table")[0];
			XmlElement titleBar = (XmlElement)tableWrapper.SelectNodes("*[@class=\"title-bar\"]")[0];
			XmlElement cmdBar = (XmlElement)titleBar.SelectNodes("*[@class=\"command-bar\"]")[0];

			// Append any commands defined for the item.
			if (this.Commands != null)
			{
				foreach (DashboardCommand cmd in this.Commands)
					cmdBar.AppendChild(cmd.Generate(xml));
			}

			// Add the header rows.
			var headerRows = this
				.Rows
				.Where(r => r.DashboardTableRowType == DashboardTableRowType.Header)
				.ToList();
			if (headerRows.Count > 0)
			{
				var headerFragment = DashboardHelper.CreateFragment(xml, "<thead></thead>").FirstChild;
				foreach (var row in headerRows)
				{
					headerFragment.AppendChild(row.Generate(xml));
				}
				table.AppendChild(headerFragment);
			}

			// Add the footer rows.
			var footerRows = this
				.Rows
				.Where(r => r.DashboardTableRowType == DashboardTableRowType.Footer)
				.ToList();
			if (footerRows.Count > 0)
			{
				var footerFragment = DashboardHelper.CreateFragment(xml, "<tfoot></tfoot>").FirstChild;
				foreach (var row in footerRows)
				{
					footerFragment.AppendChild(row.Generate(xml));
				}
				table.AppendChild(footerFragment);
			}

			// Add the body rows.
			var bodyRows = this
				.Rows
				.Where(r => r.DashboardTableRowType == DashboardTableRowType.Body)
				.ToList();
			if (bodyRows.Count > 0)
			{
				var bodyFragment = DashboardHelper.CreateFragment(xml, "<tbody></tbody>").FirstChild;
				foreach (var row in bodyRows)
				{
					bodyFragment.AppendChild(row.Generate(xml));
				}
				table.AppendChild(bodyFragment);
			}

			// Add the id if defined.
			if (!String.IsNullOrWhiteSpace(this.ID))
				table.SetAttribute("id", this.ID);

			// Add the attributes.
			foreach (var key in this.Attributes.Keys)
			{
				// Can't have style here.
				if (key == "style")
					continue;
				var attr = xml.CreateAttribute(key);
				attr.Value = this.Attributes[key];
				table.Attributes.Append(attr);
			}

			// Add the style.
			{
				var attr = xml.CreateAttribute("style");
				attr.Value = $"{this.GetCssStyles() ?? ""} {table.GetAttribute("style") ?? ""}";
				table.Attributes.Append(attr);
			}

			return fragment;
		}

		/// <inheritdoc />
		protected override XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml)
		{
			// Create the basic structure of the table.
			return  DashboardHelper.CreateFragment(xml,
					"<div class='table-wrapper' style='max-height: 200px; overflow-y: auto;'>"
						+ "<div class='title-bar'>"
							+ "<span class='command-bar'></span>"
						+ "</div>"
						+ "<table>"
						+ "</table>" 
					+ "</div>");
		}
	}
}