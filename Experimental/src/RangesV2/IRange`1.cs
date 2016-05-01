using System;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>Common interface for different range implementations</summary>
	/// <typeparam name="T">The type of the value. Should implement <seealso cref="IComparable{T}" /> or <seealso cref="IComparable" />.</typeparam>
	[PublicAPI]
	public interface IRange<T>
	{
		/// <summary>Boundary From. Limits the values from the left.</summary>
		/// <value>Boundary From.</value>
		RangeBoundaryFrom<T> From { get; }

		/// <summary>Boundary To. Limits the values from the right.</summary>
		/// <value>Boundary To.</value>
		RangeBoundaryTo<T> To { get; }
	}
}