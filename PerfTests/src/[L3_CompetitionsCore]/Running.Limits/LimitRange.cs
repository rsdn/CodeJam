using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;

using CodeJam.Ranges;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Limits
{
	/// <summary>
	/// Limit range.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public struct LimitRange : IEquatable<LimitRange>
	{
		#region Static members
		/// <summary>Implements the operator ==.</summary>
		/// <param name="a">The limit1.</param>
		/// <param name="b">The limit2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(LimitRange a, LimitRange b) => a._limitRange == b._limitRange;

		/// <summary>Implements the operator !=.</summary>
		/// <param name="a">The limit1.</param>
		/// <param name="b">The limit2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(LimitRange a, LimitRange b) => a._limitRange != b._limitRange;

		/// <summary>Empty limit range.</summary>
		public static LimitRange Empty;

		/// <summary>
		/// Creates a new ratio limit range. (0,0) and (null, null) pairs are threated as empty range,
		/// negative values threated as infinite.
		/// </summary>
		/// <param name="from">From value.</param>
		/// <param name="to">To value.</param>
		/// <returns>Limits range.</returns>
		public static LimitRange CreateRatioLimit(double? from, double? to)
		{
			if ((from == null && to == null) ||
				(from == 0 && to == 0))
				return Empty;

			var fromValue = from ?? double.NegativeInfinity;
			var toValue = to ?? double.PositiveInfinity;
			if (fromValue < 0)
				fromValue = double.NegativeInfinity;
			if (toValue < 0)
				toValue = double.PositiveInfinity;

			return new LimitRange(Range.TryCreate(fromValue, toValue));
		}
		#endregion

		#region Fields & ctor
		private readonly Range<double> _limitRange;

		private LimitRange(Range<double> limitRange)
		{
			_limitRange = limitRange;
		}
		#endregion

		/// <summary>The range is empty.</summary>
		/// <value><c>true</c> if the range is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => _limitRange.IsEmpty;

		/// <summary>The range is NOT empty.</summary>
		/// <value><c>true</c> if the range is not empty; otherwise, <c>false</c>.</value>
		public bool IsNotEmpty => _limitRange.IsNotEmpty;

		/// <summary>Determines whether the limit range contains another limit range.</summary>
		/// <param name="other">The limit range to check.</param>
		/// <returns><c>true</c>, if the limit range contains another limit range.</returns>
		public bool Contains(LimitRange other) => _limitRange.Contains(other._limitRange);

		/// <summary>Returns a union range containing both of limit ranges.</summary>
		/// <param name="other">The limit range to union with.</param>
		/// <returns>A union range containing both of the limit ranges.</returns>
		public LimitRange UnionWith(LimitRange other) =>
			_limitRange.Contains(other._limitRange)
				? this
				: new LimitRange(_limitRange.Union(other._limitRange));

		private string GetFormat()
		{
			// First non-empty value
			var value = _limitRange.From.GetValueOrDefault(
				_limitRange.To.GetValueOrDefault());
			
			return BenchmarkHelpers.GetAutoscaledFormat(value);
		}

		/// <summary>Returns storage string representation for min limit ratio.</summary>
		/// <value>Storage string representation for min limit ratio.</value>
		public string MinRatioText
		{
			get
			{
				var limitRange = _limitRange;

				if (limitRange.IsEmpty)
					return null;
				if (limitRange.From.IsNegativeInfinity)
					return limitRange.To.IsPositiveInfinity
						? (-1.0).ToString(HostEnvironmentInfo.MainCultureInfo)
						: null;

				return limitRange.FromValue.ToString(GetFormat(), HostEnvironmentInfo.MainCultureInfo);
			}
		}

		/// <summary>Returns storage string representation for max limit ratio.</summary>
		/// <value>Storage string representation for max limit ratio.</value>
		public string MaxRatioText
		{
			get
			{
				var limitRange = _limitRange;

				if (limitRange.IsEmpty)
					return null;
				if (limitRange.To.IsPositiveInfinity) // max should be specified if not empty.
					return (-1.0).ToString(HostEnvironmentInfo.MainCultureInfo);

				return limitRange.ToValue.ToString(GetFormat(), HostEnvironmentInfo.MainCultureInfo);
			}
		}

		/// <summary>To the display string.</summary>
		/// <returns>String representation of the limit range.</returns>
		public string ToDisplayString() =>
			_limitRange
				.ToString(GetFormat(), HostEnvironmentInfo.MainCultureInfo);

		#region Equality members
		/// <summary>Equalses the specified other.</summary>
		/// <param name="other">The other.</param>
		/// <returns>
		///   <c>true</c> if the <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(LimitRange other) => _limitRange.Equals(other._limitRange);

		/// <summary>Determines whether the <paramref name="obj"/> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the <paramref name="obj"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is LimitRange && Equals((LimitRange)obj);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode() => _limitRange.GetHashCode();
		#endregion
	}
}