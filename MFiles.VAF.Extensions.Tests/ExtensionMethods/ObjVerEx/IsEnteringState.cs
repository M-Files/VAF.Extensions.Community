using MFiles.VAF.Configuration;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class IsEnteringState
		: TestBaseWithVaultMock
	{

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsIfNullObjVerEx()
		{
			((Common.ObjVerEx)null).IsEnteringState(123);
		}

		[TestMethod]
		public void ReturnsFalseIfNullMFIdentifier()
		{
			Assert.IsFalse(new MFiles.VAF.Common.ObjVerEx().IsEnteringState(123));
		}


	}
}
