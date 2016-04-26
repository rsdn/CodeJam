using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using static CodeJam.PlatformDependent;

namespace CodeJam.RangesV2
{
	/// <summary>Describes a range of the values</summary>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	public partial struct Range<T> : IEquatable<Range<T>>
	{
		#region Operators
		/// <summary>Implements the operator ==.</summary>
		/// <param name="range1">The range1.</param>
		/// <param name="range2">The range2.</param>
		/// <returns><c>True</c>, if ranges are equal</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator ==(Range<T> range1, Range<T> range2) =>
			range1.Equals(range2);

		/// <summary>Implements the operator !=.</summary>
		/// <param name="range1">The range1.</param>
		/// <param name="range2">The range2.</param>
		/// <returns><c>True</c>, if ranges are inequal.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator !=(Range<T> range1, Range<T> range2) =>
			!range1.Equals(range2);
		#endregion

		#region CLS-friendly operators
		/// <summary> Equality comparison method. </summary>
		/// <param name="range1">The range1.</param>
		/// <param name="range2">The range2.</param>
		/// <returns><c>True</c>, if ranges are equal.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool Equals(Range<T> range1, Range<T> range2) =>
			range1 == range2;
		#endregion

		#region IEquatable<Range<T>>
		/// <summary>Indicates whether the current range is equal to another.</summary>
		/// <param name="other">An range to compare with this.</param>
		/// <returns>
		/// <c>True</c> if the current range is equal to the <paramref name="other"/> parameter;
		/// otherwise, false.
		/// </returns>
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		public bool Equals(Range<T> other) =>
			IsEmpty
				? other.IsEmpty
				: (From == other.From && To == other.To);

		/// <summary>Indicates whether the current range and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with this. </param>
		/// <returns>
		/// <c>True</c> if <paramref name="obj"/> and the current range are the same type
		/// and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj) =>
			obj is Range<T> && Equals((Range<T>)obj);

		/// <summary>Returns a hash code for the current range.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode() =>
			HashCode.Combine(From.GetHashCode(), To.GetHashCode());
		#endregion
	}
}