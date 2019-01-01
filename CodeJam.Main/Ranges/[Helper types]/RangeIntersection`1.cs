﻿using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Collections;
using CodeJam.Strings;

using JetBrains.Annotations;

using static CodeJam.Ranges.CompositeRangeInternal;

namespace CodeJam.Ranges
{
	/// <summary>
	/// Describes an intersection of multiple ranges.
	/// </summary>
	/// <typeparam name="T">The type of the range values.</typeparam>
	/// <seealso cref="System.IFormattable"/>
	[PublicAPI]
	public struct RangeIntersection<T> : IFormattable
	{
		[NotNull] private static readonly IReadOnlyList<Range<T>> _emptyRanges = Array<Range<T>>.Empty.AsReadOnly();

		private readonly IReadOnlyList<Range<T>> _ranges;

		#region Fields & .ctor()
		/// <summary>Initializes a new instance of the <see cref="RangeIntersection{T}"/> struct.</summary>
		/// <param name="intersectionRange">The intersection range.</param>
		/// <param name="ranges">Intersecting ranges.</param>
		internal RangeIntersection(

		#region T4-dont-replace
			Range<T> intersectionRange,
		#endregion
			[NotNull] Range<T>[] ranges)
		{
			DebugCode.BugIf(
				ranges.Any(r => !r.HasIntersection(intersectionRange)),
				"Ranges should intersect intersectionRange.");

			IntersectionRange = intersectionRange;
			_ranges = ranges.AsReadOnly();
		}
		#endregion

		#region T4-dont-replace
		/// <summary>The common part for all ranges in intersection.</summary>
		/// <value>The common part for all ranges in intersection.</value>
		public Range<T> IntersectionRange { get; }
		#endregion

		/// <summary>The ranges in the intersection, if any.</summary>
		/// <value>The ranges in the intersection, if any.</value>
		[NotNull]
		public IReadOnlyList<Range<T>> Ranges => _ranges ?? _emptyRanges;

		/// <summary>Gets a value indicating whether the intersection does not contain any ranges.</summary>
		/// <value><c>true</c> if the intersection does not contain any ranges; otherwise, <c>false</c>.</value>
		public bool IsEmpty => Ranges.Count == 0;

		/// <summary>Gets a value indicating whether the intersection contains any ranges.</summary>
		/// <value><c>true</c> if the intersection contains any ranges; otherwise, <c>false</c>.</value>
		public bool IsNotEmpty => Ranges.Count > 0;

		#region ToString
		/// <summary>Returns string representation of the range intersection.</summary>
		/// <returns>The string representation of the range intersection.</returns>
		public override string ToString()
		{
			if (IntersectionRange.IsEmpty)
				return RangeInternal.EmptyString;

			var intersectionRangePart = IntersectionRange.ToString();
			var rangesPart = IsEmpty
				? RangeInternal.EmptyString
				: Ranges.Select(element => element.ToString()).Join(SeparatorString);

			return intersectionRangePart +
				PrefixString +
				rangesPart +
				SuffixString;
		}

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <returns>The string representation of the range.</returns>
		[NotNull, Pure]
		public string ToString(string format) => ToString(format, null);

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored.
		/// </summary>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>The string representation of the range.</returns>
		[NotNull, Pure]
		public string ToString(IFormatProvider formatProvider) => ToString(null, formatProvider);

		/// <summary>
		/// Returns string representation of the range using the specified format string.
		/// If <typeparamref name="T"/> does not implement <seealso cref="IFormattable"/> the format string is ignored.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>The string representation of the range.</returns>
		[NotNull, Pure]
		public string ToString(string format, IFormatProvider formatProvider)
		{
			var intersectionRangePart = IntersectionRange.ToString(format, formatProvider);
			var rangesPart = IsEmpty
				? RangeInternal.EmptyString
				: Ranges.Select(element => element.ToString(format, formatProvider)).Join(SeparatorString);

			return intersectionRangePart +
				PrefixString +
				rangesPart +
				SuffixString;
		}
		#endregion
	}
}