using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;
using static CodeJam.RangesV2.RangeInternal;

namespace CodeJam.RangesV2
{
	// The file contains members that should not be copied into Range<T, TKey>. DO NOT remove it

	/// <summary>Describes a range of the values.</summary>
	public partial struct Range<T>
	{
		#region Predefined values
		/// <summary>Empty range, ∅</summary>
		public static readonly Range<T> Empty = new Range<T>(RangeBoundaryFrom<T>.Empty, RangeBoundaryTo<T>.Empty);

		/// <summary>Infinite range, (-∞..+∞)</summary>
		public static readonly Range<T> Infinite = new Range<T>(
			RangeBoundaryFrom<T>.NegativeInfinity, RangeBoundaryTo<T>.PositiveInfinity);
		#endregion

		#region IRangeFactory members
		/// <summary>Creates a new instance of the range.</summary>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns>A new instance of the range with specified From-To boundaries.</returns>
		[Pure]
		Range<T> IRangeFactory<T, Range<T>>.CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new Range<T>(from, to);

		/// <summary>Creates a new instance of the range, if possible.</summary>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns>
		/// A new instance of the range with specified From-To boundaries,
		/// or empty range, if from-to boundaries forms invalid range pair.
		/// </returns>
		[Pure]
		Range<T> IRangeFactory<T, Range<T>>.TryCreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			Range.TryCreate(from, to);
		#endregion

		#region IEquatable<Range<T>>
		/// <summary>Indicates whether the current range is equal to another.</summary>
		/// <param name="other">An range to compare with this.</param>
		/// <returns>
		/// <c>True</c> if the current range is equal to the <paramref name="other"/> parameter;
		/// otherwise, false.
		/// </returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public bool Equals(Range<T> other) =>
			_from == other._from && _to == other._to;


		/// <summary>Returns a hash code for the current range.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		[Pure]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Read the comment on the fields.")]
		public override int GetHashCode() =>
			HashCode.Combine(_from.GetHashCode(), _to.GetHashCode());
		#endregion

		#region ToString
		/// <summary>Returns string representation of the range.</summary>
		/// <returns>The string representation of the range.</returns>
		[Pure]
		public override string ToString() =>
			IsEmpty ? EmptyString : _from + SeparatorString + _to;

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>The string representation of the range.</returns>
		[Pure]
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		public string ToString(string format, IFormatProvider formatProvider) =>
			IsEmpty
				? EmptyString
				: (_from.ToString(format, formatProvider) + SeparatorString + _to.ToString(format, formatProvider));
		#endregion
	}
}