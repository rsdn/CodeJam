
using System;
using System.Collections.Generic;

namespace CodeJam.Ranges
{
	/// <summary>
	/// Helper comparer for operations over <see cref="Range{T}.To"/>.
	/// </summary>
	/// <typeparam name="T">The type of the range values.</typeparam>
	[Serializable]
	internal sealed class RangeBoundaryToDescendingComparer<T> : IComparer<RangeBoundaryTo<T>>
	{
		/// <summary>Default comparer instance.</summary>
		public static readonly RangeBoundaryToDescendingComparer<T> Instance = new RangeBoundaryToDescendingComparer<T>();

		#region Implementation of IComparer<in Range<T>>
		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />
		/// </returns>
		public int Compare(RangeBoundaryTo<T> x, RangeBoundaryTo<T> y) =>
			// DONTTOUCH: order by descending, args are swapped.
			y.CompareTo(x);
		#endregion
	}
}