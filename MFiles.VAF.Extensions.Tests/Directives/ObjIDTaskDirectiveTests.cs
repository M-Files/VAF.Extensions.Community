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

		[TestMethod]
		public void TryGetObjID_InvalidID_Zero()
		{
			var directive = new ObjIDTaskDirective()
			{
				ObjectTypeID = 101,
				ObjectID = 0
			};
			Assert.IsFalse(directive.TryGetObjID(out _));
		}

		[TestMethod]
		public void TryGetObjID_InvalidID_Negative()
		{
			var directive = new ObjIDTaskDirective()
			{
				ObjectTypeID = 101,
				ObjectID = -1
			};
			Assert.IsFalse(directive.TryGetObjID(out _));
		}

		[TestMethod]
		public void TryGetObjID_InvalidType()
		{
			var directive = new ObjIDTaskDirective()
			{
				ObjectTypeID = -1,
				ObjectID = 123
			};
			Assert.IsFalse(directive.TryGetObjID(out _));
		}

		[TestMethod]
		public void TryGetObjID_CorrectData()
		{
			var objID = new ObjID();
			objID.SetIDs(101, 2);
			var directive = objID.ToObjIDTaskDirective();

			Assert.IsTrue(directive.TryGetObjID(out ObjID o));
			Assert.AreEqual(objID.ID, o.ID);
			Assert.AreEqual(objID.Type, o.Type);

		}

		[TestMethod]
		public void TryGetObjID_CorrectData_ZeroObjectType()
		{
			var objID = new ObjID();
			objID.SetIDs(0, 2);
			var directive = objID.ToObjIDTaskDirective();

			Assert.IsTrue(directive.TryGetObjID(out ObjID o));
			Assert.AreEqual(objID.ID, o.ID);
			Assert.AreEqual(objID.Type, o.Type);

		}
	}
}
