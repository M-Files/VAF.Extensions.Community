using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFiles.VAF.Configuration;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	/// <summary>
	/// Provides a base class for testing configuration-style objects.
	/// </summary>
	public abstract class ConfigurationClassTestBase
	{
		public abstract System.Type GetClassBeingTested();

		[TestMethod]
		public void ClassBeingTestedIsNotNull()
		{
			Assert.IsNotNull(this.GetClassBeingTested());
		}

		[TestMethod]
		public void ClassHasDataContractAttribute()
		{
			Assert.IsNotNull
			(
				this.GetClassBeingTested()
				.GetCustomAttributes(false)
				.Cast<DataContractAttribute>()
				.FirstOrDefault(),
				"The class "
			);;
		}

		[TestMethod]
		public void EnsurePropertiesHaveDataMemberAttribute()
		{
			// What class should have the properties?
			var classBeingTested = this.GetClassBeingTested();

			// What properties should have [DataMember] attributes?
			foreach (var attribute in this
				.GetType()
				.GetCustomAttributes(typeof(DataMemberRequiredAttribute), true)
				.Cast<DataMemberRequiredAttribute>())
			{
				// Ensure each property has the attribute.
				foreach (var propertyName in attribute.PropertyNames)
				{
					Assert.IsTrue
						(
							classBeingTested
							.GetProperties()
							.Where(p => p.DeclaringType == classBeingTested && p.Name == propertyName)
							.FirstOrDefault()
							.HasCustomAttribute<DataMemberAttribute>()
						);
				}
			}
		}

		[TestMethod]
		public void EnsurePropertiesHaveSecurityAttribute()
		{
			// What class should have the properties?
			var classBeingTested = this.GetClassBeingTested();

			// What properties should have [Security] attribute?
			foreach (var attribute in this
				.GetType()
				.GetCustomAttributes(typeof(SecurityRequiredAttribute), true)
				.Cast<SecurityRequiredAttribute>())
			{
				// Ensure the property has the attribute.
				var securityAttribute =
							classBeingTested
							.GetProperties()
							.Where(p => p.DeclaringType == classBeingTested && p.Name == attribute.PropertyName)
							.FirstOrDefault()?
							.GetCustomAttribute<SecurityAttribute>();
				Assert.IsNotNull(securityAttribute);

				// Ensure values are correct.
				Assert.AreEqual(attribute.IsPassword, securityAttribute.IsPassword);
				Assert.AreEqual(attribute.ChangeBy, securityAttribute.ChangeBy);
				Assert.AreEqual(attribute.ViewBy, securityAttribute.ViewBy);
			}
		}

		[TestMethod]
		public void EnsurePropertiesHaveJsonConfEditorAttribute()
		{
			// What class should have the properties?
			var classBeingTested = this.GetClassBeingTested();

			// What properties should have [JsonConfEditor] attribute?
			foreach (var attribute in this
				.GetType()
				.GetCustomAttributes(typeof(JsonConfEditorRequiredAttribute), true)
				.Cast<JsonConfEditorRequiredAttribute>())
			{
				// Ensure the property has the attribute.
				var jsonConfEditorAttribute = classBeingTested
							.GetProperties()
							.Where(p => p.DeclaringType == classBeingTested && p.Name == attribute.PropertyName)
							.FirstOrDefault()?
							.GetCustomAttribute<JsonConfEditorAttribute>();
				Assert.IsNotNull(jsonConfEditorAttribute);

				// If we have a default then check.
				if (null != attribute.DefaultValue)
					Assert.AreEqual(attribute.DefaultValue, jsonConfEditorAttribute.DefaultValue);

				// Hidden / ShowWhen / HideWhen?
				Assert.AreEqual(attribute.Hidden, jsonConfEditorAttribute.Hidden);
				if (false == string.IsNullOrWhiteSpace(attribute.ShowWhen))
					Assert.AreEqual(attribute.ShowWhen, jsonConfEditorAttribute.ShowWhen);
				if (false == string.IsNullOrWhiteSpace(attribute.HideWhen))
					Assert.AreEqual(attribute.HideWhen, jsonConfEditorAttribute.HideWhen);
			}
		}
	}
}
