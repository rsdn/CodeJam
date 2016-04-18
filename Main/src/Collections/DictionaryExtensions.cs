using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Extensions for <see cref="IDictionary{TKey,TValue}"/>
	/// </summary>
	[PublicAPI]
	public static class DictionaryExtensions
	{
		#region GetValueOrDefault
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default(TValue) if key does not exists in
		/// <paramref name="dictionary"/>
		/// </summary>
		[Pure]
		public static TValue GetValueOrDefault<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: default(TValue);
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default(TValue) if key does not exists in
		/// <paramref name="dictionary"/>
		/// </summary>
		// Resolve ambiguity between IDictionary and IReadOnlyDictionary in System.Dictionary class.
		[Pure]
		public static TValue GetValueOrDefault<TKey, TValue>([NotNull] this Dictionary<TKey, TValue> dictionary, TKey key) =>
			GetValueOrDefault((IDictionary<TKey, TValue>)dictionary, key);

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default(TValue) if key does not exists in
		/// <paramref name="dictionary"/>
		/// </summary>
		[Pure]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[NotNull] this IReadOnlyDictionary<TKey, TValue> dictionary,
			TKey key)
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: default(TValue);
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		[Pure]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			TKey key,
			TValue defaultValue)
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValue;
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		[Pure]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			TKey key,
			[NotNull, InstantHandle] Func<TKey, TValue> defaultValueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValueFactory(key);
		}
		#endregion

		#region GetOrAdd, AddOrUpdate
		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist.
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">The key of the element to add.</param>
		/// <returns>
		///   The value for the key. This will be either the existing value for the key if the key is already in the
		///   dictionary, or the new value if the key was not in the dictionary.
		/// </returns>
		public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, TKey key)
			where TValue : new()
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			if (!dictionary.TryGetValue(key, out result))
			{
				result = new TValue();
				dictionary.Add(key, result);
			}
			return result;
		}

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist.
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">the value to be added, if the key does not already exist</param>
		/// <returns>
		///   The value for the key. This will be either the existing value for the key if the key is already in the
		///   dictionary, or the new value if the key was not in the dictionary.
		/// </returns>
		public static TValue GetOrAdd<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			TKey key,
			TValue value)
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			if (!dictionary.TryGetValue(key, out result))
			{
				result = value;
				dictionary.Add(key, result);
			}
			return result;
		}

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist.
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="valueFactory">The function used to generate a value for the key</param>
		/// <returns>
		///   The value for the key. This will be either the existing value for the key if the key is already in the
		///   dictionary, or the new value if the key was not in the dictionary.
		/// </returns>
		public static TValue GetOrAdd<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			TKey key,
			[NotNull, InstantHandle] Func<TKey, TValue> valueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(valueFactory, nameof(valueFactory));

			TValue result;
			if (!dictionary.TryGetValue(key, out result))
			{
				result = valueFactory(key);
				dictionary.Add(key, result);
			}
			return result;
		}

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="addValue">The value to be added for an absent key</param>
		/// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
		/// <returns>
		///   The new value for the key. This will be either be addValue (if the key was absent) or the result of
		///   updateValueFactory (if the key was present).
		/// </returns>
		public static TValue AddOrUpdate<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			TKey key,
			TValue addValue,
			[NotNull, InstantHandle] Func<TKey, TValue, TValue> updateValueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(updateValueFactory, nameof(updateValueFactory));

			TValue result;
			if (dictionary.TryGetValue(key, out result))
			{
				var newValue = updateValueFactory(key, result);
				dictionary[key] = newValue;
				return newValue;
			}
			dictionary.Add(key, addValue);
			return addValue;
		}

		/// <summary>
		///   Adds a key/value pair to the <see cref="IDictionary{TKey,TValue}"/> if the key does not already exist,
		///   or updates a key/value pair <see cref="IDictionary{TKey,TValue}"/> by using the specified function
		///   if the key already exists.
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="addValueFactory">The function used to generate a value for an absent key</param>
		/// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
		/// <returns>
		///   The new value for the key. This will be either be addValue (if the key was absent) or the result of
		///   updateValueFactory (if the key was present).
		/// </returns>
		public static TValue AddOrUpdate<TKey, TValue>(
			[NotNull] this IDictionary<TKey, TValue> dictionary,
			TKey key,
			[NotNull, InstantHandle] Func<TKey, TValue> addValueFactory,
			[NotNull, InstantHandle] Func<TKey, TValue, TValue> updateValueFactory)
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(addValueFactory, nameof(addValueFactory));
			Code.NotNull(updateValueFactory, nameof(updateValueFactory));

			TValue result;
			if (dictionary.TryGetValue(key, out result))
			{
				var newValue = updateValueFactory(key, result);
				dictionary[key] = newValue;
				return newValue;
			}
			var newAddValue = addValueFactory(key);
			dictionary.Add(key, newAddValue);
			return newAddValue;
		}
		#endregion
	}
}