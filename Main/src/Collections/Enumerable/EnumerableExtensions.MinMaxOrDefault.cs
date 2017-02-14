using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		#region MinOrDefault
		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <returns>Minimum item from the sequence or default value.</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source) =>
				MinOrDefault(source, null, default(TSource));

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, TSource defaultValue) =>
				MinOrDefault(source, null, defaultValue);

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
		[Pure]
		public static TSource MinOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull] IComparer<TSource> comparer) =>
				MinOrDefault(source, comparer, default(TSource));

		/// <summary>Returns minimum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
		/// <returns>Minimum item from the sequence or default value</returns>
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


		#region MaxOrDefault
		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source) =>
				MaxOrDefault(source, null, default(TSource));

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
		/// <returns>Maximum item from the sequence or default value.</returns>
		[Pure]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, TSource defaultValue) =>
				MaxOrDefault(source, null, defaultValue);

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
		[Pure]
		public static TSource MaxOrDefault<TSource>(
			[NotNull, InstantHandle] this IEnumerable<TSource> source, [CanBeNull] IComparer<TSource> comparer) =>
				MaxOrDefault(source, comparer, default(TSource));

		/// <summary>Returns maximum item from the sequence or default value.</summary>
		/// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
		/// <param name="source">The sequence .</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
		/// <returns>Maximum item from the sequence or default value</returns>
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
	}
}