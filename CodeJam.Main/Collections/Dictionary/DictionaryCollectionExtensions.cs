using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Extensions for IDictionary&lt;TKey, Container&lt;TValue&gt;&gt;.
	/// </summary>
	[PublicAPI]
	public static class DictionaryCollectionExtensions
	{
		#region List

		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateList<TKey, TValue>(
			[NotNull] this IDictionary<TKey, ICollection<TValue>> dict,
			[NotNull] TKey key,
			TValue value)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => new List<TValue>()).Add(value);
		}

		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateList<TKey, TValue>(
			[NotNull] this IDictionary<TKey, IList<TValue>> dict,
			[NotNull] TKey key,
			TValue value)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => new List<TValue>()).Add(value);
		}

		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateList<TKey, TValue>(
			[NotNull] this IDictionary<TKey, List<TValue>> dict,
			[NotNull] TKey key,
			TValue value)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => new List<TValue>()).Add(value);
		}

		#endregion

		#region HashSet

		/// <summary>
		/// Add value to values set of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateHashSet<TKey, TValue>(
			[NotNull] this IDictionary<TKey, ICollection<TValue>> dict,
			[NotNull] TKey key,
			TValue value)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => new HashSet<TValue>()).Add(value);
		}

#if !LESSTHAN_NET40
		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateHashSet<TKey, TValue>(
			[NotNull] this IDictionary<TKey, ISet<TValue>> dict,
			[NotNull] TKey key,
			TValue value)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => new HashSet<TValue>()).Add(value);
		}
#endif

		/// <summary>
		/// Add value to values set of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateHashSet<TKey, TValue>(
			[NotNull] this IDictionary<TKey, HashSet<TValue>> dict,
			[NotNull] TKey key,
			TValue value)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => new HashSet<TValue>()).Add(value);
		}

		#endregion

		#region Collection

		/// <summary>
		/// Add value to values set of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateCollection<TKey, TValue>(
			[NotNull] this IDictionary<TKey, ICollection<TValue>> dict,
			[NotNull] TKey key,
			TValue value,
			Func<ICollection<TValue>> createCollection)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}

#if !LESSTHAN_NET40
		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateCollection<TKey, TValue>(
			[NotNull] this IDictionary<TKey, ISet<TValue>> dict,
			[NotNull] TKey key,
			TValue value,
			Func<ISet<TValue>> createCollection)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}
#endif

		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateCollection<TKey, TValue>(
			[NotNull] this IDictionary<TKey, IList<TValue>> dict,
			[NotNull] TKey key,
			TValue value,
			Func<IList<TValue>> createCollection)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (createCollection == null) throw new ArgumentNullException(nameof(createCollection));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}

		/// <summary>
		/// Add value to values set of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateCollection<TKey, TValue>(
			[NotNull] this IDictionary<TKey, List<TValue>> dict,
			[NotNull] TKey key,
			TValue value,
			[NotNull] Func<List<TValue>> createCollection)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (createCollection == null) throw new ArgumentNullException(nameof(createCollection));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}

		/// <summary>
		/// Add value to values set of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateCollection<TKey, TValue>(
			[NotNull] this IDictionary<TKey, HashSet<TValue>> dict,
			[NotNull] TKey key,
			TValue value,
			Func<HashSet<TValue>> createCollection)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (createCollection == null) throw new ArgumentNullException(nameof(createCollection));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}

		#endregion
	}
}
