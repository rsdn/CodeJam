using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		#region TopoSort
		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T>(
				[NotNull, InstantHandle] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter) =>
			TopoSort(source, dependsOnGetter, EqualityComparer<T>.Default);

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T>(
				[NotNull] this ICollection<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter) =>
			TopoSort(source, dependsOnGetter, EqualityComparer<T>.Default);

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="keySelector">Function that returns an item key, wich is used to compare.</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T, TKey>(
				[NotNull, InstantHandle] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull, InstantHandle] Func<T, TKey> keySelector) =>
			TopoSort(source, dependsOnGetter, KeyEqualityComparer.Create(keySelector));

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="keySelector">Function that returns an item key, wich is used to compare.</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T, TKey>(
				[NotNull] this ICollection<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull, InstantHandle] Func<T, TKey> keySelector) =>
			TopoSort(source, dependsOnGetter, KeyEqualityComparer.Create(keySelector));

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="keySelector">Function that returns an item key, wich is used to compare.</param>
		/// <param name="keyComparer">Equality comparer for item comparison</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T, TKey>(
				[NotNull, InstantHandle] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull, InstantHandle] Func<T, TKey> keySelector,
				[NotNull] IEqualityComparer<TKey> keyComparer) =>
			TopoSort(source, dependsOnGetter, KeyEqualityComparer.Create(keySelector, keyComparer));

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="keySelector">Function that returns an item key, wich is used to compare.</param>
		/// <param name="keyComparer">Equality comparer for item comparison</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T, TKey>(
				[NotNull] this ICollection<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull, InstantHandle] Func<T, TKey> keySelector,
				[NotNull] IEqualityComparer<TKey> keyComparer) =>
			TopoSort(source, dependsOnGetter, KeyEqualityComparer.Create(keySelector, keyComparer));

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="equalityComparer">Equality comparer for item comparison</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T>(
				[NotNull, InstantHandle] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull] IEqualityComparer<T> equalityComparer) =>
			GroupTopoSort(source, dependsOnGetter, equalityComparer)
				.Select(g => g.AsEnumerable())
				.SelectMany();

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="equalityComparer">Equality comparer for item comparison</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/>.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T> TopoSort<T>(
				[NotNull] this ICollection<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull] IEqualityComparer<T> equalityComparer) =>
			GroupTopoSort(source, dependsOnGetter, equalityComparer)
				.Select(g => g.AsEnumerable())
				.SelectMany();
		#endregion

		#region GroupTopoSort
		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/> separated by dependency level.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T[]> GroupTopoSort<T>(
				[NotNull, InstantHandle] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter) =>
			GroupTopoSort(source, dependsOnGetter, EqualityComparer<T>.Default);

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/> separated by dependency level.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T[]> GroupTopoSort<T>(
				[NotNull] this ICollection<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter) =>
			GroupTopoSort(source, dependsOnGetter, EqualityComparer<T>.Default);

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="equalityComparer">Equality comparer for item comparison</param>
		/// <returns>Topologically sorted list of items in <paramref name="source"/> separated by dependency level.</returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T[]> GroupTopoSort<T>(
				[NotNull, InstantHandle] this IEnumerable<T> source,
				[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
				[NotNull] IEqualityComparer<T> equalityComparer) =>
			GroupTopoSort(source.ToArray(), dependsOnGetter, equalityComparer);

		/// <summary>
		/// Performs topological sort on <paramref name="source"/>.
		/// </summary>
		/// <param name="source">Collection to sort.</param>
		/// <param name="dependsOnGetter">Function that returns items dependent on specified item.</param>
		/// <param name="equalityComparer">Equality comparer for item comparison</param>
		/// <returns>
		/// Topologically sorted list of items in <paramref name="source"/>, separated by dependency level.
		/// </returns>
		[NotNull]
		[Pure]
		public static IEnumerable<T[]> GroupTopoSort<T>(
			[NotNull] this ICollection<T> source,
			[NotNull, InstantHandle] Func<T, IEnumerable<T>> dependsOnGetter,
			[NotNull] IEqualityComparer<T> equalityComparer)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(dependsOnGetter, nameof(dependsOnGetter));
			Code.NotNull(equalityComparer, nameof(equalityComparer));

			// Fast path
			if (source.Count == 0)
				yield break;

			var dependants = LazyDictionary.Create(k => new List<T>(), equalityComparer, false);
			var workArray = new int[source.Count];
			var indices = new Dictionary<T, int>(equalityComparer);
			var level = new List<T>();
			foreach (var item in source.WithIndex())
			{
				var count = 0;
				foreach (var dep in dependsOnGetter(item.Item))
				{
					dependants[dep].Add(item.Item);
					count++;
				}
				if (count == 0)
					level.Add(item.Item);
				else
					workArray[item.Index] = count;
				indices.Add(item.Item, item.Index);
			}

			if (level.Count == 0)
				throw CycleException(nameof(source));

			// Fast path
			if (level.Count == workArray.Length)
			{
				yield return level.ToArray();
				yield break;
			}

			var pendingCount = workArray.Length;
			while (true)
			{
				var nextLevel = Lazy.Create(() => new List<T>(), false);
				foreach (var item in level)
					foreach (var dep in dependants[item])
					{
						var pending = --workArray[indices[dep]];
						if (pending == 0)
							nextLevel.Value.Add(dep);
					}
				yield return level.ToArray();
				pendingCount -= level.Count;
				if (pendingCount == 0)
					yield break;
				if (!nextLevel.IsValueCreated)
					throw CycleException(nameof(source));
				level = nextLevel.Value;
			}
		}

		private static ArgumentException CycleException(string argName) =>
			CodeExceptions.Argument(argName, "Cycle detected.");
		#endregion
	}
}
