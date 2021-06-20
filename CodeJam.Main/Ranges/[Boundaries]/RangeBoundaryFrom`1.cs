﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;

using JetBrains.Annotations;

using static CodeJam.Ranges.RangeInternal;
using static CodeJam.Targeting.MethodImplOptionsEx;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

// The file contains members to be shared between RangeBoundaryFrom<T> and RangeBoundaryTo<T>.

namespace CodeJam.Ranges
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
		private const int _equalResult = 0;

		private static readonly Func<T?, T?, bool> _equalsFunc = Operators<T>.AreEqual;

		private static readonly Func<T?, T?, int> _compareFunc = Operators<T>.Compare;

		private static readonly bool _hasNaN = Operators<T>.HasNaN;

		private static readonly bool _hasNegativeInfinity = Operators<T>.HasNegativeInfinity;

		[AllowNull]
		private static readonly T _negativeInfinity = Operators<T>.HasNegativeInfinity
			? Operators<T>.NegativeInfinity
			: default;

		private static readonly bool _hasPositiveInfinity = Operators<T>.HasPositiveInfinity;

		[AllowNull]
		private static readonly T _positiveInfinity = Operators<T>.HasPositiveInfinity
			? Operators<T>.PositiveInfinity
			: default;

		/// <summary>
		/// Infrastructure helper method to create a boundary that handles default and infinite values.
		/// The boundaryKind should be either Inclusive or Exclusive
		/// </summary>
		/// <param name="value">The value of the boundary.</param>
		/// <param name="boundaryKind">The kind of the boundary.</param>
		/// <returns>A new range boundary.</returns>
		// DONTTOUCH: DO NOT make internal. Helper method for custom range implementations.
			[EditorBrowsable(EditorBrowsableState.Never)]
			[MethodImpl(AggressiveInlining)]
		public static RangeBoundaryFrom<T> AdjustAndCreate(T? value, RangeBoundaryFromKind boundaryKind)
		{
			DebugCode.AssertArgument(
				boundaryKind is RangeBoundaryFromKind.Inclusive or RangeBoundaryFromKind.Exclusive,
				nameof(boundaryKind),
				"The boundary kind should be be either Inclusive or Exclusive");

			if (_hasNaN && !_equalsFunc(value, value))
			{
				value = default;
				boundaryKind = RangeBoundaryFromKind.Empty;
			}
			if (_hasNegativeInfinity && _equalsFunc(value, _negativeInfinity))
			{
				value = default;
				boundaryKind = RangeBoundaryFromKind.Infinite;
			}
			if (_hasPositiveInfinity && _equalsFunc(value, _positiveInfinity))
			{
				throw CodeExceptions.Argument(nameof(value), "The positive infinity value should not be used for From boundaries.");
			}
			if (value == null && boundaryKind != RangeBoundaryFromKind.Empty)
			{
				boundaryKind = RangeBoundaryFromKind.Infinite;
			}

#pragma warning disable 618 // Validation not required: value and kind are adjusted
			return new RangeBoundaryFrom<T>(value, boundaryKind, SkipsArgValidation);
#pragma warning restore 618
		}

		/// <summary>
		/// Infrastructure helper method to check if the value can be used as the value of the boundary.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c> if it is safe to pass the value as a boundary constructor parameter.</returns>
		// DONTTOUCH: DO NOT make internal. Helper method for custom range implementations.
			[EditorBrowsable(EditorBrowsableState.Never)]
			[MethodImpl(AggressiveInlining)]
		public static bool IsValid(T? value)
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
		public static readonly RangeBoundaryFrom<T> NegativeInfinity = new(
			default, RangeBoundaryFromKind.Infinite);
		#endregion

		#region Formattable logic
		private static readonly Func<T?, string?, IFormatProvider?, string?> _formattableCallback =
			CreateFormattableCallback<T>();
		#endregion

		#endregion

		#region Fields & .ctor
		// DONTTOUCH: DO NOT mark fields as readonly. See NestedStructAccessPerfTests as a proof WHY.
#pragma warning disable IDE0044

		private T? _value;

		private RangeBoundaryFromKind _kind;

#pragma warning restore IDE0044

		/// <summary>Creates a new range boundary.</summary>
		/// <param name="value">
		/// The value of the boundary.
		/// Infinite (or empty) boundaries should use default(T) or NegativeInfinity(T) (if the type has one) as the value.
		/// </param>
		/// <param name="boundaryKind">The kind of the boundary.</param>
		public RangeBoundaryFrom(T? value, RangeBoundaryFromKind boundaryKind)
		{
			if (_hasNaN && !_equalsFunc(value, value))
			{
				value = default;
				if (boundaryKind != RangeBoundaryFromKind.Empty)
					throw CodeExceptions.Argument(nameof(value), "The NaN value should be used only for Empty boundaries.");
			}
			if (_hasNegativeInfinity && _equalsFunc(value, _negativeInfinity))
			{
				value = default;
				if (boundaryKind != RangeBoundaryFromKind.Infinite)
					throw CodeExceptions.Argument(
						nameof(value), "The negative infinity value should be used only for Infinite boundaries.");
			}
			if (_hasPositiveInfinity && _equalsFunc(value, _positiveInfinity))
				throw CodeExceptions.Argument(nameof(value), "The positive infinity value should not be used for From boundaries.");

			if (boundaryKind != RangeBoundaryFromKind.Inclusive && boundaryKind != RangeBoundaryFromKind.Exclusive)
			{
				if (_compareFunc(value, default) != _equalResult)
					throw CodeExceptions.Argument(nameof(value), "Value of the infinite/empty boundary should be equal to default(T).");
			}
			else if (value == null)
				throw CodeExceptions.Argument(
					nameof(boundaryKind),
					"BoundaryKind for the null values should be either RangeBoundaryFromKind.Infinite or RangeBoundaryFromKind.Empty.");
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
		internal RangeBoundaryFrom(T? value, RangeBoundaryFromKind boundaryKind, UnsafeOverload skipsArgValidation)
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
		[MemberNotNullWhen(true, nameof(_value))]
		[MemberNotNullWhen(true, nameof(Value))]
		public bool IsNotEmpty => _kind != RangeBoundaryFromKind.Empty;

		/// <summary>The boundary == -∞.</summary>
		/// <value>
		/// <c>true</c> if the boundary is negative infinity; otherwise, <c>false</c>.
		/// </value>
		public bool IsNegativeInfinity => _kind == RangeBoundaryFromKind.Infinite;

		/// <summary>The boundary has value (is not an infinite boundary) and does include the value.</summary>
		/// <value>
		/// <c>true</c> if the boundary is inclusive boundary; otherwise, <c>false</c>.
		/// </value>
		[MemberNotNullWhen(true, nameof(_value))]
		[MemberNotNullWhen(true, nameof(Value))]
		public bool IsInclusiveBoundary => _kind == RangeBoundaryFromKind.Inclusive;

		/// <summary>The boundary has value (is not an infinite boundary) but does not include the value.</summary>
		/// <value>
		/// <c>true</c> if the boundary is exclusive boundary; otherwise, <c>false</c>.
		/// </value>
		[MemberNotNullWhen(true, nameof(_value))]
		[MemberNotNullWhen(true, nameof(Value))]
		public bool IsExclusiveBoundary => _kind == RangeBoundaryFromKind.Exclusive;

#pragma warning disable CS8775
		/// <summary>The boundary has a value (is not an infinite boundary).</summary>
		/// <value><c>true</c> if the boundary has a value; otherwise, <c>false</c>.</value>
		[MemberNotNullWhen(true, nameof(_value))]
		[MemberNotNullWhen(true, nameof(Value))]
		public bool HasValue => _kind is RangeBoundaryFromKind.Inclusive or RangeBoundaryFromKind.Exclusive;
#pragma warning restore CS8775

		/// <summary>The value of the boundary.</summary>
		/// <value>
		/// The value of the boundary of <seealso cref="InvalidOperationException"/> if <see cref="HasValue"/> equals to <c>false</c>.
		/// </value>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HasValue"/> equals to <c>false</c>.</exception>
		public T Value
		{
			[DebuggerHidden]
			[MemberNotNullWhen(true, nameof(_value))]
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
		[Pure, System.Diagnostics.Contracts.Pure]
		[MethodImpl(AggressiveInlining)]
		public T? GetValueOrDefault() => _value;

		/// <summary>
		/// The value of the boundary or the <paramref name="defaultValue"/> if <see cref="HasValue"/> property equals to <c>false</c>.
		/// </summary>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>Value of the boundary or <paramref name="defaultValue"/>.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[MethodImpl(AggressiveInlining)]
		public T? GetValueOrDefault(T? defaultValue) => HasValue ? _value : defaultValue;
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public RangeBoundaryTo<T> GetComplementation() =>
			RangeBoundaryTo<T>.AdjustAndCreate(
				_value,
				_kind switch
				{
					RangeBoundaryFromKind.Inclusive => RangeBoundaryToKind.Exclusive,
					RangeBoundaryFromKind.Exclusive => RangeBoundaryToKind.Inclusive,
					_ => throw CodeExceptions.UnexpectedValue(
						$"Cannot get complementation for the boundary '{this}' as it has no value.")
					});

		/// <summary>Checks that the boundary is complementation for specified boundary.</summary>
		/// <param name="other">Another boundary.</param>
		/// <returns><c>True</c>, if the boundary is complementation for specified boundary.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public bool IsComplementationFor(RangeBoundaryTo<T> other) => HasValue && GetComplementation() == other;

		/// <summary>
		/// Creates a new boundary with updated value (if the current boundary has one).
		/// If the boundary has no value the method returns the boundary unchanged.
		/// </summary>
		/// <param name="newValueSelector">Callback to obtain a new value for the boundary. Used if the boundary has a value.</param>
		/// <returns>Range boundary with the same kind but with a new value (if the current boundary has one).</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public RangeBoundaryFrom<T> WithValue([InstantHandle] Func<T, T> newValueSelector)
		{
			Code.NotNull(newValueSelector, nameof(newValueSelector));

			if (HasValue)
			{
				var newValue = newValueSelector(_value);

				return AdjustAndCreate(newValue, _kind);
			}

			return this;
		}

		/// <summary>
		/// Creates a new boundary with updated value (if the current boundary has one).
		/// If the boundary has no value the method returns the boundary unchanged.
		/// </summary>
		/// <typeparam name="T2">The new type of the range value</typeparam>
		/// <param name="newValueSelector">Callback to obtain a new value for the boundary. Used if the boundary has a value.</param>
		/// <returns>Range boundary with the same kind but with a new value (if the current boundary has one).</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public RangeBoundaryFrom<T2> WithValue<T2>([InstantHandle] Func<T, T2> newValueSelector)
		{
			Code.NotNull(newValueSelector, nameof(newValueSelector));

			if (HasValue)
			{
				var newValue = newValueSelector(_value);

				return RangeBoundaryFrom<T2>.AdjustAndCreate(newValue, _kind);
			}

#pragma warning disable 618 // Validation not required: HasValue checked.
			return new RangeBoundaryFrom<T2>(default, _kind, UnsafeOverload.SkipsArgValidation);
#pragma warning restore 618
		}

		/// <summary>
		/// Creates a new boundary with exclusive boundary kind if the current boundary has a value.
		/// The original boundary is returned otherwise.
		/// </summary>
		/// <returns>
		/// Range boundary with exclusive boundary kind or the original one if the boundary has no value.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public RangeBoundaryFrom<T> ToExclusive() => IsInclusiveBoundary ? Range.BoundaryFromExclusive(_value) : this;

		/// <summary>
		/// Creates a new boundary with inclusive boundary kind if the current boundary has a value.
		/// The original boundary is returned otherwise.
		/// </summary>
		/// <returns>
		/// Range boundary with inclusive boundary kind or the original one if the boundary has no value.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public RangeBoundaryFrom<T> ToInclusive() => IsExclusiveBoundary ? Range.BoundaryFrom(_value) : this;
		#endregion

		#region IEquatable<RangeBoundaryFrom<T>>
		/// <summary>Indicates whether the current boundary is equal to another.</summary>
		/// <param name="other">The boundary to compare with this.</param>
		/// <returns>
		/// <c>True</c> if the current boundary is equal to the <paramref name="other"/> parameter;
		/// otherwise, false.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[MethodImpl(AggressiveInlining)]
		public bool Equals(RangeBoundaryFrom<T> other) => _kind == other._kind && _equalsFunc(_value, other._value);

		/// <summary>Indicates whether the current boundary and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with this. </param>
		/// <returns>
		/// <c>True</c> if <paramref name="obj"/> and the current boundary are the same type
		/// and represent the same value; otherwise, false.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public override bool Equals(object? obj) => obj is RangeBoundaryFrom<T> other && Equals(other);

		/// <summary>Returns the hash code for the current boundary.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Read the comment on the fields.")]
		public override int GetHashCode() =>
			HasValue ? HashCode.Combine(_value.GetHashCode(), (int)_kind) : (int)_kind;
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
		[Pure, System.Diagnostics.Contracts.Pure]
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(RangeBoundaryFrom<T> other)
		{
			int result;

			// If any boundary has no value - compare kinds only
			if (HasValue && other.HasValue)
			{
				// Compare values
				result = _compareFunc(_value, other._value);

				// Are same? compare kinds.
				if (result == _equalResult)
				{
					result = ((byte)_kind).CompareTo((byte)other._kind);
				}
			}
			else
			{
				result = ((byte)_kind).CompareTo((byte)other._kind);
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
		[Pure, System.Diagnostics.Contracts.Pure]
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(RangeBoundaryTo<T> other)
		{
			int result;

			// If any boundary has no value - compare kinds only
			if (HasValue && other.HasValue)
			{
				// Compare values
				result = _compareFunc(_value, other.GetValueOrDefault());

				// Are same and any of is exclusive - compare kinds.
				// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
				if (result == _equalResult
					&& (_kind == RangeBoundaryFromKind.Exclusive || other.Kind == RangeBoundaryToKind.Exclusive))
				{
					result = ((byte)_kind).CompareTo((byte)other.Kind);
				}
			}
			else
			{
				result = ((byte)_kind).CompareTo((byte)other.Kind);
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
		[Pure, System.Diagnostics.Contracts.Pure]
		[MethodImpl(AggressiveInlining)]
		public int CompareTo(T? other) => CompareTo(Range.GetCompareToBoundary(other));
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
		[Pure, System.Diagnostics.Contracts.Pure]
		int IComparable.CompareTo(object? obj) =>
			obj switch
			{
				RangeBoundaryFrom<T> rbf => CompareTo(rbf),
				RangeBoundaryTo<T> rbt => CompareTo(rbt),
				_ => CompareTo((T?)obj!) // https://github.com/dotnet/roslyn/issues/34976
				};
		#endregion

		#endregion

		#region ToString
		/// <summary> Returns string representation of the boundary. </summary>
		/// <returns> The string representation of the boundary. </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public override string ToString() =>
			_kind switch
			{
				RangeBoundaryFromKind.Empty => EmptyString,
				RangeBoundaryFromKind.Infinite => NegativeInfinityBoundaryString,
				RangeBoundaryFromKind.Inclusive => FromInclusiveString + _value,
				RangeBoundaryFromKind.Exclusive => FromExclusiveString + _value,
				_ => EmptyString
				};

		/// <summary>
		/// Returns string representation of the boundary using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored
		/// </summary>
		/// <param name="format">The format string</param>
		/// <returns> The string representation of the boundary. </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public string ToString(string format) => ToString(format, null);

		/// <summary>
		/// Returns string representation of the boundary using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored
		/// </summary>
		/// <param name="format">The format string</param>
		/// <param name="formatProvider">The format provider</param>
		/// <returns> The string representation of the boundary. </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public string ToString(string? format, IFormatProvider? formatProvider) =>
			_kind switch
			{
				RangeBoundaryFromKind.Empty => EmptyString,
				RangeBoundaryFromKind.Infinite => NegativeInfinityBoundaryString,
				RangeBoundaryFromKind.Inclusive => FromInclusiveString + _formattableCallback(_value, format, formatProvider),
				RangeBoundaryFromKind.Exclusive => FromExclusiveString + _formattableCallback(_value, format, formatProvider),
				_ => EmptyString
				};
		#endregion
	}
}