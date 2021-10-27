using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
using TaskEx = System.Threading.Tasks.Task;

#else
using TaskEx = System.Threading.Tasks.TaskEx;

#endif

namespace CodeJam.Threading
{
	/// <summary>
	/// Helper methods for <see cref="Task"/> and <see cref="Task{TResult}"/>.
	/// </summary>
	public static partial class TaskHelper
	{
		private const int _maxProcessorCountRefreshTicks = 30000;
		private static volatile int _processorCount;
		private static volatile int _lastProcessorCountRefreshTicks;

		// BASEDON PlatformHelper.ProcessorCount
		private static int ProcessorCount
		{
			get
			{
				var tickCount = Environment.TickCount;
				if (_processorCount == 0 || tickCount - _lastProcessorCountRefreshTicks >= _maxProcessorCountRefreshTicks)
				{
					_processorCount = Environment.ProcessorCount;
					_lastProcessorCountRefreshTicks = tickCount;
				}
				return _processorCount;
			}
		}

#if NETSTANDARD20_OR_GREATER || NET45_OR_GREATER || TARGETS_NETCOREAPP
		/// <summary>
		/// Gets the maximum degree of parallelism for the scheduler.
		/// Matches to the <see cref="Parallel.ForEach{TSource}(IEnumerable{TSource}, Action{TSource})"/> behavior.
		/// Limits <paramref name="value"/> by <see cref="TaskScheduler.MaximumConcurrencyLevel"/> value (if non-zero positiver value).
		/// Otherwise, uses <see cref="Environment.ProcessorCount"/> as fallback value.
		/// </summary>
#else
		/// <summary>
		/// Gets the maximum degree of parallelism for the scheduler.
		/// Limits <paramref name="value"/> by <see cref="TaskScheduler.MaximumConcurrencyLevel"/> value (if non-zero positiver value).
		/// Otherwise, uses <see cref="Environment.ProcessorCount"/> as fallback value.
		/// </summary>
#endif
		public static int GetMaxDegreeOfParallelism(this TaskScheduler scheduler, int value)
		{
			Code.NotNull(scheduler, nameof(scheduler));

			var concurrencyLimit = scheduler.MaximumConcurrencyLevel;

			if (concurrencyLimit > 0 && concurrencyLimit != int.MaxValue)
				return value <= 0 ? concurrencyLimit : Math.Min(concurrencyLimit, value);

			return value <= 0 ? ProcessorCount : value;
		}

		/// <summary>
		/// Runs actions over source items concurrently and asynchronously.
		/// </summary>
		/// <typeparam name="T">Type of items to process</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="maxDegreeOfParallelism">
		/// The maximum degree of parallelism. If zero or negative, default scheduler value is used.
		/// See <see cref="GetMaxDegreeOfParallelism"/> documentation for more details.
		/// </param>
		/// <param name="cancellation">The cancellation.</param>
		// BASEDON https://stackoverflow.com/a/25877042
		public static Task ForEachAsync<T>(
			this IEnumerable<T> source,
			Func<T, CancellationToken, Task> callback,
			int maxDegreeOfParallelism = 0,
			CancellationToken cancellation = default) =>
			ForEachAsync(source, (t, _, ct) => callback(t, ct), maxDegreeOfParallelism, cancellation);

		/// <summary>
		/// Runs actions over source items concurrently and asynchronously.
		/// </summary>
		/// <typeparam name="T">Type of items to process</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="maxDegreeOfParallelism">
		/// The maximum degree of parallelism. If zero or negative, default scheduler value is used.
		/// See <see cref="GetMaxDegreeOfParallelism"/> documentation for more details.
		/// </param>
		/// <param name="cancellation">The cancellation.</param>
		// BASEDON https://stackoverflow.com/a/25877042
		public static async Task ForEachAsync<T>(
			this IEnumerable<T> source,
			Func<T, long, CancellationToken, Task> callback,
			int maxDegreeOfParallelism = 0,
			CancellationToken cancellation = default)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(callback, nameof(callback));

			maxDegreeOfParallelism = TaskScheduler.Current.GetMaxDegreeOfParallelism(maxDegreeOfParallelism);

			using (var customCancellation = CreateCancellation(cancellation))
			using (customCancellation.CancellationScope())
			{
				var combinedToken = customCancellation.Token;

				await Partitioner.Create(source)
					.GetOrderablePartitions(maxDegreeOfParallelism)
					.Select(
						kvPartition => TaskEx.Run(
							async () =>
							{
								try
								{
									using (kvPartition)
									{
										while (kvPartition.MoveNext() && !combinedToken.IsCancellationRequested)
										{
											cancellation.ThrowIfCancellationRequested();
											var current = kvPartition.Current;
											await callback(current.Value, current.Key, combinedToken).ConfigureAwait(false);
										}
									}
								}
								catch (Exception)
								{
									// ReSharper disable once AccessToDisposedClosure
									customCancellation.Cancel();
									throw;
								}
							},
							combinedToken))
					.WhenAll()
					.WithAggregateException()
					.ConfigureAwait(false);

				customCancellation.Token.ThrowIfCancellationRequested();
			}
		}

