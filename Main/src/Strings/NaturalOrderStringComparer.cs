// BASEDON: the C version by Martin Pool.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Strings
{
	/// <summary>
	/// String comparisons using a "natural order" algorithm.
	/// </summary>
	[PublicAPI]
	public class NaturalOrderStringComparer : IComparer<string>
	{
		/// <summary>
		/// Gets a <see cref="NaturalOrderStringComparer"/> object
		/// that performs a string comparison using a "natural order" algorithm.
		/// </summary>
		/// <returns>
		/// A <see cref="NaturalOrderStringComparer"/> object
		/// that performs a string comparison using a "natural order" algorithm.
		/// </returns>
		[NotNull]
		public static readonly NaturalOrderStringComparer Comparer = new NaturalOrderStringComparer(false);

		/// <summary>
		/// Gets a <see cref="NaturalOrderStringComparer"/> object
		/// that performs a case-insensitive string comparison using a "natural order" algorithm.
		/// </summary>
		/// <returns>
		/// A <see cref="NaturalOrderStringComparer"/> object
		/// that performs a case-insensitive string comparison using a "natural order" algorithm.
		/// </returns>
		[NotNull]
		public static readonly NaturalOrderStringComparer IgnoreCaseComparer = new NaturalOrderStringComparer(true);

		/// <summary>
		/// Gets a <see cref="Comparison{T}"/> delegate that performs a string comparison using a "natural order" algorithm.
		/// </summary>
		/// <returns>
		/// A <see cref="Comparison{T}"/> delegate that performs a string comparison using a "natural order" algorithm.
		/// </returns>
		[NotNull]
		public static readonly Comparison<string> Comparison = (a, b) => Compare(a, b, false);

		/// <summary>
		/// Gets a <see cref="Comparison{T}"/> delegate that performs a case-insensitive
		/// string comparison using a "natural order" algorithm.
		/// </summary>
		/// <returns>
		/// A <see cref="Comparison{T}"/> delegate that performs a case-insensitive
		/// string comparison using a "natural order" algorithm.
		/// </returns>
		[NotNull]
		public static readonly Comparison<string> IgnoreCaseComparison = (a, b) => Compare(a, b, true);

		/// <summary>
		/// true to ignore case during the comparison; otherwise, false.
		/// </summary>
		private readonly bool _ignoreCase;

		/// <summary>
		/// Initializes a new instance of the <see cref="NaturalOrderStringComparer"/> class.
		/// </summary>
		private NaturalOrderStringComparer(bool ignoreCase)
		{
			_ignoreCase = ignoreCase;
		}

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="a">The first object to compare.</param>
		/// <param name="b">The second object to compare.</param>
		/// <returns>
		/// A signed integer that indicates the relative values of <paramref name="a"/> and <paramref name="b"/>,
		/// as shown in the following table.
		/// Value Meaning Less than zero <paramref name="a"/> is less than <paramref name="b"/>.
		/// Zero <paramref name="a"/> equals <paramref name="b"/>.
		/// Greater than zero <paramref name="a"/> is greater than <paramref name="b"/>.
		/// </returns>
		[Pure]
		public static int Compare([CanBeNull] string a, [CanBeNull] string b) => Compare(a, b, false);

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="a">The first object to compare.</param>
		/// <param name="b">The second object to compare.</param>
		/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
		/// <returns>
		/// A signed integer that indicates the relative values of <paramref name="a"/> and <paramref name="b"/>,
		/// as shown in the following table.
		/// Value Meaning Less than zero <paramref name="a"/> is less than <paramref name="b"/>.
		/// Zero <paramref name="a"/> equals <paramref name="b"/>.
		/// Greater than zero <paramref name="a"/> is greater than <paramref name="b"/>.
		/// </returns>
		[Pure]
		public static int Compare([CanBeNull] string a, [CanBeNull] string b, bool ignoreCase)
		{
			if (a == b)
				return 0;

			if (a == null)
				return -1;

			if (b == null)
				return 1;

			// only count the number of zeroes leading the last number compared
			var ia = SkipLeadingZeroesAndWhitespaces(a, 0);
			var ib = SkipLeadingZeroesAndWhitespaces(b, 0);

			while (true)
			{
				var ca = CharAt(a, ia);
				var cb = CharAt(b, ib);

				// process run of digits
				if (ca.IsDigit() && cb.IsDigit())
				{
					var result = CompareNumerical(a, b, ignoreCase, ref ia, ref ib);
					if (result != 0)
						return result;

					ca = CharAt(a, ia);
					cb = CharAt(b, ib);
				}

				if (ignoreCase)
				{
					ca = char.ToUpper(ca);
					cb = char.ToUpper(cb);
				}

				if (ca < cb)
					return -1;

				if (ca > cb)
					return 1;

				if (ca == char.MinValue && cb == char.MinValue)
				{
					// for cases:
					// 5.txt
					// 05.txt
					// 005.txt
					return a.Length - b.Length;
				}

				ia = SkipLeadingZeroesAndWhitespaces(a, ia + 1);
				ib = SkipLeadingZeroesAndWhitespaces(b, ib + 1);
			}
		}

		/// <summary>
		/// Compares numerical strings starting from non-zeroes
		/// </summary>
		private static int CompareNumerical(string a, string b, bool ignoreCase, ref int ia, ref int ib)
		{
			var bias = 0;

			// The longest run of digits wins. That aside, the greatest
			// value wins, but we can't know that it will until we've scanned
			// both numbers to know that they have the same magnitude, so we
			// remember it in BIAS.
			for (; ; ia++, ib++)
			{
				var ca = CharAt(a, ia);
				var cb = CharAt(b, ib);

				if (!ca.IsDigit())
				{
					// number in a is shorter than number in b
					if (cb.IsDigit())
						return -1;

					// numbers have the same length
					return bias;
				}

				// number in b is shorter than number in a
				if (!cb.IsDigit())
					return 1;

				if (bias != 0)
					continue;

				if (ignoreCase)
				{
					ca = char.ToUpper(ca);
					cb = char.ToUpper(cb);
				}

				if (ca < cb)
				{
					bias = -1;
				}
				else if (ca > cb)
				{
					bias = 1;
				}
			}
		}

		private static int SkipLeadingZeroesAndWhitespaces(string text, int index)
		{
			for (; index < text.Length; index++)
			{
				var ch = text[index];
				if (!ch.IsWhiteSpace() && ch != '0')
					return index;
			}
			return index;
		}

		private static char CharAt(string text, int index) => index < text.Length ? text[index] : char.MinValue;

		#region Implementation of the IComparer<string>
		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>,
		/// as shown in the following table.
		/// Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>.
		/// Zero <paramref name="x"/> equals <paramref name="y"/>.
		/// Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
		/// </returns>
		int IComparer<string>.Compare(string x, string y) => Compare(x, y, _ignoreCase);
		#endregion
	}
}
