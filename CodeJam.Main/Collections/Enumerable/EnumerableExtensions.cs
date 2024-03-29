﻿using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Extensions for <see cref="IEnumerable{T}"/>
	/// </summary>
	[PublicAPI]
	public static partial class EnumerableExtensions
	{
		/// <summary>
		/// Produces the set union of two sequences by using the default equality comparer.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> whose distinct elements form the first set for the union.</param>
		/// <param name="elements">An <see cref="IEnumerable{T}"/> whose distinct elements form the second set for the union.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains the elements from both input sequences, excluding duplicates.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<T> Union<T>(this IEnumerable<T> source, params T[] elements) =>
			source.Union(elements.AsEnumerable());

		/// <summary>
		/// Appends specified <paramref name="element"/> to end of the collection.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="element">Element to concat.</param>
		/// <returns>Concatenated enumerable</returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T element)
		{
			Code.NotNull(source, nameof(source));

			return ConcatCore(source, element);
		}

		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		private static IEnumerable<T> ConcatCore<T>(IEnumerable<T> source, T element)
		{
			foreach (var item in source)
				yield return item;
			yield return element;
		}

		/// <summary>
		/// Appends specified <paramref name="elements" /> to end of the collection.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="elements">Elements to concat.</param>
		/// <returns>Concatenated enumerable</returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params T[] elements)
		{
			Code.NotNull(source, nameof(source));

			return ConcatCore(source, elements);
		}

		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		private static IEnumerable<T> ConcatCore<T>(IEnumerable<T> source, T[] elements)
		{
			foreach (var item in source)
				yield return item;
			foreach (var element in elements)
				yield return element;
		}

		/// <summary>
		/// Prepends specified <paramref name="elements"/> to the collection start.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="elements">Elements to prepend.</param>
		/// <returns>Concatenated enumerable</returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, params T[] elements)
		{
			Code.NotNull(source, nameof(source));

			return PrependCore(source, elements);
		}

		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		private static IEnumerable<T> PrependCore<T>(IEnumerable<T> source, T[] elements)
		{
			foreach (var element in elements)
				yield return element;
			foreach (var item in source)
				yield return item;
		}

#if LESSTHAN_NETSTANDARD21

	/// <summary>
	/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the elements of source.</typeparam>
	/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
	/// <returns>
	/// A <see cref="HashSet{T}"/> that contains keys from the input sequence.
	/// </returns>
	[Pure, System.Diagnostics.Contracts.Pure]
	public static HashSet<T> ToHashSet<T>([InstantHandle] this IEnumerable<T> source) => new(source);

#endif

		/// <summary>
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <returns>
		/// A <see cref="HashSet{T}"/> that contains keys from the input sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static HashSet<TKey> ToHashSet<T, TKey>(
			[InstantHandle] this IEnumerable<T> source,
			[InstantHandle] Func<T, TKey> keySelector) =>
				new(source.Select(keySelector));

		/// <summary>
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/> with the specified equality comparer.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use
		/// to comparing values in the set, or <c>null</c> to use the default implementation for the set type.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <returns>
		/// A <see cref="HashSet{T}"/> that contains keys from the input sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static HashSet<TKey> ToHashSet<T, TKey>(
			[InstantHandle] this IEnumerable<T> source,
			[InstantHandle] Func<T, TKey> keySelector,
			IEqualityComparer<TKey> comparer) =>
				new(source.Select(keySelector), comparer);

		/// <summary>
		/// Sorts the elements of a sequence in ascending order.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">A sequence of values to order.</param>
		/// <returns>
		/// An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IOrderedEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source) =>
			source.OrderBy(Fn<TSource>.Self);

		/// <summary>
		/// Sorts the elements of a sequence in descending order.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">A sequence of values to order.</param>
		/// <returns>
		/// An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IOrderedEnumerable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> source) =>
			source.OrderByDescending(Fn<TSource>.Self);

		/// <summary>
		/// Returns a sequence with distinct elements from the input sequence based on the specified key.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="source">The sequence to return distinct elements from.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				source.Distinct(KeyEqualityComparer.Create(keySelector));

		/// <summary>
		/// Returns a sequence with distinct elements from the input sequence based on the specified key and key comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="source">The sequence to return distinct elements from.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare values.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			IEqualityComparer<TKey> comparer)
			where TKey : notnull =>
				source.Distinct(KeyEqualityComparer.Create(keySelector, comparer));

		/// <summary>
		/// Produces the set difference of two sequences by using the specified key to compare values.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}"/> whose elements that are not also in second will be returned.</param>
		/// <param name="second">An <see cref="IEnumerable{T}"/> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <returns>
		/// A sequence that contains the set difference of the elements of two sequences.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> ExceptBy<TSource, TKey>(
			this IEnumerable<TSource> first,
			IEnumerable<TSource> second,
			Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				first.Except(second, KeyEqualityComparer.Create(keySelector));

		/// <summary>
		/// Produces the set difference of two sequences by using the specified key and <see cref="IEqualityComparer{T}"/> to compare values.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}"/> whose elements that are not also in second will be returned.</param>
		/// <param name="second">An <see cref="IEnumerable{T}"/> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare values.</param>
		/// <returns>
		/// A sequence that contains the set difference of the elements of two sequences.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> ExceptBy<TSource, TKey>(
			this IEnumerable<TSource> first,
			IEnumerable<TSource> second,
			Func<TSource, TKey> keySelector,
			IEqualityComparer<TKey> comparer)
			where TKey : notnull =>
				first.Except(second, KeyEqualityComparer.Create(keySelector, comparer));

		/// <summary>
		/// Produces the set intersection of two sequences by using the specified key to compare values.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in second will be returned.</param>
		/// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in the first sequence will be returned.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <returns>
		/// A sequence that contains the elements that form the set intersection of two sequences.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> IntersectBy<TSource, TKey>(
			this IEnumerable<TSource> first,
			IEnumerable<TSource> second,
			Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				first.Intersect(second, KeyEqualityComparer.Create(keySelector));

		/// <summary>
		/// Produces the set intersection of two sequences by using the specified key and <see cref="IEqualityComparer{T}"/> to compare values.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in second will be returned.</param>
		/// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in the first sequence will be returned.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare values.</param>
		/// <returns>
		/// A sequence that contains the elements that form the set intersection of two sequences.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> IntersectBy<TSource, TKey>(
			this IEnumerable<TSource> first,
			IEnumerable<TSource> second,
			Func<TSource, TKey> keySelector,
			IEqualityComparer<TKey> comparer)
			where TKey : notnull =>
				first.Intersect(second, KeyEqualityComparer.Create(keySelector, comparer));

		/// <summary>
		/// Produces the set union of two sequences by using the specified key to compare values.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements form the first set for the union.</param>
		/// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements form the second set for the union.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains the elements from both input sequences, excluding duplicates.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> UnionBy<TSource, TKey>(
			this IEnumerable<TSource> first,
			IEnumerable<TSource> second,
			Func<TSource, TKey> keySelector)
			where TKey : notnull =>
				first.Union(second, KeyEqualityComparer.Create(keySelector));

		/// <summary>
		/// Produces the set union of two sequences by using the specified key and <see cref="IEqualityComparer{T}"/> to compare values.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key .</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements form the first set for the union.</param>
		/// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements form the second set for the union.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare values.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains the elements from both input sequences, excluding duplicates.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> UnionBy<TSource, TKey>(
			this IEnumerable<TSource> first,
			IEnumerable<TSource> second,
			Func<TSource, TKey> keySelector,
			IEqualityComparer<TKey> comparer)
			where TKey : notnull =>
				first.Union(second, KeyEqualityComparer.Create(keySelector, comparer));

		/// <summary>
		/// Projects each element of a sequence to an <see cref="IEnumerable{T}"/> and flattens the resulting sequences into
		/// one sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">A sequence of values to project.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> whose elements are the result of invoking the one-to-many transform function
		/// on each element of the input sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<TSource> SelectMany<TSource>(
			[InstantHandle] this IEnumerable<IEnumerable<TSource>> source) =>
				source.SelectMany(Fn<IEnumerable<TSource>>.Self);

		/// <summary>
		/// Returns first element, or specified <paramref name="defaultValue"/>, if sequence is empty.
		/// </summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to return an element from.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// <c>default</c>(<typeparamref name="T"/>) if <paramref name="source"/> is empty; otherwise, the first element in
		/// <paramref name="source"/>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T FirstOrDefault<T>([InstantHandle] this IEnumerable<T> source, T defaultValue)
		{
			Code.NotNull(source, nameof(source));

			foreach (var item in source)
				return item;
			return defaultValue;
		}

		/// <summary>
		/// Returns the first element of the sequence that satisfies a condition or a specified
		/// <paramref name="defaultValue"/> if no such element is found.
		/// </summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to return an element from.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>
		/// <c>default</c>(<typeparamref name="T"/>) if <paramref name="source"/> is empty or if no element passes the test
		/// specified by <paramref name="predicate"/>; otherwise, the first element in source that passes the test specified
		/// by <paramref name="predicate"/>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T FirstOrDefault<T>(
			[InstantHandle] this IEnumerable<T> source,
			T defaultValue,
			[InstantHandle] Func<T, bool> predicate)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(predicate, nameof(predicate));

			foreach (var item in source)
				if (predicate(item))
					return item;
			return defaultValue;
		}

		/// <summary>
		/// Casts the specified sequence to <see cref="List{T}"/> if possible, or creates a <see cref="List{T}"/> from.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="List{T}"/> from.</param>
		/// <returns>
		/// A <see cref="List{T}"/> that contains elements from the input sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static List<T> AsList<T>([InstantHandle] this IEnumerable<T> source) =>
			source as List<T> ?? new List<T>(source);

		/// <summary>
		/// Casts the specified sequence to array if possible, or creates an array from.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create an array from.</param>
		/// <returns>
		/// An array that contains elements from the input sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T[] AsArray<T>([InstantHandle] this IEnumerable<T> source) =>
			source as T[] ?? source.ToArray();

		/// <summary>
		/// Returns string representations of <paramref name="source"/> items.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to create strings from.</param>
		/// <returns>Enumeration of string representation of <paramref name="source"/> elements.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<string> ToStrings<T>([InstantHandle] this IEnumerable<T> source)
		{
			Code.NotNull(source, nameof(source));

			return ToStringsCore(source);
		}

		[Pure, System.Diagnostics.Contracts.Pure]
		private static IEnumerable<string> ToStringsCore<T>(IEnumerable<T> source)
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var obj in source)
				yield return obj?.ToString() ?? "";
		}

		/// <summary>
		/// Checks, if <paramref name="item"/> is first element of <paramref name="source"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to check.</param>
		/// <param name="item">Source item to compare with first element.</param>
		/// <returns>
		/// <c>true</c>, if <paramref name="source"/> has at least one element and first element is equals to
		/// <paramref name="item"/>, otherwise <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static bool IsFirst<TSource>([InstantHandle] this IEnumerable<TSource> source, TSource item) =>
			source.IsFirst(item, EqualityComparer<TSource>.Default);

		/// <summary>
		/// Checks, if <paramref name="item"/> is first element of <paramref name="source"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to check.</param>
		/// <param name="item">Source item to compare with first element.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>
		/// <c>true</c>, if <paramref name="source"/> has at least one element and first element is equals to
		/// <paramref name="item"/>, otherwise <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static bool IsFirst<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			TSource item,
			IEqualityComparer<TSource>? comparer)
		{
			Code.NotNull(source, nameof(source));

			comparer ??= EqualityComparer<TSource>.Default;

			// Fast path
			// ReSharper disable once CollectionNeverUpdated.Local
			if (source is IList<TSource> list)
				return list.Count != 0 && comparer.Equals(item, list[0]);

			foreach (var current in source)
				return comparer.Equals(item, current);
			return false;
		}

		/// <summary>
		/// Checks, if <paramref name="item"/> is last element of <paramref name="source"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to check.</param>
		/// <param name="item">Source item to compare with last element.</param>
		/// <returns>
		/// <c>true</c>, if <paramref name="source"/> has at least one element and last element is equals to
		/// <paramref name="item"/>, otherwise <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static bool IsLast<TSource>([InstantHandle] this IEnumerable<TSource> source, TSource item) =>
			source.IsLast(item, EqualityComparer<TSource>.Default);

		/// <summary>
		/// Checks, if <paramref name="item"/> is last element of <paramref name="source"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to check.</param>
		/// <param name="item">Source item to compare with last element.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>
		/// <c>true</c>, if <paramref name="source"/> has at least one element and last element is equals to
		/// <paramref name="item"/>, otherwise <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static bool IsLast<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			TSource item,
			IEqualityComparer<TSource>? comparer)
		{
			Code.NotNull(source, nameof(source));

			comparer ??= EqualityComparer<TSource>.Default;

			// Fast path
			// ReSharper disable once CollectionNeverUpdated.Local
			if (source is IList<TSource> list)
			{
				var count = list.Count;
				return count != 0 && comparer.Equals(item, list[count - 1]);
			}

			using var en = source.GetEnumerator();
			if (en.MoveNext())
			{
				TSource current;
				do
				{
					current = en.Current;
				} while (en.MoveNext());
				return comparer.Equals(item, current);
			}
			return false;
		}
	}
}
