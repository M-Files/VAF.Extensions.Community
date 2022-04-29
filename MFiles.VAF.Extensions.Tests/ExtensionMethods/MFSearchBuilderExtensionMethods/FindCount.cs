using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	[TestClass]
	public class FindCount
		: TestBaseWithVaultMock
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsWithNullSearchBuilder()
		{
			((MFSearchBuilder)null).FindCount();
		}

		/// <summary>
		/// Ensures that the <see cref="MFiles.VAF.Extensions.MFSearchBuilderExtensionMethods.FindCount"/> extension method delegates to 
		/// <see cref="VaultObjectSearchOperations.GetObjectCountInSearch"/>.
		/// </summary>
		/// <remarks>
		/// Inherits the same limitations as the referenced API method.
		/// If you truly need to count all objects in a large vault then you may need to use a segmented search,
		/// but that has potentially significant overhead on the server.
		/// </remarks>
		[TestMethod]
		public void CallGetObjectCountInSearch()
		{
			// Setup the search operations mock.
			var vaultSearchOperationsMock = new Mock<VaultObjectSearchOperations>();
			vaultSearchOperationsMock.Setup
			(
				m => m.GetObjectCountInSearch(Moq.It.IsAny<SearchConditions>(), Moq.It.IsAny<MFSearchFlags>())
			)
			.Returns(1)
			.Verifiable();

			// Setup the vault mock.
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.ObjectSearchOperations).Returns(vaultSearchOperationsMock.Object);

			// Create the search builder and call FindCount.
			var searchBuilder = new MFSearchBuilder(vaultMock.Object);
			Assert.AreEqual(1, searchBuilder.FindCount());
		}
	}
}
