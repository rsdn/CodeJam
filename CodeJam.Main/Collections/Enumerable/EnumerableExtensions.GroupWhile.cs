using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		#region While<TKey>
		/// <summary>Groups items in the sequence while they have same grouping key.</summary>
		/// <typeparam name="T">Type of items in sequence</typeparam>
		/// <typeparam name="TKey">The type of the grouping key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The grouping key selector.</param>
		/// <returns>Grouped items with grouping key.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<IGrouping<TKey, T>> GroupWhileEquals<T, TKey>(
			this IEnumerable<T> source,
			Func<T, TKey> keySelector) =>
				GroupWhileEquals(source, keySelector, null);

		/// <summary>Groups items in the sequence while they have same grouping key.</summary>
		/// <typeparam name="T">Type of items in sequence</typeparam>
		/// <typeparam name="TKey">The type of the grouping key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The grouping key selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Grouped items with grouping key.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<IGrouping<TKey, T>> GroupWhileEquals<T, TKey>(
			this IEnumerable<T> source,
			Func<T, TKey> keySelector,
			IEqualityComparer<TKey>? comparer)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(keySelector, nameof(keySelector));

			return GroupWhileCore(source, keySelector, comparer);
		}

		[Pure, System.Diagnostics.Contracts.Pure]
		private static IEnumerable<IGrouping<TKey, T>> GroupWhileCore<T, TKey>(
			IEnumerable<T> source,
			Func<T, TKey> keySelector,
			IEqualityComparer<TKey>? comparer)
		{
			comparer ??= EqualityComparer<TKey>.Default;
			var key = default(TKey);
			var groupingList = new List<T>();

			foreach (var item in source)
			{
				var newKey = keySelector(item);
				if (groupingList.Count > 0 && !comparer.Equals(
					key
#if !NETCOREAPP30_OR_GREATER
						! // Old frameworks missing NRT markup
#endif
					, newKey))
				{
					yield return new Grouping<TKey, T>(key!, groupingList.ToArray());
					groupingList.Clear();
				}
				key = newKey;
				groupingList.Add(item);
			}

			if (groupingList.Count > 0)
			{
				yield return new Grouping<TKey, T>(key!, groupingList.ToArray());
			}
		}
		#endregion

		#region While<TItem, TKey>
		/// <summary>Groups items in the sequence while they have same grouping key.</summary>
		/// <typeparam name="T">Type of items in sequence</typeparam>
		/// <typeparam name="TItem">The type of resulting item.</typeparam>
		/// <typeparam name="TKey">The type of the grouping key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The grouping key selector.</param>
		/// <param name="itemSelector">The item selector.</param>
		/// <returns>Grouped items with grouping key.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<IGrouping<TKey, TItem>> GroupWhileEquals<T, TItem, TKey>(
			this IEnumerable<T> source,
			Func<T, TKey> keySelector,
			Func<T, TItem> itemSelector) =>
				GroupWhileEquals(source, keySelector, itemSelector, null);

		/// <summary>Groups items in the sequence while they have same grouping key.</summary>
		/// <typeparam name="T">Type of items in sequence</typeparam>
		/// <typeparam name="TItem">The type of resulting item.</typeparam>
		/// <typeparam name="TKey">The type of the grouping key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The grouping key selector.</param>
		/// <param name="itemSelector">The item selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Grouped items with grouping key.</returns>
		public static IEnumerable<IGrouping<TKey, TItem>> GroupWhileEquals<T, TItem, TKey>(
			this IEnumerable<T> source,
			Func<T, TKey> keySelector,
			Func<T, TItem> itemSelector,
			IEqualityComparer<TKey>? comparer)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(keySelector, nameof(keySelector));
			Code.NotNull(itemSelector, nameof(itemSelector));

			return GroupWhileCore(source, keySelector, itemSelector, comparer);
		}

		private static IEnumerable<IGrouping<TKey, TItem>> GroupWhileCore<T, TItem, TKey>(
			IEnumerable<T> source,
			Func<T, TKey> keySelector,
			Func<T, TItem> itemSelector,
			IEqualityComparer<TKey>? comparer)
		{
			comparer ??= EqualityComparer<TKey>.Default;
			var key = default(TKey);
			var groupingList = new List<TItem>();

			foreach (var item in source)
			{
				var newKey = keySelector(item);
				if (groupingList.Count > 0 && !comparer.Equals(
#if NETCOREAPP30_OR_GREATER
					key
#else
					key! // No nullable markup in older targets
#endif
					,
					newKey))
				{
					yield return new Grouping<TKey, TItem>(key!, groupingList.ToArray());
					groupingList.Clear();
				}
				key = newKey;
				groupingList.Add(itemSelector(item));
			}

			if (groupingList.Count > 0)
			{
				yield return new Grouping<TKey, TItem>(key!, groupingList.ToArray());
			}
		}
		#endregion

		#region While(true)
		/// <summary>Groups items in the sequence while they have same grouping key.</summary>
		/// <typeparam name="T">Type of items in sequence</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="predicate">Grouping predicate.</param>
		/// <returns>Grouped items.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<T[]> GroupWhile<T>(
			this IEnumerable<T> source,
			Func<T?, T?, bool> predicate)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(predicate, nameof(predicate));
			return GroupWhileCore(source, predicate);
		}

		private static IEnumerable<T[]> GroupWhileCore<T>(
			IEnumerable<T> source,
			Func<T?, T?, bool> predicate)
		{
			var previousItem = default(T);
			var groupingList = new List<T>();

			foreach (var item in source)
			{
				if (groupingList.Count > 0 && !predicate(previousItem, item))
				{
					yield return groupingList.ToArray();
					groupingList.Clear();
				}
				previousItem = item;
				groupingList.Add(item);
			}

			if (groupingList.Count > 0)
			{
				yield return groupingList.ToArray();
			}
		}
		#endregion
	}
}