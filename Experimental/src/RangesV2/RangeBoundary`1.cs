using System;
using System.Diagnostics;

using JetBrains.Annotations;

using static CodeJam.RangesV2.RangeConstants;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// The boundary of the range.
	/// </summary>
	/// <remarks>
	/// Heavy tuned to be as fast as it is possible.
	/// The order of borders is the following: '∅' &lt; '-∞' &lt; 'a)' &lt; '[a' == 'a]' &lt; '(a' &lt; '+∞'.
	/// </remarks>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	[Serializable]
	[PublicAPI]
	//[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public partial struct RangeBoundary<T> : IFormattable
	{
		#region Static members

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

		#region Predefined values
		/// <summary>
		/// Empty range boundary, ∅.
		/// </summary>
		// ReSharper disable RedundantDefaultFieldInitializer
		public static readonly RangeBoundary<T> Empty = new RangeBoundary<T>();

		// ReSharper restore RedundantDefaultFieldInitializer

		/// <summary>
		/// Negative infinity, -∞.
		/// </summary>
		public static readonly RangeBoundary<T> NegativeInfinity = new RangeBoundary<T>(
			default(T), RangeBoundaryKind.NegativeInfinity);

		/// <summary>
		/// Positive infinity, +∞.
		/// </summary>
		public static readonly RangeBoundary<T> PositiveInfinity = new RangeBoundary<T>(
			default(T), RangeBoundaryKind.PositiveInfinity);
		#endregion

		#endregion

		#region Fields & .ctor
		private readonly T _value;
		private readonly RangeBoundaryKind _kind;

		/// <summary>
		/// Creates a new range boundary.
		/// </summary>
		/// <param name="value">
		/// The value of the boundary. Infinite (or empty) boundaries should use default(T) as the value.
		/// </param>
		/// <param name="boundaryKind"> The kind of the boundary. </param>
		public RangeBoundary(T value, RangeBoundaryKind boundaryKind)
		{
			if (HasNoValue(boundaryKind))
			{
				if (_compareFunc(value, default(T)) != EqualResult)
				{
					throw CodeExceptions.Argument(
						nameof(value),
						"Value of the infinite/empty boundary should be equal to default(T)");
				}
			}
			else if (value == null)
			{
				throw CodeExceptions.Argument(
					nameof(boundaryKind),
					"BoundaryKind for the null values should be either RangeBoundaryKind.NegativeInfinity, " +
						"RangeBoundaryKind.PositiveInfinity or RangeBoundaryKind.Empty.");
			}

			_value = value;
			_kind = boundaryKind;
		}
		#endregion

		#region Properties
		/// <summary>
		/// The kind of the boundary.
		/// </summary>
		// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
		public RangeBoundaryKind Kind => _kind;

		/// <summary>
		/// The boundary == ±∞.
		/// </summary>
		public bool IsInfinity => IsInfinityKind(_kind);
		/// <summary>
		/// The boundary == +∞.
		/// </summary>
		public bool IsPositiveInfinity => _kind == RangeBoundaryKind.PositiveInfinity;
		/// <summary>
		/// The boundary == -∞.
		/// </summary>
		public bool IsNegativeInfinity => _kind == RangeBoundaryKind.NegativeInfinity;
		/// <summary>
		/// The boundary == ∅.
		/// </summary>
		public bool IsEmpty => _kind == RangeBoundaryKind.Empty;
		/// <summary>
		/// The boundary != ∅.
		/// </summary>
		public bool IsNotEmpty => _kind != RangeBoundaryKind.Empty;

		/// <summary>
		/// The boundary includes the value.
		/// </summary>
		public bool IsInclusiveBoundary => IsInclusiveKind(_kind);

		/// <summary>
		/// The boundary does not include the value.
		/// </summary>
		public bool IsExclusiveBoundary => IsExclusiveKind(_kind);

		/// <summary>
		/// The boundary limits the value from right.
		/// </summary>
		public bool IsToBoundary => IsToKind(_kind);

		/// <summary>
		/// The boundary limits the value from left.
		/// </summary>
		public bool IsFromBoundary => IsFromKind(_kind);

		/// <summary>
		/// The boundary has value.
		/// </summary>
		public bool HasValue => !HasNoValue(_kind);

		/// <summary>
		/// The value of the boundary.
		/// </summary>
		/// <returns>
		/// The value of the boundary <seealso cref="InvalidOperationException"/>,
		/// if <see cref="HasValue"/> equals to <c>false</c>.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown if <see cref="HasValue"/> equals to <c>false</c>.
		/// </exception>
		[DebuggerHidden]
		public T Value
		{
			get
			{
				if (!HasValue)
				{
					throw CodeExceptions.InvalidOperation(
						"Boundary has no value. Check for HasValue property before obtaining the Value " +
							"or use GetValueOrDefault() instead.");
				}
				return _value;
			}
		}

		/// <summary>
		/// The value of the boundary or the default(T) if <see cref="HasValue"/> property equals to <c>false</c>.
		/// </summary>
		public T GetValueOrDefault() => _value;

		/// <summary>
		/// The value of the boundary or the <paramref name="defaultValue"/> <see cref="HasValue"/> property equals to <c>false</c>.
		/// </summary>
		/// <param name="defaultValue">The default value.</param>
		public T GetValueOrDefault(T defaultValue) => HasValue ? _value : defaultValue;
		#endregion

		#region ToString
		private string DebuggerDisplay => ToString("g");

		/// <summary>
		/// Returns string representation of the boundary.
		/// </summary>
		/// <returns>
		/// The string representation of the boundary
		/// </returns>
		public override string ToString()
		{
			// DONTTOUCH: do not convert this into switch with multiple returns.
			// I've tried and it looks ugly.
			string result;
			switch (_kind)
			{
				case RangeBoundaryKind.Empty:
					result = EmptyString;
					break;
				case RangeBoundaryKind.NegativeInfinity:
					result = NegativeInfinityBoundaryString;
					break;
				case RangeBoundaryKind.ToExclusive:
					result = _value.ToString() + ToExclusiveChar;
					break;
				case RangeBoundaryKind.ToInclusive:
					result = _value.ToString() + ToInclusiveChar;
					break;
				case RangeBoundaryKind.FromInclusive:
					result = FromInclusiveChar + _value.ToString();
					break;
				case RangeBoundaryKind.FromExclusive:
					result = FromExclusiveChar + _value.ToString();
					break;
				case RangeBoundaryKind.PositiveInfinity:
					result = PositiveInfinityBoundaryString;
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
		/// <returns>
		/// The string representation of the boundary
		/// </returns>
		public string ToString(string format) => ToString(format, null);

		/// <summary>
		/// Returns string representation of the boundary using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored
		/// </summary>
		/// <param name="format">The format string</param>
		/// <param name="formatProvider">The format provider</param>
		/// <returns>
		/// The string representation of the boundary
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			// DONTTOUCH: do not convert this into switch with multiple returns.
			// I've tried and it looks ugly.
			string result;
			switch (_kind)
			{
				case RangeBoundaryKind.Empty:
					result = EmptyString;
					break;
				case RangeBoundaryKind.NegativeInfinity:
					result = NegativeInfinityBoundaryString;
					break;
				case RangeBoundaryKind.ToExclusive:
					result = _formattableCallback(_value, format, formatProvider) + ToExclusiveChar;
					break;
				case RangeBoundaryKind.ToInclusive:
					result = _formattableCallback(_value, format, formatProvider) + ToInclusiveChar;
					break;
				case RangeBoundaryKind.FromInclusive:
					result = FromInclusiveChar + _formattableCallback(_value, format, formatProvider);
					break;
				case RangeBoundaryKind.FromExclusive:
					result = FromExclusiveChar + _formattableCallback(_value, format, formatProvider);
					break;
				case RangeBoundaryKind.PositiveInfinity:
					result = PositiveInfinityBoundaryString;
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