using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods
{
	internal static class AssertExtensionMethods
	{
		public static void AreEqualJson(this Assert assert, string expected, string actual)
		{
			if (null == expected)
			{
				Assert.IsNull(actual);
				return;
			}
			Assert.AreEqual
			(
				JObject.Parse(expected).ToString(),
				JObject.Parse(actual).ToString()
			);
		}
		public static void AreEqualJson(this Assert assert, JObject expected, JObject actual)
		{
			if (null == expected)
			{
				Assert.IsNull(actual);
				return;
			}
			Assert.AreEqual
			(
				expected.ToString(),
				actual.ToString()
			);
		}
	}
}
