using System.Collections.Generic;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace CodeJam.Collections.Backported
{
	/// <summary>
	/// Extensions for <see cref="IEnumerable{T}"/>
	/// </summary>
	[PublicAPI]
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
		/// <returns>
		/// A <see cref="HashSet{T}"/> that contains elements from the input sequence.
		/// </returns>
		[Pure, NotNull]
		public static HashSet<T> ToHashSet<T>([NotNull, InstantHandle] this IEnumerable<T> source) => new HashSet<T>(source);

		/// <summary>
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/> with the specified equality comparer.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use
		/// to comparing values in the set, or <c>null</c> to use the default implementation for the set type.</param>
		/// <returns>
		/// A <see cref="HashSet{T}"/> that contains elements from the input sequence.
		/// </returns>
		[Pure, NotNull]
		public static HashSet<T> ToHashSet<T>(
			[NotNull, InstantHandle] this IEnumerable<T> source,
			[NotNull] IEqualityComparer<T> comparer) =>
				new HashSet<T>(source, comparer);


		/// <summary>
		/// Prepends specified <paramref name="element"/> to the collection start.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="element">Element to prepend.</param>
		/// <returns>Concatenated enumerable</returns>
		[Pure, NotNull, LinqTunnel]
		public static IEnumerable<T> Prepend<T>([NotNull] this IEnumerable<T> source, T element)
		{
			Code.NotNull(source, nameof(source));

			return PrependCore(source, element);
		}

		[Pure, NotNull, LinqTunnel]
		private static IEnumerable<T> PrependCore<T>([NotNull] IEnumerable<T> source, T element)
		{
			yield return element;
			foreach (var item in source)
				yield return item;
		}
	}
}