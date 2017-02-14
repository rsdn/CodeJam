using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		#region MinOrDefault
		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <returns>Minimum item from the sequence.r default value.</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source) =>
				MinOrDefault(source, (IComparer<TSource>)null, default(TSource));

		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence.r default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, TSource defaultValue) =>
				MinOrDefault(source, (IComparer<TSource>)null, defaultValue);

		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Minimum item from the sequence.r default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull] IComparer<TSource> comparer) =>
				MinOrDefault(source, comparer, default(TSource));

		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence.r default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull]IComparer<TSource> comparer, TSource defaultValue)
		{
			Code.NotNull(source, nameof(source));
			comparer = comparer ?? Comparer<TSource>.Default;

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
		#endregion

		#region MinOrDefault with selector
		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>Minimum item from the sequence.r default value.</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, 
			[NotNull, InstantHandle] Func<TSource, T> selector) =>
				MinOrDefault(source, selector, null, default(T));

		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence.r default value</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector, T defaultValue) =>
				MinOrDefault(source, selector, null, defaultValue);

		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Minimum item from the sequence.r default value</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull] IComparer<T> comparer) =>
				MinOrDefault(source, selector, comparer, default(T));

		/// <summary>Returns minimum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Minimum item from the sequence.r default value</returns>
		[Pure]
		public static T MinOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector, 
			[CanBeNull]IComparer<T> comparer, T defaultValue)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(selector, nameof(selector));

			comparer = comparer ?? Comparer<T>.Default;

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
		#endregion

		#region MaxOrDefault
		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <returns>Maximum item from the sequence.r default value.</returns>
		[Pure]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source) =>
				MaxOrDefault(source, (IComparer<TSource>)null, default(TSource));

		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence.r default value.</returns>
		[Pure]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, TSource defaultValue) =>
				MaxOrDefault(source, (IComparer<TSource>)null, defaultValue);

		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Maximum item from the sequence.r default value</returns>
		[Pure]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull] IComparer<TSource> comparer) =>
				MaxOrDefault(source, comparer, default(TSource));

		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence.r default value</returns>
		[Pure]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull]IComparer<TSource> comparer, TSource defaultValue)
		{
			Code.NotNull(source, nameof(source));
			comparer = comparer ?? Comparer<TSource>.Default;

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
		#endregion

		#region MaxOrDefault with selector
		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>Maximum item from the sequence.r default value.</returns>
		[Pure]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector) =>
				MaxOrDefault(source, selector, null, default(T));

		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence.r default value</returns>
		[Pure]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector, T defaultValue) =>
				MaxOrDefault(source, selector, null, defaultValue);

		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Maximum item from the sequence.r default value</returns>
		[Pure]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull] IComparer<T> comparer) =>
				MaxOrDefault(source, selector, comparer, default(T));

		/// <summary>Returns maximum item from the sequence.r default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="source">The sequence.</param>
		/// <param name="selector">The value selector.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence.s empty.</param>
		/// <returns>Maximum item from the sequence.r default value</returns>
		[Pure]
		public static T MaxOrDefault<TSource, T>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source,
			[NotNull, InstantHandle] Func<TSource, T> selector,
			[CanBeNull]IComparer<T> comparer, T defaultValue)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(selector, nameof(selector));

			comparer = comparer ?? Comparer<T>.Default;

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
}