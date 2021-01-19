using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;

using static CodeJam.Targeting.MethodImplOptionsEx;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	public partial class EnumerableExtensions
	{
		private static class MinMaxOperators<T>
		{
			private static readonly bool _hasNaN = Operators<T>.HasNaN;
			private static readonly Func<T?, T?, bool> _areNotEqual = Operators<T>.AreNotEqual;
			private static readonly Func<T?, T?, bool> _greaterThan = Operators<T>.GreaterThan;
			private static readonly Comparer<T> _comparer = Comparer<T>.Default;

			#region Operators<T>
#pragma warning disable CA1508 // Avoid dead conditional code

			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MinOrDefault(IEnumerable<T> source, [AllowNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using var enumerator = source.GetEnumerator();
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

			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MinOrDefault<TSource>(
				IEnumerable<TSource> source,
				Func<TSource, T> selector,
				T? defaultValue)
			{
				Code.NotNull(source, nameof(source));
				Code.NotNull(selector, nameof(selector));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using var enumerator = source.GetEnumerator();
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

			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MaxOrDefault(
				IEnumerable<T> source,
				[AllowNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using var enumerator = source.GetEnumerator();
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

			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MaxOrDefault<TSource>(
				IEnumerable<TSource> source,
				Func<TSource, T> selector,
				T? defaultValue)
			{
				Code.NotNull(source, nameof(source));
				Code.NotNull(selector, nameof(selector));

				var greaterThan = _greaterThan;
				var areNotEqual = _areNotEqual;

				using var enumerator = source.GetEnumerator();
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
			#endregion

			#region Comparer<T>
			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MinOrDefault(
				IEnumerable<T> source,
				IComparer<T>? comparer,
				[AllowNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				comparer ??= _comparer;

				using var enumerator = source.GetEnumerator();
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

			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MinOrDefault<TSource>(
				IEnumerable<TSource> source,
				Func<TSource, T> selector,
				IComparer<T>? comparer,
				[AllowNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				Code.NotNull(selector, nameof(selector));

				comparer ??= _comparer;

				using var enumerator = source.GetEnumerator();
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

			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MaxOrDefault(
				IEnumerable<T> source,
				IComparer<T>? comparer,
				[AllowNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				comparer ??= _comparer;

				using var enumerator = source.GetEnumerator();
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

			[MethodImpl(AggressiveInlining)]
			[return: MaybeNull]
			public static T MaxOrDefault<TSource>(
				IEnumerable<TSource> source,
				Func<TSource, T> selector,
				IComparer<T>? comparer,
				[AllowNull] T defaultValue)
			{
				Code.NotNull(source, nameof(source));
				Code.NotNull(selector, nameof(selector));

				comparer ??= _comparer;

				using var enumerator = source.GetEnumerator();
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
#pragma warning restore CA1508 // Avoid dead conditional code

			#endregion
		}

#pragma warning restore CA1062 // Validate arguments of public methods

		#region MinOrDefault
		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <returns>Minimum item from the sequence or default value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source) =>
				MinOrDefault(source, default(TSource));

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			TSource? defaultValue) =>
				MinMaxOperators<TSource>.MinOrDefault(source, defaultValue);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			IComparer<TSource>? comparer) =>
				MinOrDefault(source, comparer, default);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MinOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			IComparer<TSource>? comparer,
			TSource? defaultValue) =>
				MinMaxOperators<TSource>.MinOrDefault(source, comparer, defaultValue);
		#endregion

		#region MinOrDefault with selector
		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>Minimum item from the sequence or default value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static T MinOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector) =>
				MinOrDefault(source, selector, default(T));

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static T MinOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector,
			T? defaultValue) =>
				MinMaxOperators<T>.MinOrDefault(source, selector, defaultValue);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static T MinOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector,
			IComparer<T>? comparer) =>
				MinOrDefault(source, selector, comparer, default);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static T MinOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector,
			IComparer<T>? comparer,
			T? defaultValue) =>
				MinMaxOperators<T>.MinOrDefault(source, selector, comparer, defaultValue);
		#endregion

		#region MaxOrDefault
		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source) =>
				MaxOrDefault(source, default(TSource));

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			TSource? defaultValue) =>
				MinMaxOperators<TSource>.MaxOrDefault(source, defaultValue);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source, IComparer<TSource>? comparer) =>
				MaxOrDefault(source, comparer, default);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static TSource MaxOrDefault<TSource>(
			[InstantHandle] this IEnumerable<TSource> source,
			IComparer<TSource>? comparer,
			TSource? defaultValue) =>
				MinMaxOperators<TSource>.MaxOrDefault(source, comparer, defaultValue);
		#endregion

		#region MaxOrDefault with selector
		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static T MaxOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector) =>
				MaxOrDefault(source, selector, default(T));

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		[return: MaybeNull]
		public static T MaxOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector,
			T? defaultValue) =>
				MinMaxOperators<T>.MaxOrDefault(source, selector, defaultValue);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T? MaxOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector,
			IComparer<T>? comparer) =>
				MaxOrDefault(source, selector, comparer, default);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T? MaxOrDefault<TSource, T>(
			[InstantHandle] this IEnumerable<TSource> source,
			[InstantHandle] Func<TSource, T> selector,
			IComparer<T>? comparer,
			T? defaultValue) =>
				MinMaxOperators<T>.MaxOrDefault(source, selector, comparer, defaultValue);
		#endregion

#pragma warning restore CA1062 // Validate arguments of public methods

	}
}