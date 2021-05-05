using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	public static partial class EnumerableExtensions
	{
		/// <summary>
		/// Applies an accumulator function over a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to aggregate over.</param>
		/// <param name="func">An accumulator function to be invoked on each element.</param>
		/// <param name="defaultValue">Default value returned if the source is empty.</param>
		/// <returns>The final accumulator value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static TSource? AggregateOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TSource, TSource> func,
			TSource defaultValue = default)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(func, nameof(func));

			using var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
				return defaultValue;

			var result = enumerator.Current;

			while (enumerator.MoveNext())
				result = func(result, enumerator.Current);

			return result;
		}

		/// <summary>
		/// Applies an accumulator function over a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to aggregate over.</param>
		/// <param name="seed">The initial accumulator value.</param>
		/// <param name="func">An accumulator function to be invoked on each element.</param>
		/// <param name="defaultValue">Default value returned if the source is empty.</param>
		/// <returns>The final accumulator value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static TAccumulate? AggregateOrDefault<TSource, TAccumulate>(
			[InstantHandle] this IEnumerable<TSource> source,
			TAccumulate seed,
			[InstantHandle] Func<TAccumulate, TSource, TAccumulate> func,
			TAccumulate defaultValue = default)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(func, nameof(func));

			using var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
				return defaultValue;

			var result = func(seed, enumerator.Current);

			while (enumerator.MoveNext())
				result = func(result, enumerator.Current);

			return result;
		}

		/// <summary>
		/// Applies an accumulator function over a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
		/// <typeparam name="TResult">The type of the resulting value.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to aggregate over.</param>
		/// <param name="seed">The initial accumulator value.</param>
		/// <param name="func">An accumulator function to be invoked on each element.</param>
		/// <param name="resultSelector">A function to transform the final accumulator value into the result value.</param>
		/// <param name="defaultValue">Default value returned if the source is empty.</param>
		/// <returns>The final accumulator value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static TResult? AggregateOrDefault<TSource, TAccumulate, TResult>(
			[InstantHandle] this IEnumerable<TSource> source,
			TAccumulate seed,
			[InstantHandle] Func<TAccumulate, TSource, TAccumulate> func,
			[InstantHandle] Func<TAccumulate, TResult> resultSelector,
			TResult defaultValue = default)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(func, nameof(func));
			Code.NotNull(resultSelector, nameof(resultSelector));

			using var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
				return defaultValue;

			var result = func(seed, enumerator.Current);

			while (enumerator.MoveNext())
				result = func(result, enumerator.Current);

			return resultSelector(result);
		}

		/// <summary>
		/// Applies an accumulator function over a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to aggregate over.</param>
		/// <param name="func">An accumulator function to be invoked on each element.</param>
		/// <param name="defaultSelector">A function to select default value if the source is empty.</param>
		/// <returns>The final accumulator value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static TSource AggregateOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TSource, TSource> func,
			[InstantHandle] Func<TSource> defaultSelector)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(func, nameof(func));
			Code.NotNull(defaultSelector, nameof(defaultSelector));

			using var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
				return defaultSelector();

			var result = enumerator.Current;

			while (enumerator.MoveNext())
				result = func(result, enumerator.Current);

			return result;
		}

		/// <summary>
		/// Applies an accumulator function over a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to aggregate over.</param>
		/// <param name="seed">The initial accumulator value.</param>
		/// <param name="func">An accumulator function to be invoked on each element.</param>
		/// <param name="defaultSelector">A function to select default value if the source is empty.</param>
		/// <returns>The final accumulator value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static TAccumulate AggregateOrDefault<TSource, TAccumulate>(
			[InstantHandle] this IEnumerable<TSource> source,
			TAccumulate seed,
			[InstantHandle] Func<TAccumulate, TSource, TAccumulate> func,
			[InstantHandle] Func<TAccumulate> defaultSelector)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(func, nameof(func));
			Code.NotNull(defaultSelector, nameof(defaultSelector));

			using var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
				return defaultSelector();

			var result = func(seed, enumerator.Current);

			while (enumerator.MoveNext())
				result = func(result, enumerator.Current);

			return result;
		}

		/// <summary>
		/// Applies an accumulator function over a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
		/// <typeparam name="TResult">The type of the resulting value.</typeparam>
		/// <param name="source">An <see cref="IEnumerable{T}"/> to aggregate over.</param>
		/// <param name="seed">The initial accumulator value.</param>
		/// <param name="func">An accumulator function to be invoked on each element.</param>
		/// <param name="resultSelector">A function to transform the final accumulator value into the result value.</param>
		/// <param name="defaultSelector">A function to select default value if the source is empty.</param>
		/// <returns>The final accumulator value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static TResult AggregateOrDefault<TSource, TAccumulate, TResult>(
			[InstantHandle] this IEnumerable<TSource> source,
			TAccumulate seed,
			[InstantHandle] Func<TAccumulate, TSource, TAccumulate> func,
			[InstantHandle] Func<TAccumulate, TResult> resultSelector,
			[InstantHandle] Func<TResult> defaultSelector)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(func, nameof(func));
			Code.NotNull(resultSelector, nameof(resultSelector));
			Code.NotNull(defaultSelector, nameof(defaultSelector));

			using var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
				return defaultSelector();

			var result = func(seed, enumerator.Current);

			while (enumerator.MoveNext())
				result = func(result, enumerator.Current);

			return resultSelector(result);
		}
	}
}