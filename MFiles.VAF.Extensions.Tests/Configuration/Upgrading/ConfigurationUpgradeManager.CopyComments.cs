using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MFiles.VAF.Extensions.Tests.Configuration.Upgrading
{
	public partial class ConfigurationUpgradeManager
		: TestBaseWithVaultMock
	{

		[TestMethod]
		public void CopyComments_Target_Null()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = new JObject();
			JObject target = null;

			c.CopyComments(source, target);

			Assert.IsNull(target);

		}

		[TestMethod]
		public void CopyComments_Source_Null()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = null;
			JObject target = new JObject();

			c.CopyComments(source, target);

			Assert.IsNotNull(target);

		}

		[TestMethod]
		public void CopyComments_NoComments()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""hello"": ""world"" }");
			JObject target = JObject.Parse(@"{ ""hello"": ""world"" }");

			c.CopyComments(source, target);

			Assert.AreEqual(1, target.Properties().Count());

		}

		[TestMethod]
		public void CopyComments_SimpleComment_OnlySourcePopulated()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""hello"": ""world"", ""hello-Comment"": ""my comment"" }");
			JObject target = JObject.Parse(@"{ ""hello"": ""world"" }");

			c.CopyComments(source, target);

			Assert.AreEqual(2, target.Properties().Count());
			Assert.AreEqual(1, target.Properties().Count(p => p.Name == "hello-Comment" && p.Value.ToString() == "my comment"));

		}

		[TestMethod]
		public void CopyComments_SimpleComment_DoesNotCopyIfTargetIsMissingCommentedProperty()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""hello"": ""world"", ""hello-Comment"": ""my comment"" }");
			JObject target = JObject.Parse(@"{ }");

			c.CopyComments(source, target);

			Assert.AreEqual(0, target.Properties().Count());

		}

		[TestMethod]
		public void CopyComments_SimpleComment_SourceAndTargetPopulated()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""hello"": ""world"", ""hello-Comment"": ""my comment"" }");
			JObject target = JObject.Parse(@"{ ""hello"": ""world"", ""hello-Comment"": ""my old comment"" }");

			c.CopyComments(source, target);

			Assert.AreEqual(2, target.Properties().Count());
			Assert.AreEqual(1, target.Properties().Count(p => p.Name == "hello-Comment" && p.Value.ToString() == "my comment"));

		}

		[TestMethod]
		public void CopyComments_SimpleComment_OnlyTargetPopulated()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""hello"": ""world"" }");
			JObject target = JObject.Parse(@"{ ""hello"": ""world"", ""hello-Comment"": ""my comment"" }");

			c.CopyComments(source, target);

			Assert.AreEqual(2, target.Properties().Count());
			Assert.AreEqual(1, target.Properties().Count(p => p.Name == "hello-Comment" && p.Value.ToString() == "my comment"));

		}

		[TestMethod]
		public void CopyComments_ChildObjectIsAlsoUpdated()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""hello"": { ""hello"" : ""world"", ""hello-Comment"" : ""my comment"" } }");
			JObject target = JObject.Parse(@"{ ""hello"": { ""hello"" : ""world"" } }");

			c.CopyComments(source, target);

			var hello = target["hello"] as JObject;
			Assert.IsNotNull(hello);
			Assert.AreEqual(1, hello.Properties().Count(p => p.Name == "hello-Comment" && p.Value.ToString() == "my comment"));

		}

		[TestMethod]
		public void CopyComments_ArrayCommentsAreCopied_OneElementOneComment()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""Triggers"": [ { ""Item"" : ""One"" } ], ""Triggers-0-Comment"" : ""my comment"" }");
			JObject target = JObject.Parse(@"{ ""Triggers"": [] }");

			c.CopyComments(source, target);

			Assert.AreEqual(2, target.Properties().Count());
			Assert.AreEqual(1, target.Properties().Count(p => p.Name == "Triggers-0-Comment" && p.Value.ToString() == "my comment"));

		}

		[TestMethod]
		public void CopyComments_ArrayCommentsAreCopied_TwoElementsOneComment()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""Triggers"": [ { ""Item"" : ""One"" }, { ""Item"" : ""Two"" } ], ""Triggers-1-Comment"" : ""my comment"" }");
			JObject target = JObject.Parse(@"{ ""Triggers"": [] }");

			c.CopyComments(source, target);

			Assert.AreEqual(2, target.Properties().Count());
			Assert.AreEqual(1, target.Properties().Count(p => p.Name == "Triggers-1-Comment" && p.Value.ToString() == "my comment"));

		}

		[TestMethod]
		public void CopyComments_ArrayCommentsAreCopied_TwoElementsTwoComments()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""Triggers"": [ { ""Item"" : ""One"" }, { ""Item"" : ""Two"" } ], ""Triggers-0-Comment"" : ""my first comment"", ""Triggers-1-Comment"" : ""my second comment"" }");
			JObject target = JObject.Parse(@"{ ""Triggers"": [] }");

			c.CopyComments(source, target);

			Assert.AreEqual(3, target.Properties().Count());
			Assert.AreEqual(1, target.Properties().Count(p => p.Name == "Triggers-0-Comment" && p.Value.ToString() == "my first comment"));
			Assert.AreEqual(1, target.Properties().Count(p => p.Name == "Triggers-1-Comment" && p.Value.ToString() == "my second comment"));

		}

		[TestMethod]
		public void CopyComments_ArrayElementChildObjectIsAlsoUpdated()
		{
			var c = new EnsureLatestSerializationSettingsUpgradeRuleProxy();

			JObject source = JObject.Parse(@"{ ""Triggers"": [ { ""Item"" : ""One"", ""Item-Comment"": ""my comment"" } ] }");
			JObject target = JObject.Parse(@"{ ""Triggers"": [ { ""Item"" : ""One"" }, { ""Item"" : ""Two"" } ]  }");

			c.CopyComments(source, target);

			var triggers = target["Triggers"] as JArray;
			Assert.IsNotNull(triggers);
			var trigger = triggers[0] as JObject;
			Assert.IsNotNull(trigger);
			Assert.AreEqual(1, trigger.Properties().Count(p => p.Name == "Item-Comment" && p.Value.ToString() == "my comment"));

		}

	}
}
