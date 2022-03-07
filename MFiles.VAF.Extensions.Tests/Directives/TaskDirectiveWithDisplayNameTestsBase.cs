using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MFiles.VAF.Extensions.Tests.Directives
{
	public abstract class TaskDirectiveWithDisplayNameTestsBase<TDirective>
		where TDirective : TaskDirectiveWithDisplayName, new()
	{
		[TestMethod]
		public void DisplayNameIsReadWrite()
		{
			var type = typeof(TDirective);
			var property = type.GetProperty("DisplayName");
			Assert.IsNotNull(property);
			Assert.IsTrue(property.CanRead);
			Assert.IsTrue(property.CanWrite);
		}

		[TestMethod]
		public void DisplayNamePersistsData()
		{
			var instance = new TDirective();
			Assert.IsNull(instance.DisplayName);
			instance.DisplayName = "hello world";
			Assert.AreEqual("hello world", instance.DisplayName);
		}

	}
}
