using System;
using System.Collections.Generic;

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
		/// <param name="threadSafe">If true, creates a thread safe implementation</param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
				[NotNull] Func<TKey, TValue> valueFactory,
				[NotNull] IEqualityComparer<TKey> comparer,
				bool threadSafe) =>
			threadSafe
				? new ConcurrentLazyDictionary<TKey, TValue>(valueFactory, comparer)
				: (ILazyDictionary<TKey, TValue>)new LazyDictionary<TKey, TValue>(valueFactory, comparer);

		/// <summary>
		/// Creates implementation of <see cref="ILazyDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of key</typeparam>
		/// <typeparam name="TValue">Type of value</typeparam>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="threadSafe">If true, creates a thread safe implementation</param>
		/// <returns><see cref="ILazyDictionary{TKey,TValue}"/> implementation.</returns>
		[NotNull]
		[Pure]
		public static ILazyDictionary<TKey, TValue> Create<TKey, TValue>(
				[NotNull] Func<TKey, TValue> valueFactory,
				bool threadSafe) =>
			threadSafe
				? new ConcurrentLazyDictionary<TKey, TValue>(valueFactory)
				: (ILazyDictionary<TKey, TValue>)new LazyDictionary<TKey, TValue>(valueFactory);
	}
}