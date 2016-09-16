using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>Common interface for different composite range implementations</summary>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	public interface ICompositeRange<T>
	{
		/// <summary>The composite range is empty, ∅.</summary>
		/// <value><c>true</c> if the range is empty; otherwise, <c>false</c>.</value>
		bool IsEmpty { get; }

		/// <summary>The composite range is NOT empty, ≠ ∅</summary>
		/// <value><c>true</c> if the range is not empty; otherwise, <c>false</c>.</value>
		bool IsNotEmpty { get; }

		/// <summary>Range that contains all subranges.</summary>
		/// <value>The containing range.</value>
		Range<T> ContainingRange { get; }

		/// <summary>Returns a sequence of merged subranges. Should be used for operations over the ranges.</summary>
		/// <returns>A sequence of merged subranges</returns>
		[NotNull]
		IEnumerable<Range<T>> GetMergedRanges();
	}
}