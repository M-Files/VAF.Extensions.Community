using MFiles.VAF.Common;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	/// <summary>
	/// A base class that tests of the <see cref="MFSearchBuilderExtensionMethods"/>
	/// can use.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public abstract class MFSearchBuilderExtensionMethodTestBase
		: TestBaseWithVaultMock
	{

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
