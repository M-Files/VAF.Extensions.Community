using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MFiles.VAF.Extensions.Tests.Directives
{
	[TestClass]
	public class ObjIDTaskDirectiveTests
		: TaskDirectiveWithDisplayNameTestsBase<ObjIDTaskDirective>
	{
		[TestMethod]
		public void ObjectIDIsReadWrite()
		{
			var type = typeof(ObjIDTaskDirective);
			var property = type.GetProperty(nameof(ObjIDTaskDirective.ObjectID));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}
		[TestMethod]
		public void ObjectTypeIDIsReadWrite()
		{
			var type = typeof(ObjIDTaskDirective);
			var property = type.GetProperty(nameof(ObjIDTaskDirective.ObjectTypeID));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void FromObjIDThrowsIfArgumentNull()
		{
			((ObjID)null).ToObjIDTaskDirective();
		}

		[TestMethod]
		public void FromObjIDCorrectData()
		{
			var objID = new ObjID();
			objID.SetIDs(101, 2);
			var directive = objID.ToObjIDTaskDirective();
			Assert.AreEqual(101, directive.ObjectTypeID);
			Assert.AreEqual(2, directive.ObjectID);
		}
	}
}
