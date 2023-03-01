using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods
{
	[TestClass]
	public class IEnumerableExtensionMethodsTests
	{
		[TestMethod]
		public void AsNotNull_ReturnsOriginalCollection()
		{
			var collection = new List<string>();
			Assert.AreSame(collection, collection.AsNotNull());
		}

		[TestMethod]
		public void AsNotNull_DoesNotReturnNull()
		{
			Assert.IsNotNull(((IEnumerable<string>)null).AsNotNull());
		}
	}
}