		/// <summary>
		/// Runs actions over source items concurrently and asynchronously.
		/// The result array is ordered correspondingly to tasks order.
		/// </summary>
		/// <typeparam name="T">Type of items to process</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="maxDegreeOfParallelism">The maximum degree of parallelism. If zero or negative, default scheduler value is used.
		/// See <see cref="GetMaxDegreeOfParallelism" /> documentation for more details.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task<TResult[]> ForEachAsync<T, TResult>(
			this IEnumerable<T> source,
			Func<T, CancellationToken, Task<TResult>> callback,
			int maxDegreeOfParallelism = 0,
			CancellationToken cancellation = default) =>
			ForEachAsync(source, (t, _, ct) => callback(t, ct), maxDegreeOfParallelism, cancellation);

		/// <summary>
		/// Runs actions over source items concurrently and asynchronously.
		/// The result array is ordered correspondingly to tasks order.
		/// </summary>
		/// <typeparam name="T">Type of items to process</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="maxDegreeOfParallelism">The maximum degree of parallelism. If zero or negative, default scheduler value is used.
		/// See <see cref="GetMaxDegreeOfParallelism" /> documentation for more details.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task<TResult[]> ForEachAsync<T, TResult>(
			this IEnumerable<T> source,
			Func<T, long, CancellationToken, Task<TResult>> callback,
			int maxDegreeOfParallelism = 0,
			CancellationToken cancellation = default)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(callback, nameof(callback));

			return Collections.Backported.EnumerableExtensions.TryGetNonEnumeratedCount(source, out int count)
				? ForEachEnumerableWithCountAsync(source, count, callback, maxDegreeOfParallelism, cancellation)
				: ForEachEnumerableAsync(source, callback, maxDegreeOfParallelism, cancellation);
		}

		private static async Task<TResult[]> ForEachEnumerableAsync<T, TResult>(
			IEnumerable<T> source,
			Func<T, long, CancellationToken, Task<TResult>> callback,
			int maxDegreeOfParallelism,
			CancellationToken cancellation)
		{
			var dict = new ConcurrentDictionary<long, TResult>();

			await source
				.ForEachAsync(
					async (t, i, ct) =>
					{
						var x = await callback(t, i, ct).ConfigureAwait(false);
						dict.TryAdd(i, x);
					},
					maxDegreeOfParallelism,
					cancellation)
				.ConfigureAwait(false);

			return dict.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray();
		}

		private static async Task<TResult[]> ForEachEnumerableWithCountAsync<T, TResult>(
			IEnumerable<T> source,
			int count,
			Func<T, long, CancellationToken, Task<TResult>> callback,
			int maxDegreeOfParallelism,
			CancellationToken cancellation)
		{
			var array = new TResult[count];

			await source
				.ForEachAsync(
					async (t, i, ct) =>
					{
						var x = await callback(t, i, ct).ConfigureAwait(false);
						array[i] = x;
					},
					maxDegreeOfParallelism,
					cancellation)
				.ConfigureAwait(false);

			Thread.MemoryBarrier();

			return array;
		}

		/// <summary>
		/// Simplifies the <see cref="AggregateException"/> handling on await.
		/// By default awaiter rethrows only first exception of the <see cref="Exception.InnerException"/>.
		/// This helper rethrows original <see cref="AggregateException"/> as is.
		/// </summary>
		/// <param name="source">The task that may throw <see cref="AggregateException"/>.</param>
		// BASEDON https://stackoverflow.com/a/18315625
		public static async Task WithAggregateException(this Task source)
		{
			Code.NotNull(source, nameof(source));
			try
			{
				await source.ConfigureAwait(false);
			}
			catch
			{
				if (source.Exception != null)
					ExceptionDispatchInfo.Capture(source.Exception).Throw();
				throw;
			}
		}

		/// <summary>
		/// Simplifies the <see cref="AggregateException"/> handling on await.
		/// By default awaiter rethrows only first exception of the <see cref="Exception.InnerException"/>.
		/// This helper rethrows original <see cref="AggregateException"/> as is.
		/// </summary>
		/// <param name="source">The task that may throw <see cref="AggregateException"/>.</param>
		// BASEDON https://stackoverflow.com/a/18315625
		public static async Task<T> WithAggregateException<T>(this Task<T> source)
		{
			Code.NotNull(source, nameof(source));
			try
			{
				return await source.ConfigureAwait(false);
			}
			catch
			{
				if (source.Exception != null)
					ExceptionDispatchInfo.Capture(source.Exception).Throw();
				throw;
			}
		}
	}
}
