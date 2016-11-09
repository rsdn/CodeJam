using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		/// <summary>
		/// Extracts <paramref name="count"/> elements from a sequence at a particular zero-based starting index.
		/// </summary>
		/// <remarks>
		/// If the starting position or count specified result in slice extending past the end of the sequence,
		/// it will return all elements up to that point. There is no guarantee that the resulting sequence will
		/// contain the number of elements requested - it may have anywhere from 0 to <paramref name="count"/>.<br/>
		/// This method is implemented in an optimized manner for any sequence implementing <c>IList{T}</c>.<br/>
		/// The result of Slice() is identical to: <c>sequence.Skip(startIndex).Take(count)</c>.
		/// </remarks>
		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence from which to extract elements.</param>
		/// <param name="startIndex">The zero-based index at which to begin slicing.</param>
		/// <param name="count">The number of items to slice out of the index.</param>
		/// <returns>
		/// A new sequence containing any elements sliced out from the source sequence.
		/// </returns>
		[NotNull, Pure]
		public static IEnumerable<T> Slice<T>([NotNull] this IEnumerable<T> source, int startIndex, int count)
		{
			if (source == null) throw new ArgumentNullException(nameof (source));
			if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof (startIndex));

			if (count <= 0)
				return Enumerable.Empty<T>();

			var list = source as IList<T>;
			if (list == null)
				return SliceImpl(source, startIndex, count);

			return startIndex != 0 || list.Count > count
				? SliceImpl(list, startIndex, count)
				: list;
		}

		private static IEnumerable<T> SliceImpl<T>(IList<T> list, int index, int count)
		{
			var total = list.Count;
			while (index < total && count-- > 0)
				yield return list[index++];
		}

		private static IEnumerable<T> SliceImpl<T>(IEnumerable<T> source, int index, int count)
		{
			using (var e = source.GetEnumerator())
			{
				while (index > 0 && e.MoveNext())
					index--;

				if (index == 0)
					while (count-- > 0 && e.MoveNext())
						yield return e.Current;
			}
		}
	}
}
