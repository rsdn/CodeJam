using System;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Extensions for <see cref="IQueryable{T}"/>
	/// </summary>
	[PublicAPI]
	public static partial class QueryableExtensions
	{
		/// <summary>
		/// Extracts <paramref name="count"/> elements from a sequence at a particular zero-based starting index.
		/// </summary>
		/// <remarks>
		/// If the starting position or count specified result in slice extending past the end of the sequence,
		/// it will return all elements up to that point. There is no guarantee that the resulting sequence will
		/// contain the number of elements requested - it may have anywhere from 0 to <paramref name="count"/>.
		/// </remarks>
		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence from which to extract elements.</param>
		/// <param name="startIndex">The zero-based index at which to begin slicing.</param>
		/// <param name="count">The number of items to slice out of the index.</param>
		/// <returns>
		/// A new sequence containing any elements sliced out from the source sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IQueryable<T> Slice<T>(
			this IQueryable<T> source, [NonNegativeValue] int startIndex, [NonNegativeValue] int count)
		{
			if (startIndex > 0)
				source = source.Skip(startIndex);

			if (count > 0)
				source = source.Take(count);

			return source;
		}

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
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IQueryable<T> Page<T>(this IQueryable<T> source, int pageIndex, int pageSize)
		{
			if (pageIndex > 1 && pageSize > 0)
				source = source.Skip((pageIndex - 1) * pageSize);

			if (pageSize > 0)
				source = source.Take(pageSize);

			return source;
		}
	}
}