#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES
// Thread is missing if targeting to these frameworks
#else
using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Threading
{
	/// <summary>
	/// Parallel extensions.
	/// </summary>
	[PublicAPI]
	public static class ParallelExtensions
	{

		/// <summary>
		/// Implements Provider-Consumer pattern.
		/// </summary>
		/// <typeparam name="TSource">Type of source value</typeparam>
		/// <typeparam name="TTarget">Type of target value</typeparam>
		/// <param name="source">Incoming data.</param>
		/// <param name="providerCount">Number of provider threads.</param>
		/// <param name="providerFunc">Provider function</param>
		/// <param name="consumerCount">Number of consumer threads.</param>
		/// <param name="consumerAction">Consumer action.</param>
		/// <param name="processName">Process name pattern.</param>
		public static void RunInParallel<TSource,TTarget>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			int providerCount,
			[NotNull, InstantHandle] Func<TSource,TTarget> providerFunc,
			int consumerCount,
			[NotNull, InstantHandle] Action<TTarget> consumerAction,
			string processName = "ParallelProcess")
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(providerFunc, nameof(providerFunc));
			Code.NotNull(consumerAction, nameof(consumerAction));

			using var providerQueue = new ParallelQueue(providerCount, processName + "_provider_");
			using var consumerQueue = new ParallelQueue(consumerCount, processName + "_consumer_");
			foreach (var item in source)
			{
				var pItem = item;

				providerQueue.EnqueueItem(() =>
				{
					var data = providerFunc(pItem);

					// ReSharper disable once AccessToDisposedClosure
					consumerQueue.EnqueueItem(() => consumerAction(data));
				});
			}

			providerQueue.WaitAll();
			consumerQueue.WaitAll();
		}

		/// <summary>
		/// Implements Provider-Consumer pattern.
		/// </summary>
		/// <typeparam name="TSource">Type of source value</typeparam>
		/// <typeparam name="TTarget">Type of target value</typeparam>
		/// <param name="source">Incoming data.</param>
		/// <param name="providerFunc">Provider function</param>
		/// <param name="consumerCount">Number of consumer threads.</param>
		/// <param name="consumerAction">Consumer action.</param>
		/// <param name="processName">Process name pattern.</param>
		public static void RunInParallel<TSource,TTarget>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource,TTarget> providerFunc,
			int consumerCount,
			[NotNull, InstantHandle] Action<TTarget> consumerAction,
			string processName = "ParallelProcess")
			=> RunInParallel(source, Environment.ProcessorCount, providerFunc, consumerCount, consumerAction, processName);

		/// <summary>
		/// Implements Provider-Consumer pattern.
		/// </summary>
		/// <typeparam name="TSource">Type of source value</typeparam>
		/// <typeparam name="TTarget">Type of target value</typeparam>
		/// <param name="source">Incoming data.</param>
		/// <param name="providerCount">Number of provider threads.</param>
		/// <param name="providerFunc">Provider function</param>
		/// <param name="consumerAction">Consumer action.</param>
		/// <param name="processName">Process name pattern.</param>
		public static void RunInParallel<TSource,TTarget>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			int providerCount,
			[NotNull, InstantHandle] Func<TSource,TTarget> providerFunc,
			[NotNull, InstantHandle] Action<TTarget> consumerAction,
			string processName = "ParallelProcess")
			=> RunInParallel(source, providerCount, providerFunc, Environment.ProcessorCount, consumerAction, processName);

		/// <summary>
		/// Implements Provider-Consumer pattern.
		/// </summary>
		/// <typeparam name="TSource">Type of source value</typeparam>
		/// <typeparam name="TTarget">Type of target value</typeparam>
		/// <param name="source">Incoming data.</param>
		/// <param name="providerFunc">Provider function</param>
		/// <param name="consumerAction">Consumer action.</param>
		/// <param name="processName">Process name pattern.</param>
		public static void RunInParallel<TSource,TTarget>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource,TTarget> providerFunc,
			[NotNull, InstantHandle] Action<TTarget> consumerAction,
			string processName = "ParallelProcess")
			=> RunInParallel(source, Environment.ProcessorCount / 2, providerFunc, Environment.ProcessorCount / 2, consumerAction, processName);

		/// <summary>
		/// Runs in parallel provided source of actions.
		/// </summary>
		/// <param name="source">Actions to run.</param>
		/// <param name="parallelCount">number of threads to use.</param>
		/// <param name="processName">Process name pattern.</param>
		public static void RunInParallel(
			[NotNull, ItemNotNull, InstantHandle] this IEnumerable<Action> source,
			int parallelCount,
			string processName = "ParallelProcess")
		{
			using var queue = new ParallelQueue(parallelCount, processName + '_');
			foreach (var action in source)
			{
				var data = action;
				queue.EnqueueItem(data);
			}

			queue.WaitAll();
		}

		/// <summary>
		/// Runs in parallel provided source of actions.
		/// </summary>
		/// <param name="source">Actions to run.</param>
		/// <param name="processName">Process name pattern.</param>
		public static void RunInParallel([NotNull, InstantHandle] this IEnumerable<Action> source, string processName = "ParallelProcess")
			=> RunInParallel(source, Environment.ProcessorCount, processName);

		/// <summary>
		/// Runs in parallel actions for provided data source.
		/// </summary>
		/// <typeparam name="T">Type of element in source</typeparam>
		/// <param name="source">Source to run.</param>
		/// <param name="parallelCount">number of threads to use.</param>
		/// <param name="action">Action to run.</param>
		/// <param name="processName">Process name.</param>
		public static void RunInParallel<T>(
			[NotNull, InstantHandle] this IEnumerable<T> source,
			int parallelCount,
			[NotNull, InstantHandle] Action<T> action,
			string processName = "ParallelProcess")
		{
			using var queue = new ParallelQueue(parallelCount, processName + '_');
			foreach (var item in source)
			{
				var data = item;
				var run = action;
				queue.EnqueueItem(() => run(data));
			}

			queue.WaitAll();
		}

		/// <summary>
		/// Runs in parallel actions for provided data source.
		/// </summary>
		/// <typeparam name="T">Type of element in source</typeparam>
		/// <param name="source">Source to run.</param>
		/// <param name="action">Action to run.</param>
		/// <param name="processName">Process name.</param>
		public static void RunInParallel<T>(
			[NotNull, InstantHandle] this IEnumerable<T> source,
			[NotNull, InstantHandle] Action<T> action,
			string processName = "ParallelProcess")
			=> RunInParallel(source, Environment.ProcessorCount, action, processName);
	}
}
#endif