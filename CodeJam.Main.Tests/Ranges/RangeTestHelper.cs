using System;
using System.Globalization;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	public static class RangeTestHelper
	{
		#region Parse helpers
		private static RangeBoundaryFrom<T> ParseBoundaryFromCore<T>([NotNull] string value, [NotNull] Func<string, T> parseValueCallback)
		{
			if (value == RangeInternal.EmptyString)
				return RangeBoundaryFrom<T>.Empty;
			if (value == RangeInternal.NegativeInfinityBoundaryString)
				return RangeBoundaryFrom<T>.NegativeInfinity;

			RangeBoundaryFromKind kind;
			string valuePart;
			if (value.StartsWith(RangeInternal.FromInclusiveString))
			{
				kind = RangeBoundaryFromKind.Inclusive;
				valuePart = value.Substring(RangeInternal.FromInclusiveString.Length);
			}
			else if (value.StartsWith(RangeInternal.FromExclusiveString))
			{
				kind = RangeBoundaryFromKind.Exclusive;
				valuePart = value.Substring(RangeInternal.FromExclusiveString.Length);
			}
			else
			{
				throw new ArgumentException("Unknown range string format: " + value, nameof(value));
			}
			return new RangeBoundaryFrom<T>(parseValueCallback(valuePart), kind);
		}

		private static RangeBoundaryTo<T> ParseBoundaryToCore<T>(
			[NotNull] string value, [NotNull] Func<string, T> parseValueCallback)
		{
			if (value == RangeInternal.EmptyString)
				return RangeBoundaryTo<T>.Empty;
			if (value == RangeInternal.PositiveInfinityBoundaryString)
				return RangeBoundaryTo<T>.PositiveInfinity;

			RangeBoundaryToKind kind;
			string valuePart;
			if (value.EndsWith(RangeInternal.ToInclusiveString))
			{
				kind = RangeBoundaryToKind.Inclusive;
				valuePart = value.Substring(0, value.Length - RangeInternal.ToInclusiveString.Length);
			}
			else if (value.EndsWith(RangeInternal.ToExclusiveString))
			{
				kind = RangeBoundaryToKind.Exclusive;
				valuePart = value.Substring(0, value.Length - RangeInternal.ToExclusiveString.Length);
			}
			else
			{
				throw new ArgumentException("Unknown range string format: " + value, nameof(value));
			}
			return new RangeBoundaryTo<T>(parseValueCallback(valuePart), kind);
		}

		public static Range<T> ParseRange<T>([NotNull] string value, [NotNull] Func<string, T> parseValueCallback)
		{
			if (value == RangeInternal.EmptyString)
				return Range<T>.Empty;

			var boundaries = value.Split(new[] { RangeInternal.SeparatorString }, 2, StringSplitOptions.None);

			return Range.Create(
				ParseBoundaryFromCore(boundaries[0], parseValueCallback),
				ParseBoundaryToCore(boundaries[1], parseValueCallback));
		}

		public static Range<T, TKey> ParseRange<T, TKey>(
			[NotNull] string value,
			[NotNull] Func<string, T> parseValueCallback,
			[NotNull] Func<string, TKey> parseKeyCallback)
		{
			var keyAndRange = value
				.Substring(RangeInternal.KeyPrefixString.Length)
				.Split(new[] { RangeInternal.KeySeparatorString }, 2, StringSplitOptions.None);

			if (keyAndRange[1] == RangeInternal.EmptyString)
				return Range<T>.Empty.WithKey(parseKeyCallback(keyAndRange[0]));

			var boundaries = keyAndRange[1].Split(new[] { RangeInternal.SeparatorString }, 2, StringSplitOptions.None);

			return Range.Create(
				ParseBoundaryFromCore(boundaries[0], parseValueCallback),
				ParseBoundaryToCore(boundaries[1], parseValueCallback),
				parseKeyCallback(keyAndRange[0]));
		}

		public static Range<double?> ParseRangeDouble([NotNull] string value) =>
			ParseRange(value, s => (double?)double.Parse(s, CultureInfo.InvariantCulture));

		public static Range<int?, string> ParseKeyedRangeInt32([NotNull] string value) =>
			ParseRange(value, s => (int?)int.Parse(s, CultureInfo.InvariantCulture), s => s.IsNullOrEmpty() ? null : s);
		#endregion
	}
}