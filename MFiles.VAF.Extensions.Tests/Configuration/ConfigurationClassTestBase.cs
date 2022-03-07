using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFiles.VAF.Configuration;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	/// <summary>
	/// Provides a base class for testing configuration-style objects.
	/// </summary>
	public abstract class ConfigurationClassTestBase<TConfigurationClass>
	{
		[TestMethod]
		public void ClassHasDataContractAttribute()
		{
			var found = false;
			var typeToSearch = typeof(TConfigurationClass);
			while (found == false && typeToSearch != null && typeToSearch != typeof(object))
			{
				if (typeToSearch.GetCustomAttributes(typeof(DataContractAttribute), true).Length > 0)
				{
					found = true;
					continue;
				}
				typeToSearch = typeToSearch.BaseType;
			}
			Assert.IsTrue
			(
				found,
				$"{typeof(TConfigurationClass).FullName} does not have a [DataContract] attribute."
			);;
		}

		[TestMethod]
		public void EnsurePropertiesHaveDataMemberAttribute()
		{
			// What class should have the properties?
			var classBeingTested = typeof(TConfigurationClass);

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
							.GetProperties()?
							.Where(p => p.Name == propertyName)
							.FirstOrDefault()?
							.HasCustomAttribute<DataMemberAttribute>() ?? false,
							$"{classBeingTested.FullName}.{propertyName} was not found or does not have a [DataMember] attribute."
						);
				}
			}
		}

		[TestMethod]
		public void EnsurePropertiesHaveSecurityAttribute()
		{
			// What class should have the properties?
			var classBeingTested = typeof(TConfigurationClass);

			// What properties should have [Security] attribute?
			foreach (var attribute in this
				.GetType()
				.GetCustomAttributes(typeof(SecurityRequiredAttribute), true)
				.Cast<SecurityRequiredAttribute>())
			{
				// Ensure the property has the attribute.
				var securityAttribute =
							classBeingTested
							.GetProperties()?
							.Where(p => p.Name == attribute.PropertyName)
							.FirstOrDefault()?
							.GetCustomAttribute<SecurityAttribute>();
				Assert.IsNotNull
				(
					securityAttribute,
					$"{classBeingTested.FullName}.{attribute.PropertyName} was not found or does not have a [Security] attribute."
				);

				// Ensure values are as expected.
				Assert.AreEqual
				(
					attribute.IsPassword, 
					securityAttribute.IsPassword,
					$"[Security(IsPassword=)] was not set properly for {classBeingTested.FullName}.{attribute.PropertyName}"
				);
				Assert.AreEqual
				(
					attribute.ChangeBy,
					securityAttribute.ChangeBy,
					$"[Security(ChangeBy=)] was not set properly for {classBeingTested.FullName}.{attribute.PropertyName}"
				);
				Assert.AreEqual
				(
					attribute.ViewBy, 
					securityAttribute.ViewBy,
					$"[Security(ViewBy=)] was not set properly for {classBeingTested.FullName}.{attribute.PropertyName}"
				);
			}
		}

		[TestMethod]
		public void EnsurePropertiesHaveJsonConfEditorAttribute()
		{
			// What class should have the properties?
			var classBeingTested = typeof(TConfigurationClass);

			// What properties should have [JsonConfEditor] attribute?
			foreach (var attribute in this
				.GetType()
				.GetCustomAttributes(typeof(JsonConfEditorRequiredAttribute), true)
				.Cast<JsonConfEditorRequiredAttribute>())
			{
				// Ensure the property has the attribute.
				var jsonConfEditorAttribute = classBeingTested
							.GetProperties()?
							.Where(p => p.Name == attribute.PropertyName)
							.FirstOrDefault()?
							.GetCustomAttribute<JsonConfEditorAttribute>();
				Assert.IsNotNull
				(
					jsonConfEditorAttribute,
					$"{classBeingTested.FullName}.{attribute.PropertyName} was not found or does not have a [JsonConfEditor] attribute."
				);

				// If we have a default then check.
				if (null != attribute.DefaultValue)
					Assert.AreEqual(attribute.DefaultValue, jsonConfEditorAttribute.DefaultValue);

				// If we child type editor then check
				if (false == string.IsNullOrWhiteSpace(attribute.ChildTypeEditor))
					Assert.AreEqual
					(
						attribute.ChildTypeEditor, 
						jsonConfEditorAttribute.ChildTypeEditor,
						$"[JsonConfEditor(ChildTypeEditor=)] was not set properly for {classBeingTested.FullName}.{attribute.PropertyName}"
					);

				// Hidden / ShowWhen / HideWhen?
				Assert.AreEqual
				(
					attribute.Hidden,
					jsonConfEditorAttribute.Hidden,
					$"[JsonConfEditor(Hidden=)] was not set properly for {classBeingTested.FullName}.{attribute.PropertyName}"
				);
				if (false == string.IsNullOrWhiteSpace(attribute.ShowWhen))
					Assert.AreEqual
					(
						attribute.ShowWhen,
						jsonConfEditorAttribute.ShowWhen,
						$"[JsonConfEditor(Hidden=)] was not set properly for {classBeingTested.FullName}.{attribute.PropertyName}"
					);
				if (false == string.IsNullOrWhiteSpace(attribute.HideWhen))
					Assert.AreEqual
					(
						attribute.HideWhen, 
						jsonConfEditorAttribute.HideWhen,
						$"[JsonConfEditor(Hidden=)] was not set properly for {classBeingTested.FullName}.{attribute.PropertyName}"
					);
			}
		}
	}
}
