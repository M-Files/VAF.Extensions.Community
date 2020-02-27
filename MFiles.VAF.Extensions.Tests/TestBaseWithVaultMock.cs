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
		{
			var mock = new Mock<Vault>
			{
				DefaultValue = DefaultValue.Mock
			};
			mock.SetupAllProperties();
			return mock;
		}
	}
}
