using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Various collections extensions.
	/// </summary>
	[PublicAPI]
	public static class CollectionExtensions
	{
		/// <summary>
		/// Indicates whether the specified collection is <c>null</c> or empty.
		/// </summary>
		/// <typeparam name="T">Type of the collection values</typeparam>
		/// <param name="collection">The collection to test for emptiness.</param>
		/// <returns>
		/// <c>true</c>, if the <paramref name="collection"/> parameter is <c>null</c>
		/// or empty; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		[ContractAnnotation("collection:null => true")]
		public static bool IsNullOrEmpty<T>(this ICollection<T>? collection) =>
			collection == null || collection.Count == 0;

		/// <summary>
		/// Indicates whether the specified array is <c>null</c> or empty.
		/// </summary>
		/// <typeparam name="T">Type of the array values</typeparam>
		/// <param name="array">The array to test for emptiness.</param>
		/// <returns>
		/// <c>true</c>, if the <paramref name="array"/> parameter is <c>null</c>
		/// or empty; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		[ContractAnnotation("array:null => true")]
		public static bool IsNullOrEmpty<T>(this T[]? array)
		{
			// DONTTOUCH: Do not remove return statements
			// https://github.com/dotnet/coreclr/issues/914

			if (array == null || 0u >= (uint)array.Length)
				return true;

			return false;
		}

		/// <summary>
		/// Indicates whether the specified collection is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of the collection values</typeparam>
		/// <param name="collection">The collection to test.</param>
		/// <returns>
		/// <c>true</c>, if the <paramref name="collection"/> parameter is not null nor empty; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		[ContractAnnotation("collection:null => false")]
		public static bool NotNullNorEmpty<T>(this ICollection<T>? collection) =>
			collection != null && collection.Count != 0;

		/// <summary>
		/// Indicates whether the specified array is is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of the array values</typeparam>
		/// <param name="array">The array.</param>
		/// <returns>
		/// <c>true</c>, if the <paramref name="array"/> parameter is not null nor empty; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		[ContractAnnotation("array:null => false")]
		public static bool NotNullNorEmpty<T>(this T[] array) => !array.IsNullOrEmpty();

		/// <summary>
		/// Returns an empty instance of the collection for null values.
		/// </summary>
		/// <typeparam name="T">Type of the collection values</typeparam>
		/// <param name="source">The source.</param>
		/// <returns>The collection or empty instance if the collection is <c>null</c>.</returns>
		[Pure]
		[NotNull]
		[LinqTunnel]
		public static IEnumerable<T> EmptyIfNull<T>([CanBeNull] this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

		/// <summary>
		/// Returns an empty instance of the array for null values.
		/// </summary>
		/// <typeparam name="T">Type of the array values</typeparam>
		/// <param name="array">The array.</param>
		/// <returns>The array or empty instance if the array is <c>null</c>.</returns>
		[Pure]
		[NotNull]
		public static T[] EmptyIfNull<T>([CanBeNull] this T[]? array) => array ?? Array<T>.Empty;

		/// <summary>
		/// Returns an empty instance of the collection for null values.
		/// </summary>
		/// <typeparam name="T">Type of the collection values</typeparam>
		/// <param name="collection">The collection.</param>
		/// <returns>The collection or empty instance if the collection is <c>null</c>.</returns>
		[Pure]
		[NotNull]
		public static List<T> EmptyIfNull<T>([CanBeNull] this List<T>? collection) => collection ?? new List<T>();

		/// <summary>
		/// Returns an empty instance of the dictionary for null values.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <returns>The dictionary or <c>null</c> if the dictionary is <c>null</c>.</returns>
		[Pure]
		[NotNull]
		public static Dictionary<TKey, TValue> EmptyIfNull<TKey, TValue>(
			[CanBeNull] this Dictionary<TKey, TValue>? dictionary) where TKey : notnull =>
				dictionary ?? new Dictionary<TKey, TValue>();

		/// <summary>
		/// Returns an empty instance of the dictionary for null values.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>The dictionary or <c>null</c> if the dictionary is <c>null</c>.</returns>
		[Pure]
		[NotNull]
		public static Dictionary<TKey, TValue> EmptyIfNull<TKey, TValue>(
			[CanBeNull] this Dictionary<TKey, TValue>? dictionary,
			IEqualityComparer<TKey> comparer) where TKey : notnull => dictionary ?? new Dictionary<TKey, TValue>(comparer);

		/// <summary>
		/// Returns <c>null</c> if the collection is array.
		/// </summary>
		/// <typeparam name="T">Type of the array values</typeparam>
		/// <param name="array">The array.</param>
		/// <returns><c>null</c> if the array is empty.</returns>
		[Pure]
		[CanBeNull]
		public static T[]? NullIfEmpty<T>([CanBeNull] this T[]? array) => array.IsNullOrEmpty() ? null : array;

		/// <summary>
		/// Returns <c>null</c> if the collection is empty.
		/// </summary>
		/// <typeparam name="T">Type of the collection values</typeparam>
		/// <param name="collection">The collection.</param>
		/// <returns><c>null</c> if the collection is empty.</returns>
		[Pure]
		[CanBeNull]
		public static List<T>? NullIfEmpty<T>([CanBeNull] this List<T>? collection) =>
			collection.IsNullOrEmpty() ? null : collection;

		/// <summary>
		/// Returns <c>null</c> if the dictionary is empty.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <returns><c>null</c> if the dictionary is empty.</returns>
		[Pure]
		[CanBeNull]
		public static Dictionary<TKey, TValue>? NullIfEmpty<TKey, TValue>(
			[CanBeNull] this Dictionary<TKey, TValue>? dictionary) where TKey : notnull =>
				dictionary.IsNullOrEmpty() ? null : dictionary;

		/// <summary>
		/// Returns a new array with default value if the array is empty.
		/// </summary>
		/// <typeparam name="T">Type of the array values</typeparam>
		/// <param name="array">The array.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>A new array with default value if the array is empty.</returns>
		[Pure]
		[NotNull]
		public static T[] DefaultIfEmpty<T>([NotNull] this T[] array, T defaultValue)
		{
			Code.NotNull(array, nameof(array));
			return array.Length == 0
				? new[] { defaultValue }
				: array;
		}

		/// <summary>
		/// Returns a new collection with default value if the collection is empty.
		/// </summary>
		/// <typeparam name="T">Type of the collection values</typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>A new collection with default value if the collection is empty.</returns>
		[Pure]
		[NotNull]
		public static List<T> DefaultIfEmpty<T>([NotNull] this List<T> collection, T defaultValue)
		{
			Code.NotNull(collection, nameof(collection));
			return collection.Count == 0
				? new List<T> { defaultValue }
				: collection;
		}

		/// <summary>
		/// Returns a new dictionary with default value if the dictionary is empty.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="defaultKey">The default key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>
		/// A new dictionary with default key and value if the dictionary is empty.
		/// </returns>
		[Pure]
		[NotNull]
		public static Dictionary<TKey, TValue> DefaultIfEmpty<TKey, TValue>(
			[NotNull] this Dictionary<TKey, TValue> dictionary,
			TKey defaultKey,
			TValue defaultValue) where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			return dictionary.Count == 0
				? new Dictionary<TKey, TValue> { { defaultKey, defaultValue } }
				: dictionary;
		}

		/// <summary>
		/// Returns a new dictionary with default value if the dictionary is empty.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="defaultKey">The default key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>
		/// A new dictionary with default key and value if the dictionary is empty.
		/// </returns>
		[Pure]
		[NotNull]
		public static Dictionary<TKey, TValue> DefaultIfEmpty<TKey, TValue>(
			[NotNull] this Dictionary<TKey, TValue> dictionary,
			TKey defaultKey,
			TValue defaultValue,
			IEqualityComparer<TKey> comparer) where TKey : notnull
		{
			Code.NotNull(dictionary, nameof(dictionary));
			return dictionary.Count == 0
				? new Dictionary<TKey, TValue>(comparer) { { defaultKey, defaultValue } }
				: dictionary;
		}

		/// <summary>
		/// Adds the elements to the end of the <see cref="ICollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the items that the collection contains.</typeparam>
		/// <param name="source">The collection to add the elements to.</param>
		/// <param name="items">The items to add to the collection.</param>
		public static void AddRange<T>([NotNull] this ICollection<T> source, [NotNull] params T[] items)
		{
			foreach (var item in items)
				source.Add(item);
		}

		/// <summary>
		/// Adds the elements to the end of the <see cref="ICollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the items that the collection contains.</typeparam>
		/// <param name="source">The collection to add the elements to.</param>
		/// <param name="items">The items to add to the collection.</param>
		public static void AddRange<T>([NotNull] this ICollection<T> source, [NotNull] IList<T> items)
		{
			for (int i = 0, count = items.Count; i < count; i++)
				source.Add(items[i]);
		}

		/// <summary>
		/// Adds the elements to the end of the <see cref="ICollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the items that the collection contains.</typeparam>
		/// <param name="source">The collection to add the elements to.</param>
		/// <param name="items">The items to add to the collection.</param>
		public static void AddRange<T>([NotNull] this ICollection<T> source, [NotNull, InstantHandle] IEnumerable<T> items)
		{
			foreach (var item in items)
				source.Add(item);
		}
	}
}