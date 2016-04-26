using System;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// Common interface for different range implementations
	/// </summary>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	[PublicAPI]
	public interface IRange<T>
	{
		/// <summary> From boundary. Limits the values from left </summary>
		RangeBoundary<T> From { get; }
		/// <summary> To boundary. Limits the values from right </summary>
		RangeBoundary<T> To { get; }
	}

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