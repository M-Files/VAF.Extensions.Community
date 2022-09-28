using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	public partial class ConfigurationUpgradeManager
		: TestBaseWithVaultMock
	{

		[TestMethod]
		public void AreEqual_Nulls()
		{
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = this.GetNamedValueStorageManagerMock().Object
			};

			// Check that null == null, but null != instance.
			Assert.IsTrue(c.AreEqual(null, null));
			Assert.IsFalse(c.AreEqual(new Newtonsoft.Json.Linq.JObject(), null));
			Assert.IsFalse(c.AreEqual(null, new Newtonsoft.Json.Linq.JObject()));

		}

		[TestMethod]
		public void AreEqual_False_DifferentProperties()
		{
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = this.GetNamedValueStorageManagerMock().Object
			};

			var a = new Newtonsoft.Json.Linq.JObject();
			a.Add("hello", "world");

			var b = new Newtonsoft.Json.Linq.JObject();
			b.Add("hi", "world");

			Assert.IsFalse(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_False_DifferentValues()
		{
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = this.GetNamedValueStorageManagerMock().Object
			};

			var a = new Newtonsoft.Json.Linq.JObject();
			a.Add("hello", "world");

			var b = new Newtonsoft.Json.Linq.JObject();
			b.Add("hello", "you");

			Assert.IsFalse(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_True_DifferentComments()
		{
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = this.GetNamedValueStorageManagerMock().Object
			};

			var a = new Newtonsoft.Json.Linq.JObject();
			a.Add("hello", "world");
			a.Add("hello-Comment", "comment 1");

			var b = new Newtonsoft.Json.Linq.JObject();
			b.Add("hello", "world");
			b.Add("hello-Comment", "comment 2");

			Assert.IsTrue(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_True_SameChildObjects()
		{
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = this.GetNamedValueStorageManagerMock().Object
			};

			var a = new Newtonsoft.Json.Linq.JObject();
			{
				a.Add("hello", "world");
				var child = new Newtonsoft.Json.Linq.JObject();
				child.Add("property", "value1");
				a.Add("child", child);
			}

			var b = new Newtonsoft.Json.Linq.JObject();
			{
				b.Add("hello", "world");
				var child = new Newtonsoft.Json.Linq.JObject();
				child.Add("property", "value1");
				b.Add("child", child);
			}

			Assert.IsTrue(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_False_DifferentChildObjects()
		{
			var c = new Extensions.Configuration.Upgrading.ConfigurationUpgradeManager(Mock.Of<VaultApplicationBase>())
			{
				NamedValueStorageManager = this.GetNamedValueStorageManagerMock().Object
			};

			var a = new Newtonsoft.Json.Linq.JObject();
			{
				a.Add("hello", "world");
				var child = new Newtonsoft.Json.Linq.JObject();
				child.Add("property", "value1");
				a.Add("child", child);
			}

			var b = new Newtonsoft.Json.Linq.JObject();
			{
				b.Add("hello", "world");
				var child = new Newtonsoft.Json.Linq.JObject();
				child.Add("property", "value2");
				b.Add("child", child);
			}

			Assert.IsFalse(c.AreEqual(a, b));

		}

	}
}
