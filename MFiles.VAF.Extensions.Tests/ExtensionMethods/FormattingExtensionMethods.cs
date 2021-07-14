using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods
{
	[TestClass]
	public class FormattingExtensionMethods
	{
		[TestMethod]
		public void ToDashboardDisplayString_HoursMinutesSeconds()
		{
			var interval = new TimeSpan(1, 30, 32);
			Assert.AreEqual
			(
				"<p>Runs every 1 hour, 30 minutes, and 32 seconds.<br /></p>", 
				interval.ToDashboardDisplayString()
			);
		}
	}
}
