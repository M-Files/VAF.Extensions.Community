using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading.Rules
{
	public partial class EnsureLatestSerializationSettingsUpgradeRuleTests
		: TestBaseWithVaultMock
	{
		[TestMethod]
		public void AreEqual_Nulls()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			// Check that null == null, but null != instance.
			Assert.IsTrue(c.AreEqual(null, null));
			Assert.IsFalse(c.AreEqual(new JObject(), null));
			Assert.IsFalse(c.AreEqual(null, new JObject()));

		}

		[TestMethod]
		public void AreEqual_False_DifferentProperties()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			var a = new JObject();
			a.Add("hello", "world");

			var b = new JObject();
			b.Add("hi", "world");

			Assert.IsFalse(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_False_DifferentValues()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			var a = new JObject();
			a.Add("hello", "world");

			var b = new JObject();
			b.Add("hello", "you");

			Assert.IsFalse(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_True_DifferentComments()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			var a = new JObject();
			a.Add("hello", "world");
			a.Add("hello-Comment", "comment 1");

			var b = new JObject();
			b.Add("hello", "world");
			b.Add("hello-Comment", "comment 2");

			Assert.IsTrue(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_True_Nulls()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			var a = JObject.Parse(@"{ ""hello"" : null }");

			var b = JObject.Parse(@"{ ""hello"" : null }");

			Assert.IsTrue(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_True_SameChildObjects()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			var a = new JObject();
			{
				a.Add("hello", "world");
				var child = new JObject();
				child.Add("property", "value1");
				a.Add("child", child);
			}

			var b = new JObject();
			{
				b.Add("hello", "world");
				var child = new JObject();
				child.Add("property", "value1");
				b.Add("child", child);
			}

			Assert.IsTrue(c.AreEqual(a, b));

		}

		[TestMethod]
		public void AreEqual_False_DifferentChildObjects()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			var a = new JObject();
			{
				a.Add("hello", "world");
				var child = new JObject();
				child.Add("property", "value1");
				a.Add("child", child);
			}

			var b = new JObject();
			{
				b.Add("hello", "world");
				var child = new JObject();
				child.Add("property", "value2");
				b.Add("child", child);
			}

			Assert.IsFalse(c.AreEqual(a, b));

		}

	}
}
