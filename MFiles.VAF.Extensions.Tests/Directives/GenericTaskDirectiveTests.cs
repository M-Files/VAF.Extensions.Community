using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions.Tests.Directives
{
	[TestClass]
	public class GenericTaskDirectiveT1Tests
		: TaskDirectiveWithDisplayNameTestsBase<GenericTaskDirective<int>>
	{
		[TestMethod]
		public void Item1IsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int>.Item1));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void Item1_HasDataMemberAttribute()
		{
			this.AssertPropertyHasDataMemberAttribute(nameof(GenericTaskDirective<int>.Item1));
		}

		[TestMethod]
		public void GenericDirective1CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int>(1);
			Assert.AreEqual(1, genericDirective.Item1);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string>("value1");
			Assert.AreEqual("value1", genericDirective.Item1);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataList()
		{
			var genericDirective = new GenericTaskDirective<IList<int>>(new List<int> { 1, 2, 3 });
			Assert.AreEqual(1, genericDirective.Item1[0]);
			Assert.AreEqual(2, genericDirective.Item1[1]);
			Assert.AreEqual(3, genericDirective.Item1[2]);
		}
	}
	[TestClass]
	public class GenericTaskDirectiveT2Tests
		: TaskDirectiveWithDisplayNameTestsBase<GenericTaskDirective<int, int>>
	{
		[TestMethod]
		public void Item1IsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int>.Item1));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void Item1_HasDataMemberAttribute()
		{
			this.AssertPropertyHasDataMemberAttribute(nameof(GenericTaskDirective<int>.Item1));
		}

		[TestMethod]
		public void GenericDirective1CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int>(1);
			Assert.AreEqual(1, genericDirective.Item1);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string>("item1");
			Assert.AreEqual("item1", genericDirective.Item1);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataList()
		{
			var genericDirective = new GenericTaskDirective<IList<int>>(new List<int> { 1, 2, 3 });
			Assert.AreEqual(1, genericDirective.Item1[0]);
			Assert.AreEqual(2, genericDirective.Item1[1]);
			Assert.AreEqual(3, genericDirective.Item1[2]);
		}

		[TestMethod]
		public void Item2IsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int, int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int, int>.Item2));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void Item2_HasDataMemberAttribute()
		{
			this.AssertPropertyHasDataMemberAttribute(nameof(GenericTaskDirective<int, int>.Item2));
		}

		[TestMethod]
		public void GenericDirective2CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int, int>(1, 2);
			Assert.AreEqual(1, genericDirective.Item1);
			Assert.AreEqual(2, genericDirective.Item2);
		}

		[TestMethod]
		public void GenericDirective2CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string, string>("value1", "value2");
			Assert.AreEqual("value1", genericDirective.Item1);
			Assert.AreEqual("value2", genericDirective.Item2);
		}
	}
	[TestClass]
	public class GenericTaskDirectiveT3Tests
		: TaskDirectiveWithDisplayNameTestsBase<GenericTaskDirective<int, int, int>>
	{
		[TestMethod]
		public void Item1IsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int>.Item1));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void Item1_HasDataMemberAttribute()
		{
			this.AssertPropertyHasDataMemberAttribute(nameof(GenericTaskDirective<int>.Item1));
		}

		[TestMethod]
		public void GenericDirective1CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int>(1);
			Assert.AreEqual(1, genericDirective.Item1);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string>("value1");
			Assert.AreEqual("value1", genericDirective.Item1);
		}

		[TestMethod]
		public void GenericDirective1CorrectDataList()
		{
			var genericDirective = new GenericTaskDirective<IList<int>>(new List<int> { 1, 2, 3 });
			Assert.AreEqual(1, genericDirective.Item1[0]);
			Assert.AreEqual(2, genericDirective.Item1[1]);
			Assert.AreEqual(3, genericDirective.Item1[2]);
		}

		[TestMethod]
		public void Item2_HasDataMemberAttribute()
		{
			this.AssertPropertyHasDataMemberAttribute(nameof(GenericTaskDirective<int, int>.Item2));
		}

		[TestMethod]
		public void GenericDirective2CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int, int>(1, 2);
			Assert.AreEqual(1, genericDirective.Item1);
			Assert.AreEqual(2, genericDirective.Item2);
		}

		[TestMethod]
		public void GenericDirective2CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string, string>("value1", "value2");
			Assert.AreEqual("value1", genericDirective.Item1);
			Assert.AreEqual("value2", genericDirective.Item2);
		}

		[TestMethod]
		public void Item3IsReadWrite()
		{
			var type = typeof(GenericTaskDirective<int, int, int>);
			var property = type.GetProperty(nameof(GenericTaskDirective<int, int, int>.Item3));
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void Item3_HasDataMemberAttribute()
		{
			this.AssertPropertyHasDataMemberAttribute(nameof(GenericTaskDirective<int, int, int>.Item3));
		}

		[TestMethod]
		public void GenericDirective3CorrectDataInt()
		{
			var genericDirective = new GenericTaskDirective<int, int, int>(1, 2, 3);
			Assert.AreEqual(1, genericDirective.Item1);
			Assert.AreEqual(2, genericDirective.Item2);
			Assert.AreEqual(3, genericDirective.Item3);
		}

		[TestMethod]
		public void GenericDirective3CorrectDataString()
		{
			var genericDirective = new GenericTaskDirective<string, string, string>("value1", "value2", "value3");
			Assert.AreEqual("value1", genericDirective.Item1);
			Assert.AreEqual("value2", genericDirective.Item2);
			Assert.AreEqual("value3", genericDirective.Item3);
		}
	}
}
