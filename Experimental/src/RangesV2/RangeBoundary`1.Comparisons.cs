using System;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;

using static CodeJam.PlatformDependent;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// The boundary of the range.
	/// </summary>
	partial struct RangeBoundary<T> :
		IEquatable<RangeBoundary<T>>, IComparable<RangeBoundary<T>>, IComparable<T>, IComparable
	{
		#region Core logic
		private const int EqualResult = 0;

		// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
		private const RangeBoundaryKind InfinityMask = RangeBoundaryKind.NegativeInfinity | RangeBoundaryKind.PositiveInfinity;
		private const RangeBoundaryKind InclusiveMask = RangeBoundaryKind.FromInclusive | RangeBoundaryKind.ToInclusive;
		private const RangeBoundaryKind ExclusiveMask = RangeBoundaryKind.FromExclusive | RangeBoundaryKind.ToExclusive;

		private static readonly Func<T, T, bool> _equalsFunc = Operators<T>.AreEqual;
		private static readonly Func<T, T, int> _compareFunc = Operators<T>.Compare;

		private const RangeBoundaryKind ToMask =
			RangeBoundaryKind.ToExclusive | RangeBoundaryKind.ToInclusive | RangeBoundaryKind.PositiveInfinity;

		private const RangeBoundaryKind FromMask =
			RangeBoundaryKind.FromExclusive | RangeBoundaryKind.FromInclusive | RangeBoundaryKind.NegativeInfinity;

		[MethodImpl(AggressiveInlining)]
		private static bool IsInfinityKind(RangeBoundaryKind boundaryKind) => (boundaryKind & InfinityMask) != 0;

		[MethodImpl(AggressiveInlining)]
		private static bool IsInclusiveKind(RangeBoundaryKind boundaryKind) => (boundaryKind & InclusiveMask) != 0;

		[MethodImpl(AggressiveInlining)]
		private static bool IsExclusiveKind(RangeBoundaryKind boundaryKind) => (boundaryKind & ExclusiveMask) != 0;

		[MethodImpl(AggressiveInlining)]
		private static bool IsToKind(RangeBoundaryKind boundaryKind) => (boundaryKind & ToMask) != 0;

		[MethodImpl(AggressiveInlining)]
		private static bool IsFromKind(RangeBoundaryKind boundaryKind) => (boundaryKind & FromMask) != 0;

		[MethodImpl(AggressiveInlining)]
		private static bool HasNoValue(RangeBoundaryKind boundaryKind)
		{
			switch (boundaryKind)
			{
				case RangeBoundaryKind.Empty:
				case RangeBoundaryKind.NegativeInfinity:
				case RangeBoundaryKind.PositiveInfinity:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Core comparison logic. Following order is used:
		/// '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
		/// </summary>
		// DONTTOUCH. Any change will break the performance or the correctness of the comparison. 
		//   Please create issue at first
		[MethodImpl(AggressiveInlining)]
		private static int CompareCore(
			T value1, RangeBoundaryKind boundaryKind1,
			T value2, RangeBoundaryKind boundaryKind2)
		{
			int result;

			// If any boundary has no value - compare kinds
			if (HasNoValue(boundaryKind1) || HasNoValue(boundaryKind2))
			{
				result = boundaryKind1.CompareTo(boundaryKind2);
			}
			else
			{
				// Compare values
				result = _compareFunc(value1, value2);

				// Are same? compare kinds.
				// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
				if (result == EqualResult && IsExclusiveKind(boundaryKind1 | boundaryKind2))
				{
					result = boundaryKind1.CompareTo(boundaryKind2);
				}
			}
			return result;
		}

		// ReSharper restore BitwiseOperatorOnEnumWithoutFlags 
		#endregion

		#region Operators
		/// <summary> Equality operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundaries are equal.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator ==(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			boundary1.Equals(boundary2);

		/// <summary> Inequality operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundaries are not equal.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator !=(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			!boundary1.Equals(boundary2);

		/// <summary> Greater than operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 > boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator >(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			boundary1.CompareTo(boundary2) > 0;

		/// <summary> Greater than or equal operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 >= boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator >=(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			boundary1.CompareTo(boundary2) >= 0;

		/// <summary> Less than operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 &lt; boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator <(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			boundary1.CompareTo(boundary2) < 0;

		/// <summary> Less than or equal operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 &lt;= boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator <=(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			boundary1.CompareTo(boundary2) <= 0;

		/// <summary> Greater than operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">The value of the boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 > boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator >(RangeBoundary<T> boundary1, T boundary2) =>
			boundary1.CompareTo(boundary2) > 0;

		/// <summary> Greater than or equal operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">The value of the boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 >= boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator >=(RangeBoundary<T> boundary1, T boundary2) =>
			boundary1.CompareTo(boundary2) >= 0;

		/// <summary> Less than operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">The value of the boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 &lt; boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator <(RangeBoundary<T> boundary1, T boundary2) =>
			boundary1.CompareTo(boundary2) < 0;

		/// <summary> Less than or equal operator. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">The value of the boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 &lt;= boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator <=(RangeBoundary<T> boundary1, T boundary2) =>
			boundary1.CompareTo(boundary2) <= 0;

		/// <summary> Greater than operator. </summary>
		/// <param name="boundary1">The value of the boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 > boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator >(T boundary1, RangeBoundary<T> boundary2) =>
			boundary2.CompareTo(boundary1) < 0;

		/// <summary> Greater than or equal operator. </summary>
		/// <param name="boundary1">The value of the boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 >= boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator >=(T boundary1, RangeBoundary<T> boundary2) =>
			boundary2.CompareTo(boundary1) <= 0;

		/// <summary> Less than operator. </summary>
		/// <param name="boundary1">The value of the boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 &lt; boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator <(T boundary1, RangeBoundary<T> boundary2) =>
			boundary2.CompareTo(boundary1) > 0;

		/// <summary> Less than or equal operator. </summary>
		/// <param name="boundary1">The value of the boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundary1 &lt;= boundary2.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool operator <=(T boundary1, RangeBoundary<T> boundary2) =>
			boundary2.CompareTo(boundary1) >= 0;
		#endregion

		#region CLS-friendly operators
		/// <summary> Equality comparison method. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns><c>True</c>, if boundaries are equal.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool Equals(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			boundary1 == boundary2;

		/// <summary> Comparison method. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns>
		/// <c>0</c>, if boundaries are equal,
		/// Negative value if boundary1 &lt; boundary2,
		/// Positive value if boundary1 > boundary2,
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public static int CompareTo(RangeBoundary<T> boundary1, RangeBoundary<T> boundary2) =>
			boundary1.CompareTo(boundary2);

		/// <summary> Comparison method. </summary>
		/// <param name="boundary1">Boundary 1.</param>
		/// <param name="boundary2">The value of the boundary 2.</param>
		/// <returns>
		/// <c>0</c>, if boundaries are equal,
		/// Negative value if boundary1 &lt; boundary2,
		/// Positive value if boundary1 > boundary2,
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public static int CompareTo(RangeBoundary<T> boundary1, T boundary2) =>
			boundary1.CompareTo(boundary2);

		/// <summary> Comparison method. </summary>
		/// <param name="boundary1">The value of the boundary 1.</param>
		/// <param name="boundary2">Boundary 2.</param>
		/// <returns>
		/// <c>0</c>, if boundaries are equal,
		/// Negative value if boundary1 &lt; boundary2,
		/// Positive value if boundary1 > boundary2,
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public static int CompareTo(T boundary1, RangeBoundary<T> boundary2)
		{
			// DONTTOUCH: (-result) may result in overflow.
			var result = boundary2.CompareTo(boundary1);

			if (result > 0)
				return -1;
			if (result < 0)
				return 1;
			return 0;
		}
		#endregion

		#region IEquatable<RangeBoundary<T>>
		/// <summary>Indicates whether the current boundary is equal to another.</summary>
		/// <param name="other">An boundary to compare with this.</param>
		/// <returns>
		/// <c>True</c> if the current boundary is equal to the <paramref name="other" /> parameter;
		/// otherwise, false.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public bool Equals(RangeBoundary<T> other) =>
			_kind == other._kind &&
			_equalsFunc(_value, other._value);

		/// <summary>Indicates whether the current boundary and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with this. </param>
		/// <returns>
		/// <c>True</c> if <paramref name="obj" /> and the current boundary are the same type
		/// and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj) =>
			obj is RangeBoundary<T> && Equals((RangeBoundary<T>)obj);

		/// <summary>Returns the hash code for the current boundary.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			if (HasValue)
			{
				return HashCode.Combine(_value.GetHashCode(), _kind.GetHashCode());
			}

			return _kind.GetHashCode();
		}
		#endregion

		#region IComparable<RangeBoundary<T>>
		/// <summary>
		/// Compares the current boundary with another one. Following order is used:
		/// '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
		/// </summary>
		/// <param name="other">Boundary value to compare with this.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings:
		/// * Less than zero This object is less than the <paramref name="other" /> parameter.
		/// * Zero This object is equal to <paramref name="other" />.
		/// * Greater than zero This object is greater than <paramref name="other" />.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(RangeBoundary<T> other) =>
			CompareCore(_value, _kind, other._value, other._kind);

		#region IComparable<T>
		/// <summary>
		/// Compares the current boundary with the value of another boundary of the same kind. Following order is used:
		/// '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
		/// </summary>
		/// <param name="other">An object to compare with this.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings:
		/// * Less than zero This object is less than the <paramref name="other" /> parameter.
		/// * Zero This object is equal to <paramref name="other" />.
		/// * Greater than zero This object is greater than <paramref name="other" />.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(T other)
		{
			var otherBoundaryKind = other == null ?
				RangeBoundaryKind.NegativeInfinity :
				(IsToBoundary ? RangeBoundaryKind.ToInclusive : RangeBoundaryKind.FromInclusive);

			return CompareCore(_value, _kind, other, otherBoundaryKind);
		}
		#endregion

		#region IComparable
		/// <summary>
		/// Compares the current boundary with the boundary or with the value of another boundary of the same kind. Following order is used:
		/// '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
		/// </summary>
		/// <param name="obj">An object to compare with this object.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings:
		/// * Less than zero This object is less than the <paramref name="obj" /> parameter.
		/// * Zero This object is equal to <paramref name="obj" />.
		/// * Greater than zero This object is greater than <paramref name="obj" />.
		/// </returns>
		int IComparable.CompareTo(object obj)
		{
			var other = obj as RangeBoundary<T>?;
			return other == null
				? CompareTo((T)obj)
				: CompareTo(other.Value);
		}
		#endregion
		#endregion
	}
}