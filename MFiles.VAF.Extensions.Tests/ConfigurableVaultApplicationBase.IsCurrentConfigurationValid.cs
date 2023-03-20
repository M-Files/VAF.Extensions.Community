using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests
{
	[TestClass]
	public class ConfigurableVaultApplicationBase
	{
		[TestMethod]
		[DataRow(true, false, false, true)] // Valid, no cache.
		[DataRow(false, false, false, true)] // Invalid, no cache.
		[DataRow(true, true, false, false)] // Valid, with cache.
		[DataRow(false, true, false, false)] // Invalid, with cache.
		[DataRow(true, true, true, true)] // Valid, with cache, force refresh.
		[DataRow(false, true, true, true)] // Invalid, with cache, force refresh.
		public void IsCurrentConfigurationValid
		(
			bool isConfigurationValid, // What IsValid will return.
			bool warmCache, // If true, cache will be populated before tests.
			bool forceCacheRefresh, // If true then the cache will be populated.
			bool isValidCallExpected // If true, IsValid must be called.
		)
		{
			var proxy = new Proxy(isConfigurationValid); // IsValid will return true.

			// Should we warm the cache?
			if (warmCache)
			{
				proxy.IsCurrentConfigurationValid(null, forceCacheRefresh); // Call it once so that it populates the cache.
				proxy.IsValidCalled = false; // Reset our flag.
			}

			// Does it return the correct valid?
			Assert.AreEqual(isConfigurationValid, proxy.IsCurrentConfigurationValid(null, forceCacheRefresh));

			// Did it call IsValid?
			Assert.AreEqual(isValidCallExpected, proxy.IsValidCalled);
		}

		private class Proxy
			: ConfigurableVaultApplicationBaseProxy
		{
			public bool IsConfigurationValid { get; } = false;
			public bool IsValidCalled { get; set; } = false;
			public Proxy(bool isConfigurationValid)
			{
				this.IsConfigurationValid = isConfigurationValid;
			}
			public override bool IsValid(Vault vault)
			{
				this.IsValidCalled = true;
				return this.IsConfigurationValid;
			}
			public new bool IsCurrentConfigurationValid(Vault vault, bool force = false)
			{
				return base.IsCurrentConfigurationValid(vault, force);
			}
		}
	}
}
