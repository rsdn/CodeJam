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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			IEqualityComparer<TKey> comparer,
			bool threadSafe)
			where TKey : notnull =>
				threadSafe
					? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, comparer)
					: new LazyDictionary<TKey, TValue>(valueFactory, comparer);

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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			IEnumerable<KeyValuePair<TKey, TValue>> collection,
			IEqualityComparer<TKey> comparer,
			bool threadSafe)
			where TKey : notnull =>
				threadSafe
					? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection, comparer)
					: new LazyDictionary<TKey, TValue>(valueFactory, collection, comparer);

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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			bool threadSafe)
			where TKey : notnull =>
				threadSafe
					? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory)
					: new LazyDictionary<TKey, TValue>(valueFactory);

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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			IEnumerable<KeyValuePair<TKey, TValue>> collection,
			bool threadSafe)
			where TKey : notnull =>
				threadSafe
					? new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection)
					: new LazyDictionary<TKey, TValue>(valueFactory, collection);

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode. </param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			LazyThreadSafetyMode threadSafety)
			where TKey : notnull
		{
			return
				threadSafety switch
				{
					LazyThreadSafetyMode.None => new LazyDictionary<TKey, TValue>(valueFactory),
					LazyThreadSafetyMode.PublicationOnly => new ConcurrentLazyDictionary<TKey, TValue>(valueFactory),
					LazyThreadSafetyMode.ExecutionAndPublication => new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory),
					_ => throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null)
					};
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			IEnumerable<KeyValuePair<TKey, TValue>> collection,
			LazyThreadSafetyMode threadSafety)
			where TKey : notnull =>
				threadSafety switch
				{
					LazyThreadSafetyMode.None =>
						new LazyDictionary<TKey, TValue>(valueFactory, collection),
					LazyThreadSafetyMode.PublicationOnly => new ConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection),
					LazyThreadSafetyMode.ExecutionAndPublication =>
						new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection),
					_ => throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null)
					};

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="comparer">Key comparer.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode. </param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			IEqualityComparer<TKey>? comparer,
			LazyThreadSafetyMode threadSafety)
			where TKey : notnull =>
				threadSafety switch
				{
					LazyThreadSafetyMode.None =>
						new LazyDictionary<TKey, TValue>(valueFactory, comparer),
					LazyThreadSafetyMode.PublicationOnly => new ConcurrentLazyDictionary<TKey, TValue>(valueFactory, comparer),
					LazyThreadSafetyMode.ExecutionAndPublication =>
						new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, comparer),
					_ => throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null)
					};

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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
			Func<TKey, TValue> valueFactory,
			IEnumerable<KeyValuePair<TKey, TValue>> collection,
			IEqualityComparer<TKey> comparer,
			LazyThreadSafetyMode threadSafety)
			where TKey : notnull =>
				threadSafety switch
				{
					LazyThreadSafetyMode.None => new LazyDictionary<TKey, TValue>(valueFactory, collection, comparer),
					LazyThreadSafetyMode.PublicationOnly =>
						new ConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection, comparer),
					LazyThreadSafetyMode.ExecutionAndPublication =>
						new ExecSyncConcurrentLazyDictionary<TKey, TValue>(valueFactory, collection, comparer),
					_ => throw new ArgumentOutOfRangeException(nameof(threadSafety), threadSafety, null)
					};
	}
}