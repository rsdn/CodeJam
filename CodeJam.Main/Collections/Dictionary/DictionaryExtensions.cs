using System;
using System.Collections.Concurrent;
#if !LESSTHAN_NET45
using System.Threading.Tasks;
#endif
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Extensions for <see cref="IDictionary{TKey,TValue}"/>
	/// </summary>
	[PublicAPI]
	public static partial class DictionaryExtensions
	{
		#region GetOrAdd, AddOrUpdate
		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key of the element to add.</param>
		/// <returns>
		///   The value for the key. This will be either the existing value for the key if the key is already in the
		///   dictionary, or the new value if the key was not in the dictionary.
		/// </returns>
		[CollectionAccess(CollectionAccessType.Read | CollectionAccessType.UpdatedContent)]
		public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key)
			where TValue : new()
		{
			Code.NotNull(dictionary, nameof(dictionary));

			if (!dictionary.TryGetValue(key, out var result))
			{
				result = new TValue();
				dictionary.Add(key, result);
			}
			return result;
		}

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">the value to be added, if the key does not already exist</param>
		/// <returns>
		///   The value for the key. This will be either the existing value for the key if the key is already in the
		///   dictionary, or the new value if the key was not in the dictionary.
		/// </returns>
		[CollectionAccess(CollectionAccessType.Read | CollectionAccessType.UpdatedContent)]
		public static TValue GetOrAdd<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			TValue value)
		{
			Code.NotNull(dictionary, nameof(dictionary));

			if (!dictionary.TryGetValue(key, out var result))
			{
				result = value;
				dictionary.Add(key, result);
			}
			return result;
		}

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="valueFactory">The function used to generate a value for the key</param>
		/// <returns>
		///   The value for the key. This will be either the existing value for the key if the key is already in the
		///   dictionary, or the new value if the key was not in the dictionary.
		/// </returns>
		[CollectionAccess(CollectionAccessType.Read | CollectionAccessType.UpdatedContent)]
		public static TValue GetOrAdd<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			[NotNull, InstantHandle] Func<TKey, TValue> valueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(valueFactory, nameof(valueFactory));

			if (!dictionary.TryGetValue(key, out var result))
			{
				result = valueFactory(key);
				dictionary.Add(key, result);
			}
			return result;
		}

#if !LESSTHAN_NET45
		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="valueFactory">The function used to generate a value for the key</param>
		/// <returns>
		///   The value for the key. This will be either the existing value for the key if the key is already in the
		///   dictionary, or the new value if the key was not in the dictionary.
		/// </returns>
		[CollectionAccess(CollectionAccessType.Read | CollectionAccessType.UpdatedContent)]
		public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			[NotNull, InstantHandle] Func<TKey, Task<TValue>> valueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(valueFactory, nameof(valueFactory));

			if (!dictionary.TryGetValue(key, out var result))
			{
				result = await valueFactory(key);
				dictionary.Add(key, result);
			}
			return result;
		}
#endif

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="addValue">The value to be added for an absent key</param>
		/// <param name="updateValueFactory">
		/// The function used to generate a new value for an existing key based on the key's existing value
		/// </param>
		/// <returns>
		///   The new value for the key. This will be either be addValue (if the key was absent) or the result of
		///   updateValueFactory (if the key was present).
		/// </returns>
		[CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.UpdatedContent)]
		public static TValue AddOrUpdate<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			TValue addValue,
			[NotNull, InstantHandle] Func<TKey, TValue, TValue> updateValueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(updateValueFactory, nameof(updateValueFactory));

			if (dictionary.TryGetValue(key, out var result))
			{
				var newValue = updateValueFactory(key, result);
				dictionary[key] = newValue;
				return newValue;
			}
			dictionary.Add(key, addValue);
			return addValue;
		}

#if !LESSTHAN_NET45
		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="addValue">The value to be added for an absent key</param>
		/// <param name="updateValueFactory">
		/// The function used to generate a new value for an existing key based on the key's existing value
		/// </param>
		/// <returns>
		///   The new value for the key. This will be either be addValue (if the key was absent) or the result of
		///   updateValueFactory (if the key was present).
		/// </returns>
		[CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.UpdatedContent)]
		public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			TValue addValue,
			[NotNull, InstantHandle] Func<TKey, TValue, Task<TValue>> updateValueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(updateValueFactory, nameof(updateValueFactory));

			if (dictionary.TryGetValue(key, out var result))
			{
				var newValue = await updateValueFactory(key, result);
				dictionary[key] = newValue;
				return newValue;
			}
			dictionary.Add(key, addValue);
			return addValue;
		}
