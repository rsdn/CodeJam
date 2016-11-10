using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		/// <summary>
		/// Extracts <paramref name="pageSize"/> elements from a sequence at a particular one-based page number.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence from which to page.</param>
		/// <param name="pageIndex">The one-based page number.</param>
		/// <param name="pageSize">The size of the page.</param>
		/// <returns>
		/// A new sequence containing elements are at the specified <paramref name="pageIndex"/> from the source sequence.
		/// </returns>
		[NotNull, Pure]
		public static IEnumerable<T> Page<T>([NotNull] this IEnumerable<T> source, int pageIndex, int pageSize) =>
			source.Slice((pageIndex - 1)*pageSize, pageSize);
	}
}
