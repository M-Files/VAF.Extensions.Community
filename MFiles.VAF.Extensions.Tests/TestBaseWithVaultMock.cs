using MFilesAPI;
using Moq;

namespace MFiles.VAF.Extensions.Tests
{
	public abstract class TestBaseWithVaultMock
	{
		/// <summary>
		/// Returns a mock <see cref="Vault"/> that can be used to retrieve data as appropriate.
		/// </summary>
		/// <returns></returns>
		protected virtual Mock<Vault> GetVaultMock()
			=> this.GetVaultMock(MockBehavior.Default);

		/// <summary>
		/// Returns a mock <see cref="Vault"/> that can be used to retrieve data as appropriate.
		/// </summary>
		/// <returns></returns>
		protected virtual Mock<Vault> GetVaultMock(MockBehavior behaviour)
		{
			var mock = new Mock<Vault>(behaviour)
			{
				DefaultValue = DefaultValue.Empty
			};
			mock.SetupAllProperties();
			return mock;
		}
	}
}
