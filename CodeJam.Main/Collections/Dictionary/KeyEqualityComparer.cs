using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Provides static methods for creating <see cref="KeyEqualityComparer{T,TKey}"/>.
	/// </summary>
	[PublicAPI]
	public static class KeyEqualityComparer
	{
		/// <summary>
		/// Creates a <see cref="KeyEqualityComparer{T,TKey}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the comparing elements.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The function to extract the key for each element.</param>
		/// <returns>
		/// A <see cref="KeyEqualityComparer{T,TKey}"/>.
		/// </returns>
		public static KeyEqualityComparer<T, TKey> Create<T, TKey>(Func<T?, TKey> keySelector)
			where TKey : notnull =>
				new(keySelector);

		/// <summary>
		/// Creates a <see cref="KeyEqualityComparer{T,TKey}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the comparing elements.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The function to extract the key for each element.</param>
		/// <param name="comparer">The equality comparer to use to compare the keys.</param>
		/// <returns>
		/// A <see cref="KeyEqualityComparer{T,TKey}"/>.
		/// </returns>
		public static KeyEqualityComparer<T, TKey> Create<T, TKey>(
			Func<T?, TKey> keySelector,
			IEqualityComparer<TKey>? comparer)
			where TKey : notnull =>
				new(keySelector, comparer);
	}
}