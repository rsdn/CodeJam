using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	public static partial class EnumerableExtensions
	{
		/// <summary>
		/// Associates an index to each element of the source sequence.
		/// </summary>
		/// <typeparam name="T">Type of elements in source.</typeparam>
		/// <param name="source">The input sequence.</param>
		/// <returns>
		/// A sequence of elements paired with their index in the sequence.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		public static IEnumerable<IndexedItem<T>> WithIndex<T>(this IEnumerable<T> source)
			where T : notnull
		{
			Code.NotNull(source, nameof(source));
			return IndexImpl(source);
		}

		[Pure, System.Diagnostics.Contracts.Pure, LinqTunnel]
		private static IEnumerable<IndexedItem<T>> IndexImpl<T>(IEnumerable<T> source)
			where T : notnull
		{
			using var enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				var index = 0;
				var isFirst = true;
				var isLast = false;

				while (!isLast)
				{
					// ReSharper disable once RedundantSuppressNullableWarningExpression
					var item = enumerator.Current!;
					isLast = !enumerator.MoveNext();

					yield return new IndexedItem<T>(item, index++, isFirst, isLast);
					isFirst = false;
				}
			}
		}

		/// <summary>Combines item with previous value from the sequence.</summary>
		/// <typeparam name="T">The type of the item.</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="source">The input sequence.</param>
		/// <param name="prevNextSelector">The previous next selector.</param>
		/// <returns>Sequence of items combined with previous values from the sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<TResult> CombineWithPrevious<T, TResult>(
			this IEnumerable<T> source,
			Func<T?, T?, TResult> prevNextSelector)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(prevNextSelector, nameof(prevNextSelector));

			return CombineWithPreviousImpl(source, prevNextSelector);
		}

		[Pure, System.Diagnostics.Contracts.Pure]
		private static IEnumerable<TResult> CombineWithPreviousImpl<T, TResult>(
			this IEnumerable<T> source,
			Func<T?, T?, TResult> prevNextSelector)
		{
			var previous = default(T);
			var hasPrevious = false;

			foreach (var item in source)
			{
				if (hasPrevious)
					yield return prevNextSelector(previous, item);
				previous = item;
				hasPrevious = true;
			}
		}

		/// <summary>Combines item with previous value from the sequence.</summary>
		/// <typeparam name="T">The type of the item.</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="source">The input sequence.</param>
		/// <param name="seed">The seed value to be used as a previous for the first item in the sequence.</param>
		/// <param name="prevNextSelector">The previous next selector.</param>
		/// <returns>Sequence of items combined with previous values from the sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<TResult> CombineWithPrevious<T, TResult>(
			this IEnumerable<T> source,
			T seed,
			Func<T, T, TResult> prevNextSelector)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(prevNextSelector, nameof(prevNextSelector));

			return CombineWithPreviousImpl(source, seed, prevNextSelector);
		}

		[Pure, System.Diagnostics.Contracts.Pure]
		private static IEnumerable<TResult> CombineWithPreviousImpl<T, TResult>(
			this IEnumerable<T> source,
			T seed,
			Func<T, T, TResult> prevNextSelector)
		{
			var previous = seed;
			foreach (var item in source)
			{
				yield return prevNextSelector(previous, item);
				previous = item;
			}
		}

		/// <summary>Combines item with next value from the sequence.</summary>
		/// <typeparam name="T">The type of the item.</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="source">The input sequence.</param>
		/// <param name="combineLast">The value to be used as a next for the last item in the sequence.</param>
		/// <param name="prevNextSelector">The previous next selector.</param>
		/// <returns>Sequence of items combined with previous values from the sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<TResult> CombineWithNext<T, TResult>(
			this IEnumerable<T> source,
			T combineLast,
			Func<T?, T?, TResult> prevNextSelector)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(prevNextSelector, nameof(prevNextSelector));

			return CombineWithNextImpl(source, combineLast, prevNextSelector);
		}

		[Pure, System.Diagnostics.Contracts.Pure]
		private static IEnumerable<TResult> CombineWithNextImpl<T, TResult>(
			this IEnumerable<T> source, T combineLast,
			Func<T?, T?, TResult> prevNextSelector)
		{
			var previous = default(T);
			var hasPrevious = false;

			foreach (var item in source)
			{
				if (hasPrevious)
					yield return prevNextSelector(previous, item);
				previous = item;
				hasPrevious = true;
			}
			if (hasPrevious)
				yield return prevNextSelector(previous, combineLast);
		}
	}
}