using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions
{
	public static partial class IEnumerableExtensionMethods
	{
		/// <summary>
		/// Returns all distinct elements of the given source, where "distinctness"
		/// is determined via a projection and the default equality comparer for the projected type.
		/// </summary>
		/// <remarks>
		/// This operator uses deferred execution and streams the results, although
		/// a set of already-seen keys is retained. If a key is seen multiple times,
		/// only the first element with that key is returned.
		/// </remarks>
		/// <typeparam name="TSource">Type of the source sequence</typeparam>
		/// <typeparam name="TKey">Type of the projected element</typeparam>
		/// <param name="source">Source sequence</param>
		/// <param name="keySelector">Projection for determining "distinctness"</param>
		/// <returns>A sequence consisting of distinct elements from the source sequence,
		/// comparing them by the specified key projection.</returns>
		/// <remarks>
		/// From https://github.com/morelinq/MoreLINQ#distinctby
		/// If we need more then we should consider referencing MoreLinq directly.
		/// </remarks>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
		(
			this IEnumerable<TSource> source, 
			Func<TSource, TKey> keySelector
		)
		{
			HashSet<TKey> knownKeys = new HashSet<TKey>();
			foreach (TSource element in source.AsNotNull())
			{
				if (knownKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		/// <summary>
		/// Returns <paramref name="collection"/> or, if it is null,
		/// <see cref="Enumerable.Empty{TResult}"/>.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection.</param>
		/// <returns><paramref name="collection"/> or, if it is null, <see cref="Enumerable.Empty{TResult}"/>.</returns>
		public static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> collection) 
			=> collection ?? Enumerable.Empty<T>();
	}
}
