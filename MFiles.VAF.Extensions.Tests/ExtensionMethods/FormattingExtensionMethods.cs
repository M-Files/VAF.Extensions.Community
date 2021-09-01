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

		[TestMethod]
		public void ToDashboardDisplayString_RunOnStartup_False()
		{
			var interval = new TimeSpanEx()
			{
				Interval = new TimeSpan(1, 30, 32),
				RunOnVaultStartup = false
			};
			Assert.AreEqual
			(
				"<p>Runs every 1 hour, 30 minutes, and 32 seconds.<br /></p>",
				interval.ToDashboardDisplayString()
			);
		}

		[TestMethod]
		public void ToDashboardDisplayString_RunOnStartup_True()
		{
			var interval = new TimeSpanEx()
			{
				Interval = new TimeSpan(1, 30, 32),
				RunOnVaultStartup = true
			};
			Assert.AreEqual
			(
				"<p>Runs on vault startup and every 1 hour, 30 minutes, and 32 seconds.<br /></p>",
				interval.ToDashboardDisplayString()
			);
		}
	}
}
