using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		/// <summary>
		/// Returns a specified number of contiguous elements from the end of a sequence.
		/// </summary>
		/// <remarks>
		/// This operator uses deferred execution and streams its results.
		/// </remarks>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <param name="source">The sequence to return the last element of.</param>
		/// <param name="count">The number of elements to return.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains the specified number of elements from the end of the input sequence.
		/// </returns>
		[NotNull, Pure]
		public static IEnumerable<T> TakeLast<T>([NotNull] this IEnumerable<T> source, int count)
		{
			Code.NotNull(source, nameof (source));

			if (count <= 0)
				return Enumerable.Empty<T>();

			var list = source as IList<T>;
			if (list == null)
				return TakeLastImpl(source, count);

			return count < list.Count
				? TakeLastImpl(list, count)
				: list;
		}

		private static IEnumerable<T> TakeLastImpl<T>(IList<T> source, int count)
		{
			var total = source.Count;
			count = Math.Min(total, count);

			for (var i = total - count; i < total; i++)
				yield return source[i];
		}

		private static IEnumerable<T> TakeLastImpl<T>(IEnumerable<T> source, int count)
		{
			var queue = new Queue<T>(count);
			foreach (var item in source)
			{
				if (queue.Count == count)
					queue.Dequeue();

				queue.Enqueue(item);
			}

			foreach (var item in queue)
				yield return item;
		}
	}
}
