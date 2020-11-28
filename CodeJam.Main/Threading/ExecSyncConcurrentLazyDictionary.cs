using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.Threading
{
	/// <summary>
	/// Dictionary with lazy values initialization.
	/// </summary>
	/// <remarks>
	/// Thread safe.
	/// </remarks>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	[PublicAPI]
	public class ExecSyncConcurrentLazyDictionary<TKey, TValue> : ILazyDictionary<TKey, TValue>
		where TKey: notnull
	{
		[JetBrains.Annotations.NotNull]
		private readonly Func<TKey, TValue> _valueFactory;
		[JetBrains.Annotations.NotNull]
		private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _map;

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="comparer">Key comparer.</param>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to new.</param>
		public ExecSyncConcurrentLazyDictionary(
			[JetBrains.Annotations.NotNull] Func<TKey, TValue> valueFactory,
			[JetBrains.Annotations.NotNull] IEnumerable<KeyValuePair<TKey, TValue>> collection,
			[JetBrains.Annotations.NotNull] IEqualityComparer<TKey> comparer)
		{
			Code.NotNull(valueFactory, nameof(valueFactory));

			_valueFactory = valueFactory;
			_map = new ConcurrentDictionary<TKey, Lazy<TValue>>(
				collection
					.Select(v => new KeyValuePair<TKey, Lazy<TValue>>(
						v.Key,
						new Lazy<TValue>(() => _valueFactory(v.Key), LazyThreadSafetyMode.ExecutionAndPublication))),
				comparer);
		}

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="comparer">Key comparer.</param>
		public ExecSyncConcurrentLazyDictionary(
			Func<TKey, TValue> valueFactory,
			IEqualityComparer<TKey>? comparer)
		{
			Code.NotNull(valueFactory, nameof(valueFactory));

			_valueFactory = valueFactory;
			_map = new ConcurrentDictionary<TKey, Lazy<TValue>>(comparer ?? EqualityComparer<TKey>.Default);
		}

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="valueFactory">Function to create value on demand.</param>
		public ExecSyncConcurrentLazyDictionary([JetBrains.Annotations.NotNull] Func<TKey, TValue> valueFactory)
			: this(valueFactory, EqualityComparer<TKey>.Default)
		{ }

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="valueFactory">Function to create value on demand.</param>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to new.</param>
		public ExecSyncConcurrentLazyDictionary(
			[JetBrains.Annotations.NotNull] Func<TKey, TValue> valueFactory,
			[JetBrains.Annotations.NotNull] IEnumerable<KeyValuePair<TKey, TValue>> collection)
			: this(valueFactory, collection, EqualityComparer<TKey>.Default)
		{ }

		/// <summary>
		/// Clears all created values
		/// </summary>
		public void Clear() => _map.Clear();

		#region Implementation of IEnumerable
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
			_map
				.Select(v => new KeyValuePair<TKey, TValue>(v.Key, v.Value.Value))
				.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
		#endregion

		#region Implementation of IReadOnlyCollection<out KeyValuePair<TKey,TValue>>
		int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => _map.Count;
		#endregion

		#region Implementation of IReadOnlyDictionary<TKey,TValue>
		/// <summary>Determines whether the read-only dictionary contains an element that has the specified key.</summary>
		/// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
		/// <param name="key">The key to locate.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="key" /> is null.</exception>
		public bool ContainsKey([JetBrains.Annotations.NotNull] TKey key) => _map.ContainsKey(key);

		/// <summary>Gets the value that is associated with the specified key.</summary>
		/// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" /> interface contains an element that has the specified key; otherwise, false.</returns>
		/// <param name="key">The key to locate.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="key" /> is null.</exception>
		public bool TryGetValue(TKey key,
#if NET30_OR_GREATER
			[MaybeNullWhen(false)]
#endif
			out TValue value)
		{
			var res = _map.TryGetValue(key, out var lv);
			if (res)
			{
				value = lv!.Value;
				return true;
			}
#if NET30_OR_GREATER
			value = default;
#else
			value = default!; // No NRT markup in older frameworks
#endif
			return false;
		}

		/// <summary>Gets the element that has the specified key in the read-only dictionary.</summary>
		/// <returns>The element that has the specified key in the read-only dictionary.</returns>
		/// <param name="key">The key to locate.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="key" /> is null.</exception>
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found. </exception>
		public TValue this[[JetBrains.Annotations.NotNull] TKey key] =>
			_map
				.GetOrAdd(
					key,
					new Lazy<TValue>(() => _valueFactory(key), LazyThreadSafetyMode.ExecutionAndPublication))
				.Value;

		/// <summary>Gets an enumerable collection that contains the keys in the read-only dictionary. </summary>
		/// <returns>An enumerable collection that contains the keys in the read-only dictionary.</returns>
		public IEnumerable<TKey> Keys => _map.Keys;

		/// <summary>Gets an enumerable collection that contains the values in the read-only dictionary.</summary>
		/// <returns>An enumerable collection that contains the values in the read-only dictionary.</returns>
		public IEnumerable<TValue> Values => _map.Values.Select(lv => lv.Value);
		#endregion
	}
}