﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using JBNotNull = JetBrains.Annotations.NotNullAttribute;

namespace CodeJam.Collections
{
	partial class DictionaryExtensions
	{
		#region IDictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default value if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
				[JBNotNull] this IDictionary<TKey, TValue> dictionary,
				[JBNotNull] TKey key,
				[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, resultSelector, default(TResult));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this IDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[MaybeNull] TResult defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValue;
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this IDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValueFactory(key);
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this IDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[JBNotNull, InstantHandle] Func<TKey, TResult> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValueFactory(key);
		}
		#endregion

		#region IReadOnlyDictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default value if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
				[JBNotNull] this IReadOnlyDictionary<TKey, TValue> dictionary,
				[JBNotNull] TKey key,
				[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, resultSelector, default(TResult));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this IReadOnlyDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[MaybeNull] TResult defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValue;
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this IReadOnlyDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValueFactory(key);
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this IReadOnlyDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[JBNotNull, InstantHandle] Func<TKey, TResult> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValueFactory(key);
		}
		#endregion

		#region Dictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default value if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
				[JBNotNull] this Dictionary<TKey, TValue> dictionary,
				[JBNotNull] TKey key,
				[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, resultSelector, default(TResult));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this Dictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[MaybeNull] TResult defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValue;
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this Dictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValueFactory(key);
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this Dictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[JBNotNull, InstantHandle] Func<TKey, TResult> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValueFactory(key);
		}
		#endregion

		#region ConcurrentDictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default value if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
				[JBNotNull] this ConcurrentDictionary<TKey, TValue> dictionary,
				[JBNotNull] TKey key,
				[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, resultSelector, default(TResult));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this ConcurrentDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[MaybeNull] TResult defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValue;
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this ConcurrentDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValueFactory(key);
		}

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if key does not exists in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValueFactory">Function to return default value.</param>
		/// <param name="resultSelector">Function to select result.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or value returned by <paramref name="defaultValueFactory"/>
		/// if <paramref name="key"/> does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TResult GetValueOrDefault<TKey, TValue, TResult>(
			[JBNotNull] this ConcurrentDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[JBNotNull, InstantHandle] Func<TKey, TValue, TResult> resultSelector,
			[JBNotNull, InstantHandle] Func<TKey, TResult> defaultValueFactory)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			Code.NotNull(resultSelector, nameof(resultSelector));
			Code.NotNull(defaultValueFactory, nameof(defaultValueFactory));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? resultSelector(key, result)
					: defaultValueFactory(key);
		}
		#endregion

	}
}

namespace CodeJam.Collections.Backported
{
	/// <summary>
	/// Extensions for <see cref="IDictionary{TKey,TValue}"/>
	/// that are not included in previous FW versions
	/// </summary>
	public static class DictionaryExtensions
	{
		#region IDictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default(TValue) if key does not exists in
		/// <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/> does not exists
		/// in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>([JBNotNull] this IDictionary<TKey, TValue> dictionary, [JBNotNull] TKey key)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, default(TValue));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this IDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[MaybeNull] TValue defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValue;
		}

		#endregion

		#region IReadOnlyDictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default(TValue) if key does not exists in
		/// <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/> does not exists
		/// in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>([JBNotNull] this IReadOnlyDictionary<TKey, TValue> dictionary, [JBNotNull] TKey key)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, default(TValue));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this IReadOnlyDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[MaybeNull] TValue defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValue;
		}

		#endregion

		#region Dictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default(TValue) if key does not exists in
		/// <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/> does not exists
		/// in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>([JBNotNull] this Dictionary<TKey, TValue> dictionary, [JBNotNull] TKey key)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, default(TValue));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this Dictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[MaybeNull] TValue defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValue;
		}

		#endregion

		#region ConcurrentDictionary<TKey, TValue> overloads
		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or default(TValue) if key does not exists in
		/// <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or default value if <paramref name="key"/> does not exists
		/// in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>([JBNotNull] this ConcurrentDictionary<TKey, TValue> dictionary, [JBNotNull] TKey key)
            where TKey : notnull =>
			GetValueOrDefault(dictionary, key, default(TValue));

		/// <summary>
		/// Returns value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if key does not exists
		/// in <paramref name="dictionary"/>
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Value, associated with the <paramref name="key"/>, or <paramref name="defaultValue"/> if <paramref name="key"/>
		/// does not exists in <paramref name="dictionary"/>
		/// </returns>
		[Pure, CanBeNull]
		[CollectionAccess(CollectionAccessType.Read)]
		[return: MaybeNull]
		public static TValue GetValueOrDefault<TKey, TValue>(
			[JBNotNull] this ConcurrentDictionary<TKey, TValue> dictionary,
			[JBNotNull] TKey key,
			[MaybeNull] TValue defaultValue)
            where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));

			TValue result;
			return
				dictionary.TryGetValue(key, out result)
					? result
					: defaultValue;
		}

		#endregion

	}
}
