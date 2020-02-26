using MFiles.VAF.Common;
using MFilesAPI;
using Moq;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	/// <summary>
	/// A base class that tests of the <see cref="MFiles.VAF.Extensions.ExtensionMethods.MFSearchBuilderExtensionMethods"/>
	/// can use.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public abstract class MFSearchBuilderExtensionMethodTestBase
	{
		/// <summary>
		/// Returns a mock <see cref="Vault"/> that can be used to retrieve data as appropriate.
		/// </summary>
		/// <returns></returns>
		protected virtual Mock<Vault> GetVaultMock()
		{
			return new Mock<Vault>();
		}

		/// <summary>
		/// Returns a <see cref="MFSearchBuilder"/> that will be used for the tests.
		/// </summary>
		/// <returns></returns>
		protected virtual MFSearchBuilder GetSearchBuilder()
		{
			// Get the vault mock and populate it if needed.
			var vaultMock = this.GetVaultMock();

			// Return the search builder.
			return new MFSearchBuilder(vaultMock.Object);
		}
	}
}