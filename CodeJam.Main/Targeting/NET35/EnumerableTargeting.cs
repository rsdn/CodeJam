// Once Theraot v3 is be released, this file can be removed.

#if NET35
// BASEDON: https://github.com/dotnet/corefx/blob/bffef76f6af208e2042a2f27bc081ee908bb390b/src/System.Linq/src/System/Linq/Zip.cs

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Targeting
{
	/// <summary>
	/// Targeting methods for <see cref="Enumerable"/>
	/// </summary>
	public static class EnumerableTargeting
	{
		/// <summary>
		/// Applies a specified function to the corresponding elements of two sequences, producing a sequence of the results.
		/// </summary>
		/// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
		/// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
		/// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
		/// <param name="first">The first input sequence.</param>
		/// <param name="second">The second input sequence.</param>
		/// <param name="resultSelector">
		/// A function that specifies how to combine the corresponding elements of the two sequences.
		/// </param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains elements of the two input sequences, combined by
		/// <paramref name="resultSelector"/>.
		/// </returns>
		[NotNull]
		public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
			this IEnumerable<TFirst> first, IEnumerable<TSecond> second,
			Func<TFirst, TSecond, TResult> resultSelector)
		{
			Code.NotNull(first, nameof(first));
			Code.NotNull(second, nameof(second));
			Code.NotNull(resultSelector, nameof(resultSelector));

			return ZipIterator(first, second, resultSelector);
		}

		[NotNull]
		private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(
			[NotNull] IEnumerable<TFirst> first,
			[NotNull] IEnumerable<TSecond> second,
			[NotNull] Func<TFirst, TSecond, TResult> resultSelector)
		{
			using (var e1 = first.GetEnumerator())
			using (var e2 = second.GetEnumerator())
				while (e1.MoveNext() && e2.MoveNext())
					yield return resultSelector(e1.Current, e2.Current);
		}
	}
}
#endif