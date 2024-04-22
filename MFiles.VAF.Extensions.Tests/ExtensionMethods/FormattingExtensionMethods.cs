using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods
{
	[TestClass]
	public class FormattingExtensionMethods
	{
		public FormattingExtensionMethods()
		{
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
		}

		[TestMethod]
		public void ToDashboardDisplayString_HoursMinutesSeconds()
		{
			var interval = new TimeSpan(1, 30, 32);
			Assert.AreEqual
			(
				"<p>Runs on vault startup and every 1 hour, 30 minutes, and 32 seconds.<br /></p>",
				interval.ToDashboardDisplayString()
			);
		}

		[TestMethod]
		public void ToDashboardDisplayString_RunOnStartup_False()
		{
			var interval = new TimeSpanEx(new TimeSpan(1, 30, 32))
			{
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
			var interval = new TimeSpanEx(new TimeSpan(1, 30, 32))
			{
				RunOnVaultStartup = true
			};
			Assert.AreEqual
			(
				"<p>Runs on vault startup and every 1 hour, 30 minutes, and 32 seconds.<br /></p>",
				interval.ToDashboardDisplayString()
			);
		}

		[TestMethod]
		public void ToDisplayString_2hours_0minutes_23seconds()
		{
			TimeSpan? interval = new TimeSpan(2, 0, 23);
			Assert.AreEqual
			(
				"2 hours, and 23 seconds",
				interval.ToDisplayString()
			);
		}

		[TestMethod]
		public void ToDisplayString_2hours_1minute_23seconds()
		{
			TimeSpan? interval = new TimeSpan(2, 1, 23);
			Assert.AreEqual
			(
				"2 hours, 1 minute, and 23 seconds",
				interval.ToDisplayString()
			);
		}

		[TestMethod]
		public void ToDisplayString_2hours_10minutes_23seconds()
		{
			TimeSpan? interval = new TimeSpan(2, 10, 23);
			Assert.AreEqual
			(
				"2 hours, 10 minutes, and 23 seconds",
				interval.ToDisplayString()
			);
		}

		[TestMethod]
		public void ToDisplayString_2days_2hours_10minutes_23seconds()
		{
			TimeSpan? interval = new TimeSpan(2, 2, 10, 23);
			Assert.AreEqual
			(
				"2 days, 2 hours, 10 minutes, and 23 seconds",
				interval.ToDisplayString()
			);
		}
	}
}
