using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Contains methods for sequence creation.
	/// </summary>
	[PublicAPI]
	public static class Sequence
	{
		/// <summary>
		/// Creates a sequence from start value and next element factory.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <typeparam name="TNext">The type of <paramref name="next"/> result.</typeparam>
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <returns>Generated sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<T> Create<T, TNext>(T start, Func<T, TNext> next) where TNext : T
		{
			Code.NotNull(next, nameof(next));

			var cur = start;
			while (true)
			{
				yield return cur;
				cur = next(cur);
			}
			// ReSharper disable once IteratorNeverReturns
		}

		/// <summary>
		/// Creates a sequence from start value and next element factory.
		/// </summary>
		/// <typeparam name="T">The type of source element.</typeparam>
		/// <typeparam name="TResult">The type of result element</typeparam>
		/// <typeparam name="TNext">The type of <paramref name="next"/> result.</typeparam>
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <param name="resultSelector">A transform function to apply to each element.</param>
		/// <returns>Generated sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<TResult> Create<T, TResult, TNext>(
			T start,
			Func<T, TNext> next,
			Func<T, TResult> resultSelector)
			where TNext : T
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(resultSelector, nameof(resultSelector));

			var cur = start;
			while (true)
			{
				yield return resultSelector(cur);
				cur = next(cur);
			}
			// ReSharper disable once IteratorNeverReturns
		}

		/// <summary>
		/// Creates a sequence from start value and next element factory.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <typeparam name="TNext">The type of <paramref name="next"/> result.</typeparam>
		/// <param name="start">Start value.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="next">Next element factory.</param>
		/// <returns>Generated sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<T> Create<T, TNext>(T start, Func<T, bool> predicate, Func<T, TNext> next) where TNext : T
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(predicate, nameof(predicate));

			var cur = start;
			while (predicate(cur))
			{
				yield return cur;
				cur = next(cur);
			}
		}

		/// <summary>
		/// Creates a sequence from start value and next element factory.
		/// </summary>
		/// <typeparam name="T">The type of source element.</typeparam>
		/// <typeparam name="TResult">The type of result element</typeparam>
		/// <typeparam name="TNext">The type of <paramref name="next"/> result.</typeparam>
		/// <param name="start">Start value.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="next">Next element factory.</param>
		/// <param name="resultSelector">A transform function to apply to each element.</param>
		/// <returns>Generated sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<TResult> Create<T, TResult, TNext>(
			T start,
			Func<T, bool> predicate,
			Func<T, TNext> next,
			Func<T, TResult> resultSelector)
			where TNext : T
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(predicate, nameof(predicate));
			Code.NotNull(resultSelector, nameof(resultSelector));

			var cur = start;
			while (predicate(cur))
			{
				yield return resultSelector(cur);
				cur = next(cur);
			}
		}

		/// <summary>
		/// Creates a sequence from start value and next element factory till factory returns null.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <typeparam name="TNext">The type of <paramref name="next"/> result.</typeparam>
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <returns>Generated sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<T> CreateWhileNotNull<T, TNext>(T? start, Func<T, TNext?> next)
			where T : class
			where TNext : T
		{
			Code.NotNull(next, nameof(next));

			var cur = start;
			while (cur != null)
			{
				yield return cur;
				cur = next(cur);
			}
		}

		/// <summary>
		/// Creates a sequence from start value and next element factory till factory returns null.
		/// </summary>
		/// <typeparam name="T">The type of source element.</typeparam>
		/// <typeparam name="TResult">The type of result element</typeparam>
		/// <typeparam name="TNext">The type of <paramref name="next"/> result.</typeparam>
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <param name="resultSelector">A transform function to apply to each element.</param>
		/// <returns>Generated sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<TResult> CreateWhileNotNull<T, TResult, TNext>(
			T? start,
			Func<T, TNext?> next,
			Func<T, TResult> resultSelector)
			where T : class?
			where TNext : T
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(resultSelector, nameof(resultSelector));

			var cur = start;
			while (cur != null)
			{
				yield return resultSelector(cur);
				cur = next(cur);
			}
		}

		/// <summary>
		/// Creates a single element sequence.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="element">Element instance to create sequence from.</param>
		/// <returns>Single element sequence</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<T> CreateSingle<T>(T element)
		{
			return new[] { element };
		}

		/// <summary>
		/// Creates a single element sequence.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="elementFactory">Element factory.</param>
		/// <returns>Single element sequence</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<T> CreateSingle<T>(Func<T> elementFactory)
		{
			Code.NotNull(elementFactory, nameof(elementFactory));

			yield return elementFactory();
		}

		/// <summary>
		/// Creates infinite sequence of random int numbers;
		/// </summary>
		/// <param name="seed">
		/// A number used to calculate a starting value for the pseudo-random number sequence. If a negative number is
		/// specified, the absolute value of the number is used.
		/// </param>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">
		/// The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.
		/// </param>
		/// <returns>Infinite random sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<int> Random(int minValue, int maxValue, int seed)
		{
			var rnd = new Random(seed);
			while (true)
				yield return rnd.Next(minValue, maxValue);
			// ReSharper disable once IteratorNeverReturns
		}

		/// <summary>
		/// Creates infinite sequence of random int numbers;
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">
		/// The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.
		/// </param>
		/// <returns>Infinite random sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<int> Random(int minValue, int maxValue)
		{
			var rnd = new Random();
			while (true)
				yield return rnd.Next(minValue, maxValue);
			// ReSharper disable once IteratorNeverReturns
		}

		/// <summary>
		/// Creates infinite sequence of random int numbers;
		/// </summary>
		/// <param name="maxValue">
		/// The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.
		/// </param>
		/// <returns>Infinite random sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<int> Random(int maxValue = int.MaxValue) => Random(0, maxValue);
	}
}