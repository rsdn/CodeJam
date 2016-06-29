using System;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

namespace CodeJam.Ranges
{
	/// <summary>Extension methods for <seealso cref="Range{T}"/>.</summary>
	[PublicAPI]
	public static partial class RangeExtensions
	{
		#region Range factory methods
		[MethodImpl(AggressiveInlining)]
		private static TRange TryCreateRange<T, TRange>(
			this TRange range,
			RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
			where TRange : IRangeFactory<T, TRange> =>
				range.TryCreateRange(from, to);
		#endregion

		/// <summary>Creates a range without a range key.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="range">The range to remove key from.</param>
		/// <returns>A new range without a key.</returns>
		[MethodImpl(AggressiveInlining)]
		public static Range<T> WithoutKey<T, TKey>(this Range<T, TKey> range) =>
			Range.Create(range.From, range.To);
	}
}