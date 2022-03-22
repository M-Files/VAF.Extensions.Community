using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Tests.Directives
{
	[TestClass]
	public class GenericTaskDirectiveTests
		: TaskDirectiveWithDisplayNameTestsBase<ObjIDTaskDirective>
	{
		[TestMethod]
		public void ValueIsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int>.Value));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void Value2IsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int, int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int, int>.Value2));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void Value3IsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int, int, int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int, int, int>.Value3));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int>(1);
			Assert.AreEqual(1, genericDirective.Value);
		}

		[TestMethod]
		public void GenericDirective2CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int, int>(1, 2);
			Assert.AreEqual(1, genericDirective.Value);
			Assert.AreEqual(2, genericDirective.Value2);
		}

		[TestMethod]
		public void GenericDirective3CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int, int, int>(1, 2, 3);
			Assert.AreEqual(1, genericDirective.Value);
			Assert.AreEqual(2, genericDirective.Value2);
			Assert.AreEqual(3, genericDirective.Value3);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string>("value1");
			Assert.AreEqual("value1", genericDirective.Value);
		}

		[TestMethod]
		public void GenericDirective2CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string, string>("value1", "value2");
			Assert.AreEqual("value1", genericDirective.Value);
			Assert.AreEqual("value2", genericDirective.Value2);
		}

		[TestMethod]
		public void GenericDirective3CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string, string, string>("value1", "value2", "value3");
			Assert.AreEqual("value1", genericDirective.Value);
			Assert.AreEqual("value2", genericDirective.Value2);
			Assert.AreEqual("value3", genericDirective.Value3);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataList()
		{
			var genericDirective = new GenericTaskDirective<IList<int>>(new List<int> { 1, 2, 3 });
			Assert.AreEqual(1, genericDirective.Value[0]);
			Assert.AreEqual(2, genericDirective.Value[1]);
			Assert.AreEqual(3, genericDirective.Value[2]);
		}
	}
}
