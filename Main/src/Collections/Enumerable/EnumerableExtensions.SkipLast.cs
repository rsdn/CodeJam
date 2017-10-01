using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
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
		[NotNull, Pure]
		public static IEnumerable<T> SkipLast<T>([NotNull] this IEnumerable<T> source, int count = 1)
		{
			Code.NotNull(source, nameof (source));

			if (count <= 0)
				return Enumerable.Empty<T>();

			if (source is ICollection<T> collection)
				return count < collection.Count
					? source.Take(collection.Count - count)
					: Enumerable.Empty<T>();

			return SkipLastImpl(source, count);
		}

		private static IEnumerable<T> SkipLastImpl<T>(IEnumerable<T> source, int count)
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
