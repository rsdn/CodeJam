using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		private static Exception NoElementsException() => new InvalidOperationException("Collection has no elements");

		#region Min
		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with minimum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the minimum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <returns>The item with minimum value in the sequence.</returns>
		/// <exception cref="InvalidOperationException"><paramref name="source"/> has no not null elements</exception>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinBy<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector) =>
				MinBy(source, selector, Comparer<TValue>.Default);

		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with minimum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the minimum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <param name="defaultValue">Value returned if collection contains no not null elements.</param>
		/// <returns>
		/// The item with minimum value in the sequence or <typeparamref name="TSource"/> default value if
		/// <paramref name="source"/> has no not null elements.
		/// </returns>
		/// <exception cref="InvalidOperationException"><paramref name="source"/> has no not null elements</exception>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinByOrDefault<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector,
			TSource? defaultValue = default) =>
				MinByOrDefault(source, selector, Comparer<TValue>.Default, defaultValue);

		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with minimum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the minimum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
		/// <returns>The item with minimum value in the sequence.</returns>
		/// <exception cref="InvalidOperationException"><paramref name="source"/> has no not null elements</exception>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinBy<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector,
			IComparer<TValue>? comparer)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(selector, nameof(selector));

			comparer ??= Comparer<TValue>.Default;

			var value = default(TValue);
			TSource item;
			if (value == null)
			{
				using var e = source.GetEnumerator();
				do
				{
					if (!e.MoveNext())
						throw NoElementsException();

					value = selector(e.Current);
					item = e.Current;
				} while (value == null);

				while (e.MoveNext())
				{
					var x = selector(e.Current);
					if (x != null && comparer.Compare(x, value) < 0)
					{
						value = x;
						item = e.Current;
					}
				}
			}
			else
			{
				using var e = source.GetEnumerator();
				if (!e.MoveNext())
					return default!;

				value = selector(e.Current);
				item = e.Current;
				while (e.MoveNext())
				{
					var x = selector(e.Current);
#pragma warning disable CS8604
					if (comparer.Compare(x, value) < 0)
#pragma warning restore CS8604
					{
						value = x;
						item = e.Current;
					}
				}
			}

			return item;
		}

		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with minimum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the minimum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
		/// <param name="defaultValue">Value returned if collection contains no not null elements.</param>
		/// <returns>
		/// The item with minimum value in the sequence or <typeparamref name="TSource"/> default value if
		/// <paramref name="source"/> has no not null elements.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinByOrDefault<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector,
			IComparer<TValue>? comparer,
			TSource? defaultValue = default)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(selector, nameof(selector));

			comparer ??= Comparer<TValue>.Default;

			var value = default(TValue);
			TSource item;
			if (value == null)
			{
				using var e = source.GetEnumerator();
				do
				{
					if (!e.MoveNext())
						return defaultValue;

					value = selector(e.Current);
					item = e.Current;
				} while (value == null);

				while (e.MoveNext())
				{
					var x = selector(e.Current);
					if (x != null && comparer.Compare(x, value) < 0)
					{
						value = x;
						item = e.Current;
					}
				}
			}
			else
			{
				using var e = source.GetEnumerator();
				if (!e.MoveNext())
					return defaultValue!;

				value = selector(e.Current);
				item = e.Current;
				while (e.MoveNext())
				{
					var x = selector(e.Current);
#pragma warning disable CS8604
					if (comparer.Compare(x, value) < 0)
#pragma warning restore CS8604
					{
						value = x;
						item = e.Current;
					}
				}
			}

			return item;
		}
		#endregion

		#region Max
		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with maximum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the maximum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <returns>The item with maximum value in the sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxBy<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector) => MaxBy(source, selector, Comparer<TValue>.Default);

		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with maximum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the maximum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <param name="defaultValue">Value returned if collection contains no not null elements.</param>
		/// <returns>
		/// The item with maximum value in the sequence or <typeparamref name="TSource"/> default value if
		/// <paramref name="source"/> has no not null elements.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxByOrDefault<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector,
			TSource? defaultValue = default) =>
				MaxByOrDefault(source, selector, Comparer<TValue>.Default, defaultValue);

		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with maximum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the maximum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
		/// <returns>The item with maximum value in the sequence.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxBy<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector,
			IComparer<TValue>? comparer)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(selector, nameof(selector));

			comparer ??= Comparer<TValue>.Default;

			var value = default(TValue);
			TSource item;
			if (value == null)
			{
				using var e = source.GetEnumerator();
				do
				{
					if (!e.MoveNext())
						throw NoElementsException();

					value = selector(e.Current);
					item = e.Current;
				} while (value == null);

				while (e.MoveNext())
				{
					var x = selector(e.Current);
					if (x != null && comparer.Compare(x, value) > 0)
					{
						value = x;
						item = e.Current;
					}
				}
			}
			else
			{
				using var e = source.GetEnumerator();
				if (!e.MoveNext())
					return default!;

				value = selector(e.Current);
				item = e.Current;
				while (e.MoveNext())
				{
					var x = selector(e.Current);
					if (comparer.Compare(x, value) > 0)
					{
						value = x;
						item = e.Current;
					}
				}
			}

			return item;
		}

		/// <summary>
		/// Invokes a <paramref name="selector"/> on each element of a <paramref name="source"/>
		/// and returns the item with maximum value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="source">A sequence of values to determine the maximum value of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
		/// <param name="defaultValue">Value returned if collection contains no not null elements.</param>
		/// <returns>
		/// The item with maximum value in the sequence or <typeparamref name="TSource"/> default value if
		/// <paramref name="source"/> has no not null elements.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxByOrDefault<TSource, TValue>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, TValue> selector,
			IComparer<TValue>? comparer,
			TSource? defaultValue = default)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(selector, nameof(selector));

			comparer ??= Comparer<TValue>.Default;

			var value = default(TValue);
			TSource item;
			if (value == null)
			{
				using var e = source.GetEnumerator();
				do
				{
					if (!e.MoveNext())
						return defaultValue;

					value = selector(e.Current);
					item = e.Current;
				} while (value == null);

				while (e.MoveNext())
				{
					var x = selector(e.Current);
					if (x != null && comparer.Compare(x, value) > 0)
					{
						value = x;
						item = e.Current;
					}
				}
			}
			else
			{
				using var e = source.GetEnumerator();
				if (!e.MoveNext())
					return defaultValue!;

				value = selector(e.Current);
				item = e.Current;
				while (e.MoveNext())
				{
					var x = selector(e.Current);
#pragma warning disable CS8604
					if (comparer.Compare(x, value) > 0)
#pragma warning restore CS8604
					{
						value = x;
						item = e.Current;
					}
				}
			}

			return item;
		}
		#endregion
	}
}