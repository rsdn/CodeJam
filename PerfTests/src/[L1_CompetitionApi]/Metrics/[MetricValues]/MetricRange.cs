using System;

using CodeJam.Ranges;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>Range of metric values.</summary>
	[PublicAPI]
	public struct MetricRange : IEquatable<MetricRange>, IFormattable
	{
		#region Equality operators
		/// <summary>Implements the operator ==.</summary>
		/// <param name="a">The metric1.</param>
		/// <param name="b">The metric2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(MetricRange a, MetricRange b) => a.Equals(b);

		/// <summary>Implements the operator !=.</summary>
		/// <param name="a">The metric1.</param>
		/// <param name="b">The metric2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(MetricRange a, MetricRange b) => !a.Equals(b);
		#endregion

		#region Static members
		/// <summary>The empty metric value. Equals to <see cref="double.NaN"/>.</summary>
		public const double EmptyMetricValue = double.NaN;

		/// <summary>Negative infinity metric value. Equals to <see cref="double.NegativeInfinity"/>.</summary>
		public const double FromNegativeInfinity = double.NegativeInfinity;

		/// <summary>Positive infinity metric value. Equals to <see cref="double.PositiveInfinity"/>.</summary>
		public const double ToPositiveInfinity = double.PositiveInfinity;

		/// <summary>
		/// Empty metric values range (unset but updateable during the annotation).
		/// The <see cref="Min"/> and <see cref="Max"/> are set to <see cref="EmptyMetricValue"/>.
		/// </summary>
		public static readonly MetricRange Empty;

		/// <summary>
		/// Infinite metric values range (ignored, essentially).
		/// The <see cref="Min"/> and <see cref="Max"/> are set to
		/// <see cref="FromNegativeInfinity"/> and <see cref="ToPositiveInfinity"/>, respectively.
		/// </summary>
		public static readonly MetricRange Infinite = new MetricRange(FromNegativeInfinity, ToPositiveInfinity);
		#endregion

		#region Fields & .ctor
		private readonly Range<double> _range;

		/// <summary>Initializes a new instance of the <see cref="MetricRange"/> struct.</summary>
		/// <param name="min">
		/// The minimum value.
		/// The <see cref="EmptyMetricValue"/> marks the range as unset but updateable during the annotation.
		/// Use <seealso cref="FromNegativeInfinity"/> to set min value to negative infinity (ignored, essentially).
		/// </param>
		/// <param name="max">
		/// The maximum value.
		/// Use <see cref="EmptyMetricValue"/>
		/// to mark the range as unset but updateable during the annotation.
		/// Use <seealso cref="ToPositiveInfinity"/> to set max value to positive infinity (ignored, essentially).
		/// </param>
		public MetricRange(double min, double max) => _range = Range.Create(min, max);

		private MetricRange(Range<double> range) => _range = range;
		#endregion

		#region Range members
		/// <summary>The range is empty.</summary>
		/// <value><c>true</c> if the range is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => _range.IsEmpty;

		/// <summary>The range is NOT empty.</summary>
		/// <value><c>true</c> if the range is not empty; otherwise, <c>false</c>.</value>
		public bool IsNotEmpty => _range.IsNotEmpty;

		/// <summary>Determines whether the range contains another one.</summary>
		/// <param name="other">The metric range to check.</param>
		/// <returns><c>true</c>, if the range contains another one.</returns>
		public bool Contains(MetricRange other) => _range.Contains(other._range);

		/// <summary>Returns a union range containing both of metric ranges.</summary>
		/// <param name="other">The metric range to union with.</param>
		/// <returns>A union range containing both of the metric ranges.</returns>
		public MetricRange Union(MetricRange other) => new MetricRange(_range.Union(other._range));

		/// <summary>Gets minimum metric value.</summary>
		/// <value>
		/// The minimum metric value.
		/// The <see cref="EmptyMetricValue"/> returned if the range ie empty.
		/// The <seealso cref="FromNegativeInfinity"/> returned if the value is negative infinity (ignored, essentially).
		/// </value>
		public double Min => IsEmpty ? double.NaN : _range.From.GetValueOrDefault(FromNegativeInfinity);

		/// <summary>Gets maximum metric value.</summary>
		/// <value>
		/// The maximum metric value.
		/// The <see cref="EmptyMetricValue"/> returned if the range ie empty.
		/// The <seealso cref="ToPositiveInfinity"/> returned if the value is positive infinity (ignored, essentially).
		/// </value>
		public double Max => IsEmpty ? double.NaN : _range.To.GetValueOrDefault(ToPositiveInfinity);
		#endregion

		#region Equality members
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		public bool Equals(MetricRange other) => _range.Equals(other._range);

		/// <summary>Determines whether the <paramref name="obj"/> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="obj"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is MetricRange other && Equals(other);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode() => _range.GetHashCode();
		#endregion

		#region ToString
		/// <summary>Returns string representation of the range.</summary>
		/// <returns>The string representation of the range.</returns>
		public override string ToString() => _range.ToString();

		/// <summary>Returns string representation of the range.</summary>
		/// <param name="format">The format string.</param>
		/// <returns>The string representation of the range.</returns>
		public string ToString(string format) => ToString(format, null);

		/// <summary>Returns string representation of the range.</summary>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>The string representation of the range.</returns>
		public string ToString(IFormatProvider formatProvider) => ToString(null, formatProvider);

		/// <summary>Returns string representation of the range.</summary>
		/// <param name="format">The format string.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>The string representation of the range.</returns>
		public string ToString(string format, IFormatProvider formatProvider) =>
			_range.ToString(format, formatProvider);
		#endregion
	}
}