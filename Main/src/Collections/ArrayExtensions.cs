using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// <see cref="Array" /> class extensions.
	/// </summary>
	[PublicAPI]
	public partial class ArrayExtensions
	{
		/// <summary>
		/// Returns true, if length and content of <paramref name="a" /> equals <paramref name="b" />.
		/// </summary>
		/// <typeparam name="T">Type of array item.</typeparam>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <param name="comparer">Instance of <see cref="IComparer{T}" /> to compare values.</param>
		/// <returns>
		/// <c>true</c> if content of <paramref name="a"/> equals to <paramref name="b"/>, <c>false</c> otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
		[Pure]
		public static bool EqualsTo<T>([CanBeNull] this T[] a, [CanBeNull] T[] b, [NotNull] IEqualityComparer<T> comparer)
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
		/// Returns true, if length and content of <paramref name="a"/> equals <paramref name="b"/>.
		/// </summary>
		/// <typeparam name="T">Type of array item.</typeparam>
		/// <param name="a">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <returns>
		/// <c>true</c> if content of <paramref name="a"/> equals to <paramref name="b"/>, <c>false</c> otherwise.
		/// </returns>
		[Pure]
		public static bool EqualsTo<T>([CanBeNull] this T[] a, [CanBeNull] T[] b) =>
			EqualsTo(a, b, EqualityComparer<T>.Default);

		/// <summary>
		/// Checks if any element in array exists.
		/// </summary>
		/// <typeparam name="T">Type of array item.</typeparam>
		/// <param name="array">Array to check.</param>
		/// <remarks>This method performs fast check instead of creating enumerator</remarks>
		[Pure]
		public static bool Any<T>([NotNull] this T[] array)
		{
			Code.NotNull(array, nameof(array));
			return array.Length != 0;
		}
	}
}
