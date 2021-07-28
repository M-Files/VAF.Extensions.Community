using MFiles.VAF.Common;
using MFiles.VAF.Extensions;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MFiles.VAF.Extensions.Tests.Directives
{
	[TestClass]
	public class ObjVerExTaskDirectiveTests
		: TaskDirectiveWithDisplayNameTestsBase<ObjVerExTaskDirective>
	{
		[TestMethod]
		public void ObjVerExIsReadWrite()
		{
			var type = typeof(ObjVerExTaskDirective);
			var property = type.GetProperty(nameof(ObjVerExTaskDirective.ObjVerEx));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void FromObjVerThrowsIfArgumentNull()
		{
			((ObjVer)null).ToObjVerExTaskDirective();
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void FromObjVerExThrowsIfArgumentNull()
		{
			((ObjVerEx)null).ToObjVerExTaskDirective();
		}

		[TestMethod]
		public void FromObjVerCorrectString()
		{
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 2);
			Assert.AreEqual
			(
				"(0-1-2)",
				objVer.ToObjVerExTaskDirective().ObjVerEx
			);
		}
	}
}
