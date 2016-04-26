using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using static CodeJam.RangesV2.RangeConstants;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// Describes a range of the values
	/// </summary>
	/// <typeparam name="T">
	/// The type of the value. Should implement <seealso cref="IComparable{T}"/> or <seealso cref="IComparable"/>.
	/// </typeparam>
	[Serializable]
	[PublicAPI]
	//[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public partial struct Range<T> : IRangeFactory<T, Range<T>>, IFormattable
	{
		#region Static members

		#region Predefined values
		/// <summary> Empty range, ∅ </summary>
		public static readonly Range<T> Empty = new Range<T>(RangeBoundary<T>.Empty, RangeBoundary<T>.Empty);

		/// <summary> Infinite range, (-∞..+∞) </summary>
		public static readonly Range<T> Infinity = new Range<T>(
			RangeBoundary<T>.NegativeInfinity, RangeBoundary<T>.PositiveInfinity);
		#endregion

		#endregion

		#region Fields & .ctor()
		/// <summary> Creates instance of <seealso cref="Range{T}"/> </summary>
		/// <param name="from">Boundary from</param>
		/// <param name="to">Boundary to</param>
		public Range(RangeBoundary<T> from, RangeBoundary<T> to)
		{
			if (from.IsNotEmpty || to.IsNotEmpty)
			{
				// TODO: uncomment
				//Range.ValidateFrom(from);
				//Range.ValidateTo(to);
				if (to < from)
				{
					throw CodeExceptions.ArgumentOutOfRange(nameof(to), to, from, RangeBoundary<T>.PositiveInfinity);
				}
			}

			From = from;
			To = to;
		}

		/// <summary> Creates instance of <seealso cref="Range{T}"/> </summary>
		/// <param name="fromValue">
		/// The value of the from boundary. Infinite (or empty) boundaries should use default(T) as the value.
		/// </param>
		/// <param name="fromKind">The kind of the from boundary.</param>
		/// <param name="toValue">
		/// The value of the to boundary. Infinite (or empty) boundaries should use default(T) as the value.
		/// </param>
		/// <param name="toKind">The kind of the to boundary.</param>
		public Range(T fromValue, RangeBoundaryKind fromKind, T toValue, RangeBoundaryKind toKind)
			: this(new RangeBoundary<T>(fromValue, fromKind), new RangeBoundary<T>(toValue, toKind))
		{ }
		#endregion

		#region Properties
		/// <summary> From boundary. Limits the values from left </summary>
		public RangeBoundary<T> From { get; }

		/// <summary> To boundary. Limits the values from right </summary>
		public RangeBoundary<T> To { get; }

		/// <summary> The value of From boundary </summary>
		public T FromValue => From.Value;

		/// <summary> The value of To boundary </summary>
		public T ToValue => To.Value;

		/// <summary> The range is empty, ∅ </summary>
		public bool IsEmpty => From.IsEmpty;

		/// <summary> The range is NOT empty, ≠ ∅ </summary>
		public bool IsNotEmpty => From.IsNotEmpty;

		/// <summary> Zero length range (From matches To) </summary>
		public bool IsSinglePoint => From.CompareTo(To) == 0;

		/// <summary> Infinite range (-∞..+∞) </summary>
		public bool IsInfinity => From.IsNegativeInfinity && To.IsPositiveInfinity;
		#endregion

		#region IRangeFactory members
		/// <summary> Creates a new instance of the range </summary>
		Range<T> IRangeFactory<T, Range<T>>.CreateRange(RangeBoundary<T> from, RangeBoundary<T> to) =>
			new Range<T>(from, to);
		#endregion

		#region ToString
		/// <summary> Returns string representation of the range </summary>
		/// <returns> The string representation of the range </returns>
		public override string ToString() =>
			IsEmpty ? EmptyString : From + SeparatorString + To;

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored
		/// </summary>
		/// <param name="format">The format string</param>
		/// <returns>
		/// The string representation of the range
		/// </returns>
		[NotNull]
		public string ToString(string format) => ToString(format, null);

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored
		/// </summary>
		/// <param name="format"> The format string </param>
		/// <param name="formatProvider"> The format provider </param>
		/// <returns>
		/// The string representation of the range
		/// </returns>
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		public string ToString(string format, IFormatProvider formatProvider) =>
			IsEmpty
				? EmptyString
				: (From.ToString(format, formatProvider) + SeparatorString + To.ToString(format, formatProvider));
		#endregion
	}
}