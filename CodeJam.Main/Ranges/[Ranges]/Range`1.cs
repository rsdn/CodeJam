using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam.Strings;

using JetBrains.Annotations;

using static CodeJam.Ranges.RangeInternal;
using static CodeJam.Targeting.MethodImplOptionsEx;

// The file contains members to be shared between Range<T> and Range<T, TKey>.

namespace CodeJam.Ranges
{
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	[Serializable]
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	//[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public partial struct Range<T> : IEquatable<Range<T>>, IFormattable
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
		// DONTTOUCH: DO NOT mark fields as readonly. See NestedStructAccessPerfTests as a proof WHY.
		private RangeBoundaryFrom<T> _from;
		private RangeBoundaryTo<T> _to;

		/// <summary>Creates instance of <seealso cref="Range{T}"/></summary>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		public Range(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
		{
			var fromEmpty = from.IsEmpty;
			var toEmpty = to.IsEmpty;
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
						$"Invalid range {from.ToInvariantString()}..{to.ToInvariantString()}.");
				}
			}

			_from = from;
			_to = to;
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
			_from = from;
			_to = to;
		}
#endif
		#endregion

		#region Properties
		/// <summary>Boundary From. Limits the values from the left.</summary>
		/// <value>Boundary From.</value>
		public RangeBoundaryFrom<T> From => _from;

		/// <summary>Boundary To. Limits the values from the right.</summary>
		/// <value>Boundary To.</value>
		public RangeBoundaryTo<T> To => _to;

		/// <summary>The value of Boundary From.</summary>
		/// <value>The value of Boundary From or InvalidOperationException, if From.HasValue is <c>false</c>.</value>
		/// <exception cref="InvalidOperationException">Thrown if From.HasValue is <c>false</c>.</exception>
		public T FromValue => _from.Value;

		/// <summary>The value of Boundary To.</summary>
		/// <value>The value of Boundary To or InvalidOperationException, if To.HasValue is <c>false</c>.</value>
		/// <exception cref="InvalidOperationException">Thrown if To.HasValue is <c>false</c>.</exception>
		public T ToValue => _to.Value;

		/// <summary>The range is empty, ∅.</summary>
		/// <value><c>true</c> if the range is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => _from.IsEmpty;

		/// <summary>The range is NOT empty, ≠ ∅.</summary>
		/// <value><c>true</c> if the range is not empty; otherwise, <c>false</c>.</value>
		public bool IsNotEmpty => _from.IsNotEmpty;

		/// <summary>
		/// The range is Zero length range (the values of the boundary From and the boundary To are the same).
		/// </summary>
		/// <value> <c>true</c> if the range is single point range; otherwise, <c>false</c>. </value>
		public bool IsSinglePoint => _from.IsNotEmpty && _from.CompareTo(_to) == 0;

		/// <summary>The range is Infinite range (-∞..+∞).</summary>
		/// <value><c>true</c> if the range is infinite; otherwise, <c>false</c>.</value>
		public bool IsInfinite => _from.IsNegativeInfinity && _to.IsPositiveInfinity;
		#endregion
	}
}