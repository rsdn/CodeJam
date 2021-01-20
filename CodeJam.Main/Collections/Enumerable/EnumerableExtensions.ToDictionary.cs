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
		/// <summary>
		/// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/>
		/// according to a specified key selector function and a duplicate handling policy.
		/// </summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="Dictionary{TKey,TValue}"/> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains keys and values.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Dictionary<TKey, T> ToDictionary<T, TKey>(
			[InstantHandle] this IEnumerable<T> source,
			[InstantHandle] Func<T, TKey> keySelector,
			DictionaryDuplicate duplicateHandling)
			where TKey : notnull =>
				ToDictionary(source, keySelector, Fn<T>.Self, null, duplicateHandling);

		/// <summary>
		/// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/>
		/// according to a specified key selector function, a comparer and a duplicate handling policy.
		/// </summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="Dictionary{TKey,TValue}"/> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare keys.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains keys and values.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Dictionary<TKey, T> ToDictionary<T, TKey>(
			[InstantHandle] this IEnumerable<T> source,
			[InstantHandle] Func<T, TKey> keySelector,
			IEqualityComparer<TKey>? comparer,
			DictionaryDuplicate duplicateHandling)
			where TKey : notnull =>
				ToDictionary(source, keySelector, Fn<T>.Self, comparer, duplicateHandling);

		/// <summary>
		/// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/>
		/// according to a specified key selector function, an element selector function and a duplicate handling policy.
		/// </summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="Dictionary{TKey,TValue}"/> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains keys and values.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Dictionary<TKey, TElement> ToDictionary<T, TKey, TElement>(
			[InstantHandle] this IEnumerable<T> source,
			[InstantHandle] Func<T, TKey> keySelector,
			[InstantHandle] Func<T, TElement> elementSelector,
			DictionaryDuplicate duplicateHandling)
			where TKey : notnull =>
				ToDictionary(source, keySelector, elementSelector, null, duplicateHandling);

		/// <summary>
		/// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/>
		/// according to a specified key selector function, an element selector function,
		/// a comparer and a duplicate handling policy.
		/// </summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TKey">The type of the value returned by <paramref name="keySelector"/>.</typeparam>
		/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
		/// <param name="source">The source to create a lookup dictionary from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
		/// <param name="comparer">An equality comparer to compare keys.</param>
		/// <param name="duplicateHandling">Policy for duplicate handling.</param>
		/// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains keys and values.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Dictionary<TKey, TElement> ToDictionary<T, TKey, TElement>(
			[InstantHandle] this IEnumerable<T> source,
			[InstantHandle] Func<T, TKey> keySelector,
			[InstantHandle] Func<T, TElement> elementSelector,
			IEqualityComparer<TKey>? comparer,
			DictionaryDuplicate duplicateHandling)
			where TKey : notnull
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(keySelector, nameof(keySelector));
			Code.NotNull(elementSelector, nameof(elementSelector));
			Code.InRange(
				(int)duplicateHandling, nameof(duplicateHandling), (int)DictionaryDuplicate.Throw, (int)DictionaryDuplicate.LastWins);

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