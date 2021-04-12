using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public static class IEnumerableExtensionMethods
	{
		// From https://github.com/morelinq/MoreLINQ#distinctby
		// If we need more then we should consider referencing MoreLinq directly.
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
		(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> knownKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (knownKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}
	}
}
