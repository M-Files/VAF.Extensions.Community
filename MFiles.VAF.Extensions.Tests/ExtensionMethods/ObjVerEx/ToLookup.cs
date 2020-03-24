using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class ToLookup : TestBaseWithVaultMock
	{
		/// <summary>
		/// A null <see cref="Common.ObjVerEx"/> should throw an exception.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			((Common.ObjVerEx) null).ToLookup(true);
		}
		
		[TestMethod]
		[DataRow(false, 0, 1, 1)]
		[DataRow(true, 2, 3, -1)]
		public void ToLookupTest
			(
			bool latestVersion,
			int objectType,
			int objectID,
			int expectedOutput
			)
		{
			// Get the vault mock and populate it if needed.
			var vaultMock = this.GetVaultMock();

			// Create the ObjVerEx and set the properties.
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectType, objectID, 1);
			Lookup lkp = objVerEx.ToLookup(latestVersion);

			// Assert.
			Assert.AreEqual(objectType, lkp.ObjectType);
			Assert.AreEqual(objectID, lkp.Item);
			Assert.AreEqual(expectedOutput, lkp.Version);
		}
	}
}
