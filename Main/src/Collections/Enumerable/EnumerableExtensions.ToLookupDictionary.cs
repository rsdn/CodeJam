using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Defines behavior for duplicates in lookup source
	/// </summary>
	public enum DictionaryDuplicate
	{
		/// <summary>An exception will be thrown.</summary>
		Throw,
		/// <summary>The first item in lookup wins.</summary>
		FirstWins,
		/// <summary>The last item in lookup wins.</summary>
		LastWins
	}

	partial class EnumerableExtensions
	{
		/// <summary>Creates a lookup dictionary.</summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the value returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="source">The source to create a lookup dictionary from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A lookup dictionary.</returns>
		[Pure, NotNull]
		public static Dictionary<TKey, T> ToLookupDictionary<T, TKey>(
			[NotNull] this IEnumerable<T> source,
			[NotNull] Func<T, TKey> keySelector,
			DictionaryDuplicate duplicateHandling) =>
				ToLookupDictionary(source, keySelector, Fn<T>.Self, null, duplicateHandling);

		/// <summary>Creates a lookup dictionary.</summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the value returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="source">The source to create a lookup dictionary from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="comparer">An equality comparer to compare keys.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A lookup dictionary.</returns>
		[Pure, NotNull]
		public static Dictionary<TKey, T> ToLookupDictionary<T, TKey>(
			[NotNull] this IEnumerable<T> source,
			[NotNull] Func<T, TKey> keySelector,
			[CanBeNull] IEqualityComparer<TKey> comparer,
			DictionaryDuplicate duplicateHandling) =>
				ToLookupDictionary(source, keySelector, Fn<T>.Self, comparer, duplicateHandling);

		/// <summary>Creates a lookup dictionary.</summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the value returned by <paramref name="keySelector"/>.</typeparam>
		/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
		/// <param name="source">The source to create a lookup dictionary from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A lookup dictionary.</returns>
		[Pure, NotNull]
		public static Dictionary<TKey, TElement> ToLookupDictionary<T, TKey, TElement>(
			[NotNull] this IEnumerable<T> source,
			[NotNull] Func<T, TKey> keySelector,
			[NotNull] Func<T, TElement> elementSelector,
			DictionaryDuplicate duplicateHandling) =>
				ToLookupDictionary(source, keySelector, elementSelector, null, duplicateHandling);

		/// <summary>Creates a lookup dictionary.</summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the value returned by <paramref name="keySelector"/>.</typeparam>
		/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
		/// <param name="source">The source to create a lookup dictionary from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
		/// <param name="comparer">An equality comparer to compare keys.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A lookup dictionary.</returns>
		[Pure, NotNull]
		public static Dictionary<TKey, TElement> ToLookupDictionary<T, TKey, TElement>(
			[NotNull] this IEnumerable<T> source,
			[NotNull] Func<T, TKey> keySelector,
			[NotNull] Func<T, TElement> elementSelector,
			[CanBeNull] IEqualityComparer<TKey> comparer,
			DictionaryDuplicate duplicateHandling)
		{
			Code.InRange((int)duplicateHandling, nameof(duplicateHandling), (int)DictionaryDuplicate.Throw, (int)DictionaryDuplicate.LastWins);

			if (duplicateHandling == DictionaryDuplicate.Throw)
			{
				return source.ToDictionary(keySelector, elementSelector, comparer);
			}

			var result = new Dictionary<TKey, TElement>(comparer);
			foreach (var item in source)
			{
				var key = keySelector(item);
				if (duplicateHandling == DictionaryDuplicate.LastWins || !result.ContainsKey(key))
					result[key] = elementSelector(item);
			}

			return result;
		}
	}
}
