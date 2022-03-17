using MFiles.VAF.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	[TestClass]
	public class ConfigurationBaseTests
	{
		[TestMethod]
		public void EnsureLoggingPropertyHasCorrectSecurityAttribute()
		{
			// Get the property itself.
			var type = typeof(VAF.Extensions.Configuration.ConfigurationBase);
			var propertyInfo = type.GetProperty(nameof(VAF.Extensions.Configuration.ConfigurationBase.Logging));
			Assert.IsNotNull(propertyInfo, $"Property {type.GetProperty(nameof(VAF.Extensions.Configuration.ConfigurationBase.Logging))} not found.");

			// Get the security attribute on the property.
			var securityAttribute = propertyInfo.GetCustomAttribute<SecurityAttribute>();
			Assert.IsNotNull(securityAttribute, "Security attribute not found.");

			// Ensure that it's configurable by vault admin.
			Assert.AreEqual(SecurityAttribute.UserLevel.VaultAdmin, securityAttribute.ChangeBy);
			Assert.AreEqual(SecurityAttribute.UserLevel.VaultAdmin, securityAttribute.ViewBy);
		}
	}
}
