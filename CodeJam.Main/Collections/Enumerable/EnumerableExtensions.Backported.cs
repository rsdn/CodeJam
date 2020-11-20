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
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
		/// <returns>
		/// A <see cref="HashSet{T}"/> that contains elements from the input sequence.
		/// </returns>
		[Pure, NotNull]
		public static HashSet<T> ToHashSet<T>([NotNull, InstantHandle] this IEnumerable<T> source) => new(source);

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
		[Pure, NotNull]
		public static HashSet<T> ToHashSet<T>(
			[NotNull, InstantHandle] this IEnumerable<T> source,
			[NotNull] IEqualityComparer<T> comparer) =>
				new(source, comparer);


		/// <summary>
		/// Prepends specified <paramref name="element"/> to the collection start.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="element">Element to prepend.</param>
		/// <returns>Concatenated enumerable</returns>
		[Pure, NotNull, LinqTunnel]
		public static IEnumerable<T> Prepend<T>([NotNull] this IEnumerable<T> source, T element)
		{
			Code.NotNull(source, nameof(source));

			return PrependCore(source, element);
		}

		[Pure, NotNull, LinqTunnel]
		private static IEnumerable<T> PrependCore<T>([NotNull] IEnumerable<T> source, T element)
		{
			yield return element;
			foreach (var item in source)
				yield return item;
		}

		/// <summary>
		/// Skips a specified number of contiguous elements from the end of a sequence.
		/// </summary>
		/// <remarks>
		/// This operator uses deferred execution and streams its results.
		/// </remarks>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <param name="source">The sequence to skip the last element of.</param>
		/// <param name="count">The number of elements to skip.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that does not contains the specified number of elements from the end of the input sequence.
		/// </returns>
		[NotNull, Pure, LinqTunnel]
		public static IEnumerable<T> SkipLast<T>([NotNull] this IEnumerable<T> source, [NonNegativeValue] int count = 1)
		{
			Code.NotNull(source, nameof(source));

			if (count <= 0)
				return Enumerable.Empty<T>();

			if (source is ICollection<T> collection)
				return count < collection.Count
					? source.Take(collection.Count - count)
					: Enumerable.Empty<T>();

			return SkipLastImpl(source, count);
		}

		[NotNull]
		private static IEnumerable<T> SkipLastImpl<T>([NotNull] IEnumerable<T> source, [NonNegativeValue] int count)
		{
			var queue = new Queue<T>(count);

			foreach (var item in source)
			{
				if (queue.Count == count)
					yield return queue.Dequeue();

				queue.Enqueue(item);
			}
		}
	}
}