using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// <see cref="Array"/> class extensions.
	/// </summary>
	[PublicAPI]
	public partial class ArrayExtensions
	{
		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <returns>
		/// true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </returns>
		[Pure]
		public static bool EqualsTo([CanBeNull] this string[]? a, [CanBeNull] string[]? b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				if (!string.Equals(a[i], b[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// A parameter specifies the culture, case, and sort rules used in the comparison.
		/// </summary>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <param name="comparison">One of the enumeration values that specifies the rules for the comparison.</param>
		/// <returns>
		/// true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </returns>
		[Pure]
		public static bool EqualsTo([CanBeNull] this string[]? a, [CanBeNull] string[]? b, StringComparison comparison)
		{
			if (comparison == StringComparison.Ordinal)
				return EqualsTo(a, b);

			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				if (!string.Equals(a[i], b[i], comparison))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <typeparam name="T">Type of array item.</typeparam>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <returns>
		/// <c>true</c> if content of <paramref name="a"/> equals to <paramref name="b"/>, <c>false</c> otherwise.
		/// </returns>
		[Pure]
		public static bool EqualsTo<T>([CanBeNull] this T[]? a, [CanBeNull] T[]? b) where T : IEquatable<T>?
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
			{
				if (a[i] != null)
				{
					if (!a[i].Equals(b[i]))
						return false;
				}
				else if (b[i] != null)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <typeparam name="T">Type of array item.</typeparam>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <returns>
		/// <c>true</c> if content of <paramref name="a"/> equals to <paramref name="b"/>, <c>false</c> otherwise.
		/// </returns>
		[Pure]
		public static bool EqualsTo<T>([CanBeNull] this T[]? a, [CanBeNull] T[]? b) where T : struct, IEquatable<T>?
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			for (var i = 0; i < a.Length; i++)
				if (a[i].HasValue != b[i].HasValue || !a[i].GetValueOrDefault().Equals(b[i].GetValueOrDefault()))
					return false;

			return true;
		}

		/// <summary>
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <typeparam name="T">Type of array item.</typeparam>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <param name="comparer">Instance of <see cref="IComparer{T}"/> to compare values.</param>
		/// <returns>
		/// <c>true</c> if content of <paramref name="a"/> equals to <paramref name="b"/>, <c>false</c> otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
		[Pure]
		public static bool EqualsTo<T>([CanBeNull] this T[]? a, [CanBeNull] T[]? b, [NotNull] IEqualityComparer<T>? comparer)
		{
			Code.NotNull(comparer, nameof(comparer));

			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			if (a.Length != b.Length)
				return false;

			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var i = 0; i < a.Length; i++)
				if (!comparer.Equals(a[i], b[i]))
					return false;

			return true;
		}

		/// <summary>
		/// Checks if any element in array exists.
		/// </summary>
		/// <typeparam name="T">Type of array item.</typeparam>
		/// <param name="array">Array to check.</param>
		/// <returns><c>True</c>, if array is not empty.</returns>
		/// <remarks>This method performs fast check instead of creating enumerator</remarks>
		[Pure]
		public static bool Any<T>([NotNull] this T[] array)
		{
			Code.NotNull(array, nameof(array));
			return array.Length != 0;
		}
	}
}
