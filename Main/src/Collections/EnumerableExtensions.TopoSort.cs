using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <returns>Topologicaly sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static List<T> TopoSort<T>(
				[NotNull] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter) =>
			TopoSort(source, dependsOnGetter, EqualityComparer<T>.Default);

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="keySelector">Function that returns an item key, wich is used to compare.</param>
		/// <returns>Topologicaly sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static List<T> TopoSort<T, TKey>(
				[NotNull] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull, InstantHandle] Func<T, TKey> keySelector) =>
			TopoSort(source, dependsOnGetter, KeyEqualityComparer.Create(keySelector));

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="keySelector">Function that returns an item key, wich is used to compare.</param>
		/// <param name="keyComparer">Equality comparer for item comparision</param>
		/// <returns>Topologicaly sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static List<T> TopoSort<T, TKey>(
			[NotNull] this IEnumerable<T> source,
			[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
			[NotNull, InstantHandle] Func<T, TKey> keySelector,
			[NotNull] IEqualityComparer<TKey> keyComparer) =>
				TopoSort(source, dependsOnGetter, KeyEqualityComparer.Create(keySelector, keyComparer));

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="equalityComparer">Equality comparer for item comparision</param>
		/// <returns>Topologicaly sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static List<T> TopoSort<T>(
			[NotNull] this IEnumerable<T> source,
			[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
			[NotNull] IEqualityComparer<T> equalityComparer)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(dependsOnGetter, nameof(dependsOnGetter));
			Code.NotNull(equalityComparer, nameof(equalityComparer));

			var result = new List<T>();
			var visited = new HashSet<T>(equalityComparer);
			foreach (var item in source)
				SortLevel(item, dependsOnGetter, visited, new HashSet<T>(equalityComparer), result);

			return result;
		}

		private static void SortLevel<T>(
			T item,
			Func<T, IEnumerable<T>> dependsOnGetter,
			HashSet<T> visited,
			HashSet<T> cycleDetector,
			List<T> result)
		{
			// TODO: replace recursive algorythm by linear
			Code.AssertArgument(!cycleDetector.Contains(item), nameof(item), "There is a cycle in supplied source.");
			cycleDetector.Add(item);
			if (visited.Contains(item))
				return;
			foreach (var depItem in dependsOnGetter(item).Where(dt => !visited.Contains(dt)))
				SortLevel(depItem, dependsOnGetter, visited, cycleDetector, result);
			visited.Add(item);
			result.Add(item);
		}
	}
}
