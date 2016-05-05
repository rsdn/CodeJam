using System;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;
using static CodeJam.RangesV2.RangeInternal;

// The file contains members to be shared between Range<T> and Range<T, TKey>.

namespace CodeJam.RangesV2
{
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	[Serializable]
	[PublicAPI]
	//[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public partial struct Range<T> : IRangeFactory<T, Range<T>>, IEquatable<Range<T>>, IFormattable
	{
		#region Static members

		#region Operators
		/// <summary>Implements the operator ==.</summary>
		/// <param name="range1">The range1.</param>
		/// <param name="range2">The range2.</param>
		/// <returns><c>True</c>, if ranges are equal.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator ==(Range<T> range1, Range<T> range2) =>
			range1.Equals(range2);

		/// <summary>Implements the operator !=.</summary>
		/// <param name="range1">The range1.</param>
		/// <param name="range2">The range2.</param>
		/// <returns><c>True</c>, if ranges are not equal.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator !=(Range<T> range1, Range<T> range2) =>
			!range1.Equals(range2);
		#endregion

		#endregion

		#region Fields & .ctor()
		/// <summary>Creates instance of <seealso cref="Range{T}"/></summary>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		public Range(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
		{
			bool fromEmpty = from.IsEmpty;
			bool toEmpty = to.IsEmpty;
			if (fromEmpty != toEmpty)
			{
				throw CodeExceptions.Argument(
					nameof(from),
					$"Both {nameof(from)} and {nameof(to)} args should be either empty or non-empty.");
			}

			if (!fromEmpty)
			{
				if (to < from)
				{
					throw CodeExceptions.Argument(
						nameof(to),
						$"The boundary {nameof(to)} should be greater than or equal to boundary {nameof(from)}.");
				}
			}

			From = from;
			To = to;
		}

		/// <summary>Creates instance of <seealso cref="Range{T}"/></summary>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		public Range(T from, T to) :
			this(Range.BoundaryFrom(from), Range.BoundaryTo(to)) { }

		/// <summary>
		/// Creates instance of <seealso cref="Range{T}"/>
		/// </summary>
		/// <param name="from">Boundary from</param>
		/// <param name="to">Boundary to</param>
		/// <param name="skipsArgValidation">Stub argument to mark unsafe (no validation) constructor overload.</param>
		[Obsolete(SkipsArgValidationObsolete)]
		internal Range(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to, UnsafeOverload skipsArgValidation)
#if DEBUG
			: this(from, to) { }
#else
		{
			From = from;
			To = to;
		}
#endif
		#endregion

		#region Properties
		/// <summary>Boundary From. Limits the values from the left.</summary>
		/// <value>Boundary From.</value>
		public RangeBoundaryFrom<T> From { get; }

		/// <summary>Boundary To. Limits the values from the right.</summary>
		/// <value>Boundary To.</value>
		public RangeBoundaryTo<T> To { get; }

		/// <summary>The value of Boundary From.</summary>
		/// <value>The value of Boundary From or InvalidOperationException, if From.HasValue is <c>false</c>.</value>
		/// <exception cref="InvalidOperationException">Thrown if From.HasValue is <c>false</c>.</exception>
		public T FromValue => From.Value;

		/// <summary>The value of Boundary To.</summary>
		/// <value>The value of Boundary To or InvalidOperationException, if To.HasValue is <c>false</c>.</value>
		/// <exception cref="InvalidOperationException">Thrown if To.HasValue is <c>false</c>.</exception>
		public T ToValue => To.Value;

		/// <summary>The range is empty, ∅.</summary>
		/// <value><c>true</c> if the range is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => From.IsEmpty;

		/// <summary>The range is NOT empty, ≠ ∅</summary>
		/// <value><c>true</c> if the range is not empty; otherwise, <c>false</c>.</value>
		public bool IsNotEmpty => From.IsNotEmpty;

		/// <summary>
		/// The range is Zero length range (the values of the boundary From and the boundary To are the same).
		/// </summary>
		/// <value> <c>true</c> if the range is single point range; otherwise, <c>false</c>. </value>
		public bool IsSinglePoint => From.IsNotEmpty && From.CompareTo(To) == 0;

		/// <summary>The range is Infinite range (-∞..+∞).</summary>
		/// <value><c>true</c> if the range is infinite; otherwise, <c>false</c>.</value>
		public bool IsInfinite => From.IsNegativeInfinity && To.IsPositiveInfinity;

		#region IEquatable<Range<T>>
		/// <summary>Indicates whether the current range and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with this. </param>
		/// <returns>
		/// <c>True</c> if <paramref name="obj"/> and the current range are the same type
		/// and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj) =>
			obj is Range<T> && Equals((Range<T>)obj);
		#endregion

		#endregion

		#region ToString
		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <returns>The string representation of the range.</returns>
		[NotNull]
		public string ToString(string format) => ToString(format, null);
		#endregion
	}
}