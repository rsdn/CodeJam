using System;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>Common interface for different range implementations</summary>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	[PublicAPI]
	public interface IRange<T>
	{
		/// <summary>From boundary. Limits the values from left</summary>
		/// <value>From boundary.</value>
		RangeBoundary<T> From { get; }
		/// <summary>To boundary. Limits the values from right</summary>
		/// <value>To boundary.</value>
		RangeBoundary<T> To { get; }
	}
}