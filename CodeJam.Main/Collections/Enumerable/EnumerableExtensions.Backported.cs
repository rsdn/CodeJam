using System.Collections;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace CodeJam.Collections.Backported
{
	/// <summary>
	/// Extensions for <see cref="IEnumerable{T}"/>
	/// </summary>
	[PublicAPI]
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/> with the specified equality comparer.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use
		/// to comparing values in the set, or <c>null</c> to use the default implementation for the set type.</param>
		/// <returns>
		/// A <see cref="HashSet{T}"/> that contains elements from the input sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static HashSet<T> ToHashSet<T>(
			[InstantHandle] this IEnumerable<T> source,
			IEqualityComparer<T> comparer) =>
				new(source, comparer);

		/// <summary>
		/// Prepends specified <paramref name="element"/> to the collection start.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="element">Element to prepend.</param>
		/// <returns>Concatenated enumerable</returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T element)
		{
			Code.NotNull(source, nameof(source));

			return PrependCore(source, element);
		}

		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		private static IEnumerable<T> PrependCore<T>(IEnumerable<T> source, T element)
		{
			yield return element;
			foreach (var item in source)
				yield return item;
		}

		/// <summary>
		/// Returns number of elements in <paramref name="source"/> without enumeration if possible.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to check.</param>
		/// <param name="count">Number of elements in <paramref name="source"/>.</param>
		/// <returns><c>true</c>, if it is possible to get number of elements without enumeration, otherwise <c>false</c>.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static bool TryGetNonEnumeratedCount<TSource>(
			[InstantHandle, NoEnumeration] this IEnumerable<TSource> source,
			out int count)
		{
#if NET6_0
			return Enumerable.TryGetNonEnumeratedCount(source, out count);
#else
			switch (source)
			{
				case ICollection<TSource> col:
					count = col.Count;
					return true;
				case ICollection col:
					count = col.Count;
					return true;
				default:
					count = 0;
					return false;
			}
#endif
		}
	}
}