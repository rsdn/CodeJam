using System;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// Common factory interface to enable 'derived' range creation.
	/// Use case example: preserve the key of the range on range intersection.
	/// </summary>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	/// <typeparam name="TRange">
	/// The type of the resulting range
	/// </typeparam>
	[PublicAPI]
	public interface IRangeFactory<T, out TRange> : IRange<T> where TRange : IRange<T>
	{
		/// <summary> Creates a new instance of the range </summary>
		TRange CreateRange(RangeBoundary<T> from, RangeBoundary<T> to);
	}
}