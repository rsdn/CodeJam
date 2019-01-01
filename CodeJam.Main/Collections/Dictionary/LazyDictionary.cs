﻿using System;
using System.Collections.Generic;
using System.Threading;

using CodeJam.Threading;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Provides static methods for <see cref="ILazyDictionary{TKey,TValue}"/>.
	/// </summary>
	[PublicAPI]
	public static class LazyDictionary
	{
		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="comparer">Key comparer.</param>
		/// <param name="threadSafe">
		/// If true, creates a thread safe implementation.
		/// <paramref name="valueFactory"/> guaranteed to call only once.
		/// </param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
				[NotNull] Func<TKey, TValue> valueFactory,
				[NotNull] IEqualityComparer<TKey> comparer,
				bool threadSafe) =>
			threadSafe
				? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, comparer)
				: (ILazyDictionary<TKey, TValue>)new LazyDictionary<TKey, TValue>(valueFactory, comparer);

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="comparer">Key comparer.</param>
		/// <param name="threadSafe">
		/// If true, creates a thread safe implementation.
		/// <paramref name="valueFactory"/> guaranteed to call only once.
		/// </param>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to new.</param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			[NotNull] Func<TKey, TValue> valueFactory,
			[NotNull] IEnumerable<KeyValuePair<TKey, TValue>> collection,
			[NotNull] IEqualityComparer<TKey> comparer,
			bool threadSafe) =>
				threadSafe
					? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection, comparer)
					: (ILazyDictionary<TKey, TValue>)new LazyDictionary<TKey, TValue>(valueFactory, collection, comparer);

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="threadSafe">
		/// If true, creates a thread safe implementation.
		/// <paramref name="valueFactory"/> guaranteed to call only once.
		/// </param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
				[NotNull] Func<TKey, TValue> valueFactory,
				bool threadSafe) =>
			threadSafe
				? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory)
				: (ILazyDictionary<TKey, TValue>)new LazyDictionary<TKey, TValue>(valueFactory);

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="threadSafe">
		/// If true, creates a thread safe implementation.
		/// <paramref name="valueFactory"/> guaranteed to call only once.
		/// </param>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to new.</param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			[NotNull] Func<TKey, TValue> valueFactory,
			[NotNull] IEnumerable<KeyValuePair<TKey, TValue>> collection,
			bool threadSafe) =>
				threadSafe
					? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection)
					: (ILazyDictionary<TKey, TValue>)new LazyDictionary<TKey, TValue>(valueFactory, collection);

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode. </param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			[NotNull] Func<TKey, TValue> valueFactory,
			LazyThreadSafetyMode threadSafety)
		{
			switch (threadSafety)
			{
				case LazyThreadSafetyMode.None:
					return new LazyDictionary<TKey, TValue>(valueFactory);
				case LazyThreadSafetyMode.PublicationOnly:
					return new ConcurrentLazyDictionary<TKey, TValue>(valueFactory);
				case LazyThreadSafetyMode.ExecutionAndPublication:
					return new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory);
				default:
					throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null);
			}
		}

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode. </param>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to new.</param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			[NotNull] Func<TKey, TValue> valueFactory,
			[NotNull] IEnumerable<KeyValuePair<TKey, TValue>> collection,
			LazyThreadSafetyMode threadSafety)
		{
			switch (threadSafety)
			{
				case LazyThreadSafetyMode.None:
					return new LazyDictionary<TKey, TValue>(valueFactory, collection);
				case LazyThreadSafetyMode.PublicationOnly:
					return new ConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection);
				case LazyThreadSafetyMode.ExecutionAndPublication:
					return new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection);
				default:
					throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null);
			}
		}

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="comparer">Key comparer.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode. </param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			[NotNull] Func<TKey, TValue> valueFactory,
			[NotNull] IEqualityComparer<TKey> comparer,
			LazyThreadSafetyMode threadSafety)
		{
			switch (threadSafety)
			{
				case LazyThreadSafetyMode.None:
					return new LazyDictionary<TKey, TValue>(valueFactory, comparer);
				case LazyThreadSafetyMode.PublicationOnly:
					return new ConcurrentLazyDictionary<TKey, TValue>(valueFactory, comparer);
				case LazyThreadSafetyMode.ExecutionAndPublication:
					return new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, comparer);
				default:
					throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null);
			}
		}

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="comparer">Key comparer.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode. </param>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to new.</param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			[NotNull] Func<TKey, TValue> valueFactory,
			[NotNull] IEnumerable<KeyValuePair<TKey, TValue>> collection,
			[NotNull] IEqualityComparer<TKey> comparer,
			LazyThreadSafetyMode threadSafety)
		{
			switch (threadSafety)
			{
				case LazyThreadSafetyMode.None:
					return new LazyDictionary<TKey, TValue>(valueFactory, collection, comparer);
				case LazyThreadSafetyMode.PublicationOnly:
					return new ConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection, comparer);
				case LazyThreadSafetyMode.ExecutionAndPublication:
					return new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection, comparer);
				default:
					throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null);
			}
		}
	}
}