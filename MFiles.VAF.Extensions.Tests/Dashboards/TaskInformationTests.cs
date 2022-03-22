using MFiles.VAF.Configuration.Domain.Dashboards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Dashboards
{
	[TestClass]
	public class TaskInformationTests
	{
		[TestMethod]
		public void NoPercentage_UnencodedValueDoesNotThrow()
		{
			var x = new TaskInformation() { StatusDetails = "hello & world" };
			x.AsDashboardContent(true).ToXmlString();
		}
	}
}
