using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		/// <summary>
		/// Associates an index to each element of the source sequence.
		/// </summary>
		/// <param name="source">The input sequence.</param>
		/// <returns>
		/// A sequence of elements paired with their index in the sequence.
		/// </returns>
		[NotNull, Pure]
		public static IEnumerable<IndexedItem<T>> WithIndex<T>([NotNull] this IEnumerable<T> source)
		{
			Code.NotNull(source, nameof(source));
			return IndexImpl(source);
		}

		private static IEnumerable<IndexedItem<T>> IndexImpl<T>(IEnumerable<T> source)
		{
			using (var enumerator = source.GetEnumerator())
				if (enumerator.MoveNext())
				{
					var index = 0;
					var isFirst = true;
					var isLast = false;

					while (!isLast)
					{
						var item = enumerator.Current;
						isLast = !enumerator.MoveNext();

						yield return new IndexedItem<T>(item, index++, isFirst, isLast);
						isFirst = false;
					}
				}
		}
	}
}
