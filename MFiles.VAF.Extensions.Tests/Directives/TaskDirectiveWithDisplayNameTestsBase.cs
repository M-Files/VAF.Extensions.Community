using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

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
		public void DisplayName_HasDataMemberAttribute()
		{
			this.AssertPropertyHasDataMemberAttribute(nameof(TaskDirectiveWithDisplayName.DisplayName));
		}

		[TestMethod]
		public void DisplayNamePersistsData()
		{
			var instance = new TDirective();
			Assert.IsNull(instance.DisplayName);
			instance.DisplayName = "hello world";
			Assert.AreEqual("hello world", instance.DisplayName);
		}

		[TestMethod]
		public void DirectiveType_HasDataContractAttribute()
		{
			var type = typeof(TDirective);
			Assert.IsNotNull(type.GetCustomAttributes(false).FirstOrDefault(a => a is DataContractAttribute));
		}

		protected void AssertPropertyHasDataMemberAttribute(string propertyName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
		{
			var type = typeof(TDirective);
			Assert.IsNotNull(type.GetProperty(propertyName, bindingFlags).GetCustomAttribute(typeof(DataMemberAttribute)));
		}

	}
}
