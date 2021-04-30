using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MFiles.VAF.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class CreateCopy
		: TestBaseWithVaultMock
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			((Common.ObjVerEx)null).CreateCopy();
		}

	}
}
