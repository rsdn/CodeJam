﻿using System;
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
			this IDictionary<TKey, ICollection<TValue>> dict,
			TKey key,
			TValue value)
			where TKey : notnull
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
			this IDictionary<TKey, IList<TValue>> dict,
			TKey key,
			TValue value)
			where TKey : notnull
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
			this IDictionary<TKey, List<TValue>> dict,
			TKey key,
			TValue value)
			where TKey : notnull
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
			this IDictionary<TKey, ICollection<TValue>> dict,
			TKey key,
			TValue value)
			where TKey : notnull
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			dict.GetOrAdd(key, _ => new HashSet<TValue>()).Add(value);
		}

		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateHashSet<TKey, TValue>(
			this IDictionary<TKey, ISet<TValue>> dict,
			TKey key,
			TValue value)
			where TKey : notnull
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
			dict.GetOrAdd(key, _ => new HashSet<TValue>()).Add(value);
#elif NET35_OR_GREATER
			dict.GetOrAdd(key, _ => new HashSetEx<TValue>()).Add(value);
#else
			dict.GetOrAdd(key, _ => new HashSet<TValue>()).Add(value);
#endif
		}

		/// <summary>
		/// Add value to values set of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		public static void AddOrCreateHashSet<TKey, TValue>(
			this IDictionary<TKey, HashSet<TValue>> dict,
			TKey key,
			TValue value)
			where TKey : notnull
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
		/// <param name="createCollection">Create collection callback, called when key has no value.</param>
		public static void AddOrCreateCollection<TKey, TValue>(
			this IDictionary<TKey, ICollection<TValue>> dict,
			TKey key,
			TValue value,
			Func<ICollection<TValue>> createCollection)
			where TKey : notnull
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (createCollection == null) throw new ArgumentNullException(nameof(createCollection));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}

		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		/// <param name="createCollection">Create collection callback, called when key has no value.</param>
		public static void AddOrCreateCollection<TKey, TValue>(
			this IDictionary<TKey, ISet<TValue>> dict,
			TKey key,
			TValue value,
			Func<ISet<TValue>> createCollection)
			where TKey : notnull
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (createCollection == null) throw new ArgumentNullException(nameof(createCollection));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}

		/// <summary>
		/// Add value to values list of given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of the values in collection in the dictionary.</typeparam>
		/// <param name="dict">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value to be added to the collection mapped by the <paramref name="key"/></param>
		/// <param name="createCollection">Create collection callback, called when key has no value.</param>
		public static void AddOrCreateCollection<TKey, TValue>(
			this IDictionary<TKey, IList<TValue>> dict,
			TKey key,
			TValue value,
			Func<IList<TValue>> createCollection)
			where TKey : notnull
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
		/// <param name="createCollection">Create collection callback, called when key has no value.</param>
		public static void AddOrCreateCollection<TKey, TValue>(
			this IDictionary<TKey, List<TValue>> dict,
			TKey key,
			TValue value,
			Func<List<TValue>> createCollection)
			where TKey : notnull
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
		/// <param name="createCollection">Create collection callback, called when key has no value.</param>
		public static void AddOrCreateCollection<TKey, TValue>(
			this IDictionary<TKey, HashSet<TValue>> dict,
			TKey key,
			TValue value,
			Func<HashSet<TValue>> createCollection)
			where TKey : notnull
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (createCollection == null) throw new ArgumentNullException(nameof(createCollection));

			dict.GetOrAdd(key, _ => createCollection()).Add(value);
		}
		#endregion
	}
}