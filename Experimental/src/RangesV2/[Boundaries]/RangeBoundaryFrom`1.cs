using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;
using static CodeJam.RangesV2.RangeInternal;

namespace CodeJam.RangesV2
{
	/// <summary>The From boundary of the range.</summary>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	/// <remarks>
	/// Heavy tuned to be as fast as it is possible.
	/// The order of borders is the following: '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
	/// </remarks>
	[Serializable]
	[PublicAPI]
	//[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public partial struct RangeBoundaryFrom<T> :
		IEquatable<RangeBoundaryFrom<T>>,
		IComparable<RangeBoundaryFrom<T>>, IComparable<RangeBoundaryTo<T>>, IComparable<T>, IComparable,
		IFormattable
	{
		#region Static members
		private const int EqualResult = 0;

		private static readonly Func<T, T, bool> _equalsFunc = Operators<T>.AreEqual;
		private static readonly Func<T, T, int> _compareFunc = Operators<T>.Compare;

		private static readonly bool _hasNegativeInfinity = Operators<T>.HasNegativeInfinity;

		private static readonly T _negativeInfinity = Operators<T>.HasNegativeInfinity
			? Operators<T>.NegativeInfinity
			: default(T);

		private static readonly bool _hasPositiveInfinity = Operators<T>.HasPositiveInfinity;

		private static readonly T _positiveInfinity = Operators<T>.HasPositiveInfinity
			? Operators<T>.PositiveInfinity
			: default(T);

		/// <summary>Helper method to handle default and infinite values.</summary>
		/// <param name="value">The value of the boundary..</param>
		/// <param name="boundaryKind">The kind of the boundary.</param>
		[MethodImpl(AggressiveInlining)]
		internal static void CoerceBoundaryValue(ref T value, ref RangeBoundaryFromKind boundaryKind)
		{
			if (_hasNegativeInfinity && _equalsFunc(value, _negativeInfinity) && boundaryKind != RangeBoundaryFromKind.Empty)
			{
				value = default(T);
			}
			// TODO: what to do with TryCreate???
			if (_hasPositiveInfinity && _equalsFunc(value, _positiveInfinity))
			{
				throw CodeExceptions.Argument(nameof(value), "The From boundary does not accept positive infinity value.");
			}

			if (value == null)
			{
				boundaryKind = RangeBoundaryFromKind.Infinite;
			}
		}

		/// <summary>Checks if the value can be used as the value of the boundary.</summary>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c> if it is safe to pass the value as a boundary constructor parameter.</returns>
		[MethodImpl(AggressiveInlining)]
		internal static bool IsValid(T value)
		{
			if (_hasPositiveInfinity && _equalsFunc(value, _positiveInfinity))
			{
				return false;
			}
			return true;
		}

		#region Predefined values
		/// <summary>Empty range boundary, ∅.</summary>
		public static readonly RangeBoundaryFrom<T> Empty;

		/// <summary>Negative infinity, -∞.</summary>
		public static readonly RangeBoundaryFrom<T> NegativeInfinity = new RangeBoundaryFrom<T>(
			default(T), RangeBoundaryFromKind.Infinite);
		#endregion

		#region Formattable logic
		private static Func<T, string, IFormatProvider, string> GetFormattableCallback()
		{
			if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
			{
				return (value, format, formatProvider) => ((IFormattable)value).ToString(format, formatProvider);
			}
			return (value, format, formatProvider) => value.ToString();
		}

		private static readonly Func<T, string, IFormatProvider, string> _formattableCallback = GetFormattableCallback();
		#endregion

		#endregion

		#region Fields & .ctor
		private readonly T _value;
		private readonly RangeBoundaryFromKind _kind;

		/// <summary>Creates a new range boundary.</summary>
		/// <param name="value">
		/// The value of the boundary.
		/// Infinite (or empty) boundaries should use default(T) or NegativeInfinity(T) (if the type has one) as the value.
		/// </param>
		/// <param name="boundaryKind">The kind of the boundary.</param>
		public RangeBoundaryFrom(T value, RangeBoundaryFromKind boundaryKind)
		{
			if (_hasNegativeInfinity && _equalsFunc(value, _negativeInfinity) && boundaryKind != RangeBoundaryFromKind.Empty)
			{
				value = default(T);
			}
			if (_hasPositiveInfinity && _equalsFunc(value, _positiveInfinity))
			{
				throw CodeExceptions.Argument(nameof(value), "The From boundary does not accept positive infinity value.");
			}

			if (boundaryKind != RangeBoundaryFromKind.Inclusive && boundaryKind != RangeBoundaryFromKind.Exclusive)
			{
				if (_compareFunc(value, default(T)) != EqualResult)
				{
					throw CodeExceptions.Argument(nameof(value), "Value of the infinite/empty boundary should be equal to default(T).");
				}
			}
			else
			{
				if (value == null)
				{
					throw CodeExceptions.Argument(
						nameof(boundaryKind),
						"BoundaryKind for the null values should be either RangeBoundaryFromKind.Infinite or RangeBoundaryFromKind.Empty.");
				}
			}
			_value = value;
			_kind = boundaryKind;
		}

		/// <summary>Creates a new range boundary.</summary>
		/// <param name="value">
		/// The value of the boundary. Infinite (or empty) boundaries should use default(T) as the value.
		/// </param>
		/// <param name="boundaryKind">The kind of the boundary.</param>
		/// <param name="skipsArgValidation">Stub argument to mark unsafe (no validation) constructor overload.</param>
		[Obsolete(SkipsArgValidationObsolete)]
		internal RangeBoundaryFrom(T value, RangeBoundaryFromKind boundaryKind, UnsafeOverload skipsArgValidation)
#if DEBUG
			: this(value, boundaryKind) { }
#else
		{
			_value = value;
			_kind = boundaryKind;
		}
#endif
		#endregion

		#region Properties
		/// <summary>The kind of the boundary.</summary>
		/// <value>The kind of the boundary.</value>
		// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
		public RangeBoundaryFromKind Kind => _kind;

		/// <summary>The boundary == ∅.</summary>
		/// <value><c>true</c> if the boundary is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => _kind == RangeBoundaryFromKind.Empty;

		/// <summary>The boundary != ∅.</summary>
		/// <value>
		/// <c>true</c> if the boundary is not empty; otherwise, <c>false</c>.
		/// </value>
		public bool IsNotEmpty => _kind != RangeBoundaryFromKind.Empty;

		/// <summary>The boundary == -∞.</summary>
		/// <value>
		/// <c>true</c> if the boundary is negative infinity; otherwise, <c>false</c>.
		/// </value>
		public bool IsNegativeInfinity => _kind == RangeBoundaryFromKind.Infinite;

		/// <summary>The boundary includes the value.</summary>
		/// <value>
		/// <c>true</c> if the boundary is inclusive boundary; otherwise, <c>false</c>.
		/// </value>
		public bool IsInclusiveBoundary => _kind == RangeBoundaryFromKind.Inclusive;

		/// <summary>The boundary does not include the value.</summary>
		/// <value>
		/// <c>true</c> if the boundary is exclusive boundary; otherwise, <c>false</c>.
		/// </value>
		public bool IsExclusiveBoundary => _kind == RangeBoundaryFromKind.Exclusive;

		/// <summary>The boundary has value.</summary>
		/// <value><c>true</c> if the boundary has value; otherwise, <c>false</c>.</value>
		public bool HasValue => _kind == RangeBoundaryFromKind.Inclusive || _kind == RangeBoundaryFromKind.Exclusive;

		/// <summary>The value of the boundary.</summary>
		/// <value>
		/// The value of the boundary of <seealso cref="InvalidOperationException"/> if <see cref="HasValue"/> equals to <c>false</c>.
		/// </value>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HasValue"/> equals to <c>false</c>.</exception>
		[DebuggerHidden]
		public T Value
		{
			get
			{
				if (!HasValue)
				{
					throw CodeExceptions.InvalidOperation(
						"Boundary has no value. Check for HasValue property before obtaining the Value "
							+ "or use GetValueOrDefault() instead.");
				}
				return _value;
			}
		}

		/// <summary>
		/// The value of the boundary or the default(T) if <see cref="HasValue"/> property equals to <c>false</c>.
		/// </summary>
		/// <returns>he value of the boundary or default(T).</returns>
		public T GetValueOrDefault() => _value;

		/// <summary>
		/// The value of the boundary or the <paramref name="defaultValue"/> if <see cref="HasValue"/> property equals to <c>false</c>.
		/// </summary>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>Value of the boundary or <paramref name="defaultValue"/>.</returns>
		public T GetValueOrDefault(T defaultValue) => HasValue ? _value : defaultValue;
		#endregion

		#region Methods
		/// <summary>
		/// Returns complementation for the boundary. The conversions are:
		/// * 'a]' -> '(a'
		/// * '[a' -> 'a)'
		/// * 'a)' -> '[a'
		/// * '(a' -> 'a]'
		/// Empty or infinite boundaries will throw. Check the <see cref="RangeBoundaryFrom{T}.HasValue"/>
		/// before calling the method.
		/// </summary>
		/// <returns>Complementation for the boundary.</returns>
		public RangeBoundaryTo<T> GetComplementation()
		{
			RangeBoundaryToKind newKind;
			switch (_kind)
			{
				case RangeBoundaryFromKind.Inclusive:
					newKind = RangeBoundaryToKind.Exclusive;
					break;
				case RangeBoundaryFromKind.Exclusive:
					newKind = RangeBoundaryToKind.Inclusive;
					break;
				default:
					throw CodeExceptions.UnexpectedValue($" Cannot get complementation for the boundary '{this}' as it has no value.");
			}

#pragma warning disable 618 // Args are validated
			return new RangeBoundaryTo<T>(_value, newKind, SkipsArgValidation);
#pragma warning restore 618
		}

		/// <summary>Checks that the boundary is complementation for specified boundary.</summary>
		/// <param name="other">Another boundary.</param>
		/// <returns><c>True</c>, if the boundary is complementation for specified boundary.</returns>
		public bool IsComplementationFor(RangeBoundaryTo<T> other) => HasValue && GetComplementation() == other;

		/// <summary>
		/// Creates a new boundary with updated value (if the current boundary has one).
		/// If the boundary has no value the method returns the boundary unchanged.
		/// </summary>
		/// <param name="updateCallback">Callback returning new value of the boundary.</param>
		/// <returns>Range boundary with the same kind but with a new value (if the current boundary has one).</returns>
		public RangeBoundaryFrom<T> UpdateValue([NotNull, InstantHandle] Func<T, T> updateCallback)
		{
			if (HasValue)
			{
				var newValue = updateCallback(_value);

#pragma warning disable 618 // Args are validated
				return newValue == null ? NegativeInfinity : new RangeBoundaryFrom<T>(newValue, _kind, SkipsArgValidation);
#pragma warning restore 618
			}

			return this;
		}
		#endregion

		#region IEquatable<RangeBoundaryFrom<T>>
		/// <summary>Indicates whether the current boundary is equal to another.</summary>
		/// <param name="other">The boundary to compare with this.</param>
		/// <returns>
		/// <c>True</c> if the current boundary is equal to the <paramref name="other"/> parameter;
		/// otherwise, false.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public bool Equals(RangeBoundaryFrom<T> other) => _kind == other._kind && _equalsFunc(_value, other._value);

		/// <summary>Indicates whether the current boundary and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with this. </param>
		/// <returns>
		/// <c>True</c> if <paramref name="obj"/> and the current boundary are the same type
		/// and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj) => obj is RangeBoundaryFrom<T> && Equals((RangeBoundaryFrom<T>)obj);

		/// <summary>Returns the hash code for the current boundary.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			if (HasValue)
			{
				return HashCode.Combine(_value.GetHashCode(), (int)_kind);
			}

			return (int)_kind;
		}
		#endregion

		#region IComparable<RangeBoundaryFrom<T>>
		/// <summary>
		/// Compares the current boundary with another one. Following order is used:
		/// '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
		/// </summary>
		/// <param name="other">Boundary to compare with this.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings:
		/// * Less than zero This object is less than the <paramref name="other"/> parameter.
		/// * Zero This object is equal to <paramref name="other"/>.
		/// * Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(RangeBoundaryFrom<T> other)
		{
			int result;

			// If any boundary has no value - compare kinds
			if (!HasValue || !other.HasValue)
			{
				result = ((byte)_kind).CompareTo((byte)other._kind);
			}
			else
			{
				// Compare values
				result = _compareFunc(_value, other._value);

				// Are same? compare kinds.
				if (result == EqualResult)
				{
					result = ((byte)_kind).CompareTo((byte)other._kind);
				}
			}
			return result;
		}

		/// <summary>
		/// Compares the current boundary with another one. Following order is used:
		/// '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
		/// </summary>
		/// <param name="other">Boundary to compare with this.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings:
		/// * Less than zero This object is less than the <paramref name="other"/> parameter.
		/// * Zero This object is equal to <paramref name="other"/>.
		/// * Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(RangeBoundaryTo<T> other)
		{
			int result;

			// If any boundary has no value - compare kinds
			if (!HasValue || !other.HasValue)
			{
				result = ((byte)_kind).CompareTo((byte)other.Kind);
			}
			else
			{
				// Compare values
				result = _compareFunc(_value, other.Value);

				// Are same and any of is exclusive - compare kinds.
				// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
				if (result == EqualResult
					&& (_kind == RangeBoundaryFromKind.Exclusive || other.Kind == RangeBoundaryToKind.Exclusive))
				{
					result = ((byte)_kind).CompareTo((byte)other.Kind);
				}
			}
			return result;
		}

		#region IComparable<T>
		/// <summary>
		/// Compares the current boundary with the value of another From boundary. Following order is used:
		/// '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
		/// </summary>
		/// <param name="other">Boundary value to compare with this.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings:
		/// * Less than zero This object is less than the <paramref name="other"/> parameter.
		/// * Zero This object is equal to <paramref name="other"/>.
		/// * Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		// DONTTOUCH. Any change will break the performance or the correctness of the comparison. 
		//   Please create issue at first
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(T other) => 
			CompareTo(Range.GetCompareToBoundary(other));
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
		/// * Less than zero This object is less than the <paramref name="obj"/> parameter.
		/// * Zero This object is equal to <paramref name="obj"/>.
		/// * Greater than zero This object is greater than <paramref name="obj"/>.
		/// </returns>
		int IComparable.CompareTo(object obj)
		{
			var otherA = obj as RangeBoundaryFrom<T>?;
			if (otherA != null)
			{
				return CompareTo(otherA.GetValueOrDefault());
			}
			var otherB = obj as RangeBoundaryTo<T>?;
			if (otherB != null)
			{
				return CompareTo(otherB.GetValueOrDefault());
			}

			return CompareTo((T)obj);
		}
		#endregion

		#endregion

		#region ToString
		/// <summary> Returns string representation of the boundary. </summary>
		/// <returns> The string representation of the boundary. </returns>
		public override string ToString()
		{
			// DONTTOUCH: do not convert this into switch with multiple returns.
			// I've tried and it looks ugly.
			string result;
			switch (_kind)
			{
				case RangeBoundaryFromKind.Empty:
					result = EmptyString;
					break;
				case RangeBoundaryFromKind.Infinite:
					result = NegativeInfinityBoundaryString;
					break;
				case RangeBoundaryFromKind.Inclusive:
					result = FromInclusiveString + _value;
					break;
				case RangeBoundaryFromKind.Exclusive:
					result = FromExclusiveString + _value;
					break;
				default:
					result = EmptyString;
					break;
			}

			return result;
		}

		/// <summary>
		/// Returns string representation of the boundary using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored
		/// </summary>
		/// <param name="format">The format string</param>
		/// <returns> The string representation of the boundary. </returns>
		public string ToString(string format) => ToString(format, null);

		/// <summary>
		/// Returns string representation of the boundary using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored
		/// </summary>
		/// <param name="format">The format string</param>
		/// <param name="formatProvider">The format provider</param>
		/// <returns> The string representation of the boundary. </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			// DONTTOUCH: do not convert this into switch with multiple returns.
			// I've tried and it looks ugly.
			string result;
			switch (_kind)
			{
				case RangeBoundaryFromKind.Empty:
					result = EmptyString;
					break;
				case RangeBoundaryFromKind.Infinite:
					result = NegativeInfinityBoundaryString;
					break;
				case RangeBoundaryFromKind.Inclusive:
					result = FromInclusiveString + _formattableCallback(_value, format, formatProvider);
					break;
				case RangeBoundaryFromKind.Exclusive:
					result = FromExclusiveString + _formattableCallback(_value, format, formatProvider);
					break;
				default:
					result = EmptyString;
					break;
			}
			return result;
		}
		#endregion
	}
}