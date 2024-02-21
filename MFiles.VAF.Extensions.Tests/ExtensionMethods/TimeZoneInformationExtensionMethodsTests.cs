using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods
{
	[TestClass]
	public class TimeZoneInformationExtensionMethodsTests
	{
		[TestMethod]
		[DynamicData(nameof(GetEnsureLocalTimeData), DynamicDataSourceType.Method)]
		public void EnsureLocalTime
		(
			string timezoneName,
			DateTime input,
			DateTime expected
		)
		{
			var tzMock = new Mock<TimeZoneInformation>();
			tzMock.Setup(m => m.StandardName).Returns(timezoneName);

			var output = tzMock.Object.EnsureLocalTime(input);
			Assert.AreEqual(expected.ToString(), output.ToString());
			Assert.AreEqual(expected.Kind, output.Kind);
		}

		public static IEnumerable<object[]> GetEnsureLocalTimeData()
		{
			yield return new object[]
			{
				"GMT Standard Time",
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Utc),
				new DateTime(2023, 08, 01, 11, 00, 00, DateTimeKind.Local)
			};
			yield return new object[]
			{
				"GMT Standard Time",
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Local),
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Local),
			};
			yield return new object[]
			{
				"GMT Standard Time",
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Unspecified),
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Unspecified),
			};
		}

		[TestMethod]
		[DynamicData(nameof(GetEnsureUtcTimeData), DynamicDataSourceType.Method)]
		public void EnsureUtcTime
		(
			string timezoneName,
			DateTime input,
			DateTime expected
		)
		{
			var tzMock = new Mock<TimeZoneInformation>();
			tzMock.Setup(m => m.StandardName).Returns(timezoneName);

			var output = tzMock.Object.EnsureUTCTime(input);
			Assert.AreEqual(expected.ToString(), output.ToString());
			Assert.AreEqual(expected.Kind, output.Kind);
		}

		public static IEnumerable<object[]> GetEnsureUtcTimeData()
		{
			yield return new object[]
			{
				"GMT Standard Time",
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Local),
				new DateTime(2023, 08, 01, 09, 00, 00, DateTimeKind.Utc)
			};
			yield return new object[]
			{
				"GMT Standard Time",
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Utc),
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Utc)
			};
			yield return new object[]
			{
				"GMT Standard Time",
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Unspecified),
				new DateTime(2023, 08, 01, 10, 00, 00, DateTimeKind.Unspecified)
			};
		}
	}
}