#endif

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="addValueFactory">The function used to generate a value for an absent key</param>
		/// <param name="updateValueFactory">
		/// The function used to generate a new value for an existing key based on the key's existing value
		/// </param>
		/// <returns>
		///   The new value for the key. This will be either be addValue (if the key was absent) or the result of
		///   updateValueFactory (if the key was present).
		/// </returns>
		[CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.UpdatedContent)]
		public static TValue AddOrUpdate<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			[NotNull, InstantHandle] Func<TKey, TValue> addValueFactory,
			[NotNull, InstantHandle] Func<TKey, TValue, TValue> updateValueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(addValueFactory, nameof(addValueFactory));
			Code.NotNull(updateValueFactory, nameof(updateValueFactory));

			if (dictionary.TryGetValue(key, out var result))
			{
				var newValue = updateValueFactory(key, result);
				dictionary[key] = newValue;
				return newValue;
			}
			var newAddValue = addValueFactory(key);
			dictionary.Add(key, newAddValue);
			return newAddValue;
		}

#if !LESSTHAN_NET45
		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="addValueFactory">The function used to generate a value for an absent key</param>
		/// <param name="updateValueFactory">
		/// The function used to generate a new value for an existing key based on the key's existing value
		/// </param>
		/// <returns>
		///   The new value for the key. This will be either be addValue (if the key was absent) or the result of
		///   updateValueFactory (if the key was present).
		/// </returns>
		[CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.UpdatedContent)]
		public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			[NotNull, InstantHandle] Func<TKey, Task<TValue>> addValueFactory,
			[NotNull, InstantHandle] Func<TKey, TValue, Task<TValue>> updateValueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(addValueFactory, nameof(addValueFactory));
			Code.NotNull(updateValueFactory, nameof(updateValueFactory));

			if (dictionary.TryGetValue(key, out var result))
			{
				var newValue = await updateValueFactory(key, result);
				dictionary[key] = newValue;
				return newValue;
			}
			var newAddValue = await addValueFactory(key);
			dictionary.Add(key, newAddValue);
			return newAddValue;
		}
#endif

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="valueFactory">The function used to generate a value.</param>
		/// <returns>The new value for the key.</returns>
		[CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.UpdatedContent)]
		public static TValue AddOrUpdate<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			[NotNull, InstantHandle] Func<TKey, TValue> valueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(valueFactory, nameof(valueFactory));

			if (dictionary.ContainsKey(key))
			{
				var newValue = valueFactory(key);
				dictionary[key] = newValue;
				return newValue;
			}
			var newAddValue = valueFactory(key);
			dictionary.Add(key, newAddValue);
			return newAddValue;
		}

#if !LESSTHAN_NET45
		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="valueFactory">The function used to generate a value.</param>
		/// <returns>The new value for the key.</returns>
		[CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.UpdatedContent)]
		public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			[NotNull, InstantHandle] Func<TKey, Task<TValue>> valueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(valueFactory, nameof(valueFactory));

			if (dictionary.ContainsKey(key))
			{
				var newValue = await valueFactory(key);
				dictionary[key] = newValue;
				return newValue;
			}
			var newAddValue = await valueFactory(key);
			dictionary.Add(key, newAddValue);
			return newAddValue;
		}
#endif

		/// <summary>
		///   Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="ConcurrentDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="valueFactory">The function used to generate a value.</param>
		/// <returns>The new value for the key.</returns>
		[CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.UpdatedContent)]
		public static TValue AddOrUpdate<TKey, TValue>(
			[NotNull] this ConcurrentDictionary<TKey, TValue> dictionary,
			[NotNull] TKey key,
			[NotNull, InstantHandle] Func<TKey, TValue> valueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			return dictionary.AddOrUpdate(key, valueFactory, (k, oldValue) => valueFactory(k));
		}

		#endregion
	}
}