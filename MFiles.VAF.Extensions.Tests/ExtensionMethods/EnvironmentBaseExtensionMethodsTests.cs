using MFiles.VAF.Common;
using MFiles.VAF.Extensions.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods
{
	[TestClass]
	public class EnvironmentBaseExtensionMethodsTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void IsCurrentUserSystemProcess_ThrowsWithNullArgument()
		{
			((EnvironmentBase)null).IsCurrentUserSystemProcess();
		}

		[TestMethod]
		public void IsCurrentUserSystemProcess_ReturnsTrueForMFilesServerUser()
		{
			var environmentBase = new EnvironmentBase()
			{
				CurrentUserID = MFBuiltInUsers.MFilesServerUserID
			};
			Assert.IsTrue(environmentBase.IsCurrentUserSystemProcess());
		}

		[TestMethod]
		public void IsCurrentUserSystemProcess_ReturnsFalseForRandomValidUserId()
		{
			var environmentBase = new EnvironmentBase()
			{
				CurrentUserID = 4 // chosen by fair dice roll; guaranteed to be random.
			};
			Assert.IsFalse(environmentBase.IsCurrentUserSystemProcess());
		}
	}
}
