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
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <returns>Generated sequence.</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<T> Create<T>(T start, [NotNull] Func<T, T> next)
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
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <param name="resultSelector">A transform function to apply to each element.</param>
		/// <returns>Generated sequence.</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<TResult> Create<T, TResult>(
			T start,
			[NotNull] Func<T, T> next,
			[NotNull] Func<T, TResult> resultSelector)
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
		/// <param name="start">Start value.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="next">Next element factory.</param>
		/// <returns>Generated sequence.</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<T> Create<T>(T start, [NotNull] Func<T, bool> predicate, [NotNull] Func<T, T> next)
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(predicate, nameof (predicate));

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
		/// <param name="start">Start value.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="next">Next element factory.</param>
		/// <param name="resultSelector">A transform function to apply to each element.</param>
		/// <returns>Generated sequence.</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<TResult> Create<T, TResult>(
			T start,
			[NotNull] Func<T, bool> predicate,
			[NotNull] Func<T, T> next,
			[NotNull] Func<T, TResult> resultSelector)
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
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <returns>Generated sequence.</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<T> CreateWhileNotNull<T>(T start, [NotNull] Func<T, T> next)
			where T: class
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
		/// <param name="start">Start value.</param>
		/// <param name="next">Next element factory.</param>
		/// <param name="resultSelector">A transform function to apply to each element.</param>
		/// <returns>Generated sequence.</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<TResult> CreateWhileNotNull<T, TResult>(
			T start,
			[NotNull] Func<T, T> next,
			[NotNull] Func<T, TResult> resultSelector)
			where T : class
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
		[Pure]
		[NotNull]
		public static IEnumerable<T> CreateSingle<T>(T element)
		{
			yield return element;
		}

		/// <summary>
		/// Creates a single element sequence.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="elementFactory">Element factory.</param>
		/// <returns>Single element sequence</returns>
		[Pure]
		[NotNull]
		public static IEnumerable<T> CreateSingle<T>([NotNull] Func<T> elementFactory)
		{
			yield return elementFactory();
		}
	}
}