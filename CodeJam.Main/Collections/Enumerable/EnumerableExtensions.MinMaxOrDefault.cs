﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		private static class MinMaxOperators<T>
		{
			private static readonly bool _hasNaN = Operators<T>.HasNaN;
			[NotNull]
			private static readonly Func<T, T, bool> _areNotEqual = Operators<T>.AreNotEqual;
			[NotNull]
			private static readonly Func<T, T, bool> _greaterThan = Operators<T>.GreaterThan;
			[NotNull]
			private static readonly Comparer<T> _comparer = Comparer<T>.Default;

			#region Operators<T>
			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MinOrDefault(IEnumerable<T> source, T defaultValue)
			{
				Code.NotNull(source, nameof(source));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = enumerator.Current;
					if (_hasNaN && areNotEqual(result, result))
						return result;

					while (enumerator.MoveNext())
					{
						var candidate = enumerator.Current;
						if (_hasNaN && areNotEqual(candidate, candidate))
							return candidate;

						if (candidate != null && (result == null || greaterThan(result, candidate)))
							result = candidate;
					}
					return result;
				}
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MinOrDefault<TSource>(IEnumerable<TSource> source, Func<TSource, T> selector, T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				Code.NotNull(selector, nameof(selector));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = selector(enumerator.Current);
					if (_hasNaN && areNotEqual(result, result))
						return result;

					while (enumerator.MoveNext())
					{
						var candidate = selector(enumerator.Current);
						if (_hasNaN && areNotEqual(candidate, candidate))
							return candidate;

						if (candidate != null && (result == null || greaterThan(result, candidate)))
							result = candidate;
					}
					return result;
				}
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MaxOrDefault(IEnumerable<T> source, T defaultValue)
			{
				Code.NotNull(source, nameof(source));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = enumerator.Current;
					while (enumerator.MoveNext())
					{
						var candidate = enumerator.Current;

						if (candidate != null)
						{
							var candidateIsNan = _hasNaN && areNotEqual(candidate, candidate);

							// DONTTOUCH: !greaterThan used for result NaN handling
							if (result == null || !candidateIsNan && !greaterThan(result, candidate))
							{
								result = candidate;
							}
						}
					}
					return result;
				}
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MaxOrDefault<TSource>(IEnumerable<TSource> source, Func<TSource, T> selector, T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				Code.NotNull(selector, nameof(selector));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = selector(enumerator.Current);
					while (enumerator.MoveNext())
					{
						var candidate = selector(enumerator.Current);

						if (candidate != null)
						{
							var candidateIsNan = _hasNaN && areNotEqual(candidate, candidate);

							// DONTTOUCH: !greaterThan used for result NaN handling
							if (result == null || !candidateIsNan && !greaterThan(result, candidate))
							{
								result = candidate;
							}
						}
					}
					return result;
				}
			}
			#endregion

			#region Comparer<T>
			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MinOrDefault(IEnumerable<T> source, IComparer<T> comparer, T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				comparer = comparer ?? _comparer;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = enumerator.Current;
					while (enumerator.MoveNext())
					{
						var candidate = enumerator.Current;
						if (candidate != null && (result == null || comparer.Compare(result, candidate) > 0))
							result = candidate;
					}
					return result;
				}
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MinOrDefault<TSource>(
				[NotNull] IEnumerable<TSource> source,
				[NotNull] Func<TSource, T> selector,
				[CanBeNull] IComparer<T> comparer,
				[CanBeNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				comparer = comparer ?? _comparer;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = selector(enumerator.Current);
					while (enumerator.MoveNext())
					{
						var candidate = selector(enumerator.Current);
						if (candidate != null && (result == null || comparer.Compare(result, candidate) > 0))
							result = candidate;
					}
					return result;
				}
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MaxOrDefault(
				[NotNull] IEnumerable<T> source,
				[CanBeNull] IComparer<T> comparer,
				[CanBeNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				comparer = comparer ?? _comparer;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = enumerator.Current;
					while (enumerator.MoveNext())
					{
						var candidate = enumerator.Current;
						if (candidate != null && (result == null || comparer.Compare(result, candidate) < 0))
							result = candidate;
					}
					return result;
				}
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static T MaxOrDefault<TSource>(
				[NotNull] IEnumerable<TSource> source,
				[NotNull] Func<TSource, T> selector,
				[CanBeNull] IComparer<T> comparer,
				[CanBeNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				comparer = comparer ?? _comparer;

				using (var enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
						return defaultValue;

					var result = selector(enumerator.Current);
					while (enumerator.MoveNext())
					{
						var candidate = selector(enumerator.Current);
						if (candidate != null && (result == null || comparer.Compare(result, candidate) < 0))
							result = candidate;
					}
					return result;
				}
			}
			#endregion
		}

		#region MinOrDefault
		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <returns>Minimum item from the sequence or default value.</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source) =>
				MinOrDefault(source, default(TSource));

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, TSource defaultValue) =>
				MinMaxOperators<TSource>.MinOrDefault(source, defaultValue);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull] IComparer<TSource> comparer) =>
				MinOrDefault(source, comparer, default);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[CanBeNull] IComparer<TSource> comparer,
			TSource defaultValue) =>
				MinMaxOperators<TSource>.MinOrDefault(source, comparer, defaultValue);
		#endregion

		#region MinOrDefault with selector
		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>Minimum item from the sequence or default value.</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector) =>
				MinOrDefault(source, selector, default(T));

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector, T defaultValue) =>
				MinMaxOperators<T>.MinOrDefault(source, selector, defaultValue);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull] IComparer<T> comparer) =>
				MinOrDefault(source, selector, comparer, default);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull] IComparer<T> comparer, T defaultValue) =>
				MinMaxOperators<T>.MinOrDefault(source, selector, comparer, defaultValue);
		#endregion

		#region MaxOrDefault
		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure, CanBeNull]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source) =>
				MaxOrDefault(source, default(TSource));

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure, CanBeNull]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, TSource defaultValue) =>
				MinMaxOperators<TSource>.MaxOrDefault(source, defaultValue);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, CanBeNull]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull] IComparer<TSource> comparer) =>
				MaxOrDefault(source, comparer, default);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, CanBeNull]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[CanBeNull] IComparer<TSource> comparer,
			TSource defaultValue) =>
				MinMaxOperators<TSource>.MaxOrDefault(source, comparer, defaultValue);
		#endregion

		#region MaxOrDefault with selector
		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure, CanBeNull]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector) =>
				MaxOrDefault(source, selector, default(T));

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector, T defaultValue) =>
				MinMaxOperators<T>.MaxOrDefault(source, selector, defaultValue);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, CanBeNull]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull] IComparer<T> comparer) =>
				MaxOrDefault(source, selector, comparer, default);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull] IComparer<T> comparer, T defaultValue) =>
				MinMaxOperators<T>.MaxOrDefault(source, selector, comparer, defaultValue);
		#endregion
	}
}