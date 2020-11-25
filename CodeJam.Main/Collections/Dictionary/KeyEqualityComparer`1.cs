using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// An implementation of the <see cref="IEqualityComparer{T}"/>
	/// interface for compare values by comparing their extracted key values.
	/// </summary>
	/// <typeparam name="T">The type of the comparing elements.</typeparam>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	[PublicAPI]
	public sealed class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>
	where TKey: notnull
	{
		/// <summary>
		/// Gets the function to extract the key for each element.
		/// </summary>
		/// <returns>
		/// The function to extract the key for each element.
		/// </returns>
		[NotNull]
		public Func<T?, TKey> KeySelector { get; }

		/// <summary>
		/// Gets the equality comparer to use to compare the keys.
		/// </summary>
		/// <returns>
		/// The equality comparer to use to compare the keys.
		/// </returns>
		[NotNull]
		public IEqualityComparer<TKey> Comparer { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyEqualityComparer{T,TKey}"/>.
		/// </summary>
		/// <param name="keySelector">The function to extract the key for each element.</param>
		public KeyEqualityComparer([NotNull] Func<T?, TKey> keySelector)
		{
			Code.NotNull(keySelector, nameof (keySelector));

			KeySelector = keySelector;
			Comparer = EqualityComparer<TKey>.Default;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyEqualityComparer{T,TKey}"/>.
		/// </summary>
		/// <param name="keySelector">The function to extract the key for each element.</param>
		/// <param name="comparer">The equality comparer to use to compare the keys.</param>
		public KeyEqualityComparer([NotNull] Func<T?, TKey> keySelector, IEqualityComparer<TKey>? comparer)
		{
			Code.NotNull(keySelector, nameof(keySelector));

			KeySelector = keySelector;
			Comparer = comparer ?? EqualityComparer<TKey>.Default;
		}

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <typeparamref name="T"/> to compare.</param>
		/// <param name="y">The second object of type <typeparamref name="T"/> to compare.</param>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(T? x, T? y) =>
			Comparer.Equals(KeySelector(x), KeySelector(y));

		/// <summary>
		/// Returns a hash code for the specified object.
		/// </summary>
		/// <returns>
		/// A hash code for the specified object.
		/// </returns>
		/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
		public int GetHashCode(T obj) =>
			Comparer.GetHashCode(KeySelector(obj));
	}
}