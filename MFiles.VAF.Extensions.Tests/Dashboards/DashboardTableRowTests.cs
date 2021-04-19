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
	public class DashboardTableRowTests
	{
		[TestMethod]
		public void EmptyCellsByDefault()
		{
			Assert.AreEqual(0, new DashboardTableRow().Cells.Count);
		}

		[TestMethod]
		public void EmptyCommandsByDefault()
		{
			Assert.AreEqual(0, new DashboardTableRow().Commands.Count);
		}

		[TestMethod]
		public void BodyByDefault()
		{
			Assert.AreEqual(DashboardTableRowType.Body, new DashboardTableRow().DashboardTableRowType);
		}

		[TestMethod]
		public void GeneratesEmptyRowByDefault()
		{
			var row = new DashboardTableRow();
			var element = row.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("tr", element.LocalName);
			Assert.AreEqual(0, element.ChildNodes.Count);
		}

		[TestMethod]
		public void AddCellIncrementsCellCount()
		{
			var row = new DashboardTableRow();
			var cell = row.AddCell("hello");
			Assert.IsNotNull(cell);
			Assert.AreEqual(1, row.Cells.Count);
			Assert.AreEqual(cell, row.Cells[0]);
		}

		[TestMethod]
		public void AddCellGeneratesRowWithOneCell()
		{
			var row = new DashboardTableRow();
			row.AddCell("hello");
			var element = row.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("tr", element.LocalName);
			Assert.AreEqual(1, element.ChildNodes.Count);
		}

		[TestMethod]
		public void AddCellsIncrementsCellCount()
		{
			var row = new DashboardTableRow();
			var cellList = row.AddCells("hello", "world", "new", "cells");
			Assert.IsNotNull(cellList);
			Assert.AreEqual(4, cellList.Count);
			Assert.AreEqual(4, row.Cells.Count);
		}

		[TestMethod]
		public void AddCellsGeneratesRowWithAppropriateCells()
		{
			var row = new DashboardTableRow();
			row.AddCells("hello", "world", "new", "cells");
			var element = row.ToXmlFragment()?.FirstChild;
			Assert.IsNotNull(element);
			Assert.AreEqual("tr", element.LocalName);
			Assert.AreEqual(4, element.ChildNodes.Count);
		}
	}
}
