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
		public void CurrentUserIsSystemProcess_ThrowsWithNullArgument()
		{
			((EnvironmentBase)null).CurrentUserIsSystemProcess();
		}

		[TestMethod]
		public void CurrentUserIsSystemProcess_ReturnsTrueForMFilesServerUser()
		{
			var environmentBase = new EnvironmentBase()
			{
				CurrentUserID = EnvironmentBaseExtensionMethods.MFilesServerUserID
			};
			Assert.IsTrue(environmentBase.CurrentUserIsSystemProcess());
		}

		[TestMethod]
		public void CurrentUserIsSystemProcess_ReturnsFalseForRandomValidUserId()
		{
			var environmentBase = new EnvironmentBase()
			{
				CurrentUserID = 4 // chosen by fair dice roll; guaranteed to be random.
			};
			Assert.IsFalse(environmentBase.CurrentUserIsSystemProcess());
		}
	}
}
