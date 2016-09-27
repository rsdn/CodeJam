using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>CompositeRange internal helpers</summary>
	internal static class CompositeRangeInternal
	{
		#region The stub param to mark unsafe overloads
		/// <summary>
		/// Helper enum to mark unsafe (no validation) constructor overloads.
		/// Should be used ONLY if the arguments are validated already
		/// AND the code is on the hotpath.
		/// </summary>
		internal enum UnsafeOverload
		{
			/// <summary>Threat passed ranges as usual.</summary>
			FullValidation,

			/// <summary>Do not check for empty ranges.</summary>
			NoEmptyRanges,

			/// <summary>Do not sort ranges.</summary>
			RangesAlreadySorted,

			/// <summary>No validation at all.</summary>
			NoEmptyRangesAlreadySortedAndMerged
		}
		#endregion

		#region CompositeRanbe Constants
		internal const string PrefixString = ": { ";
		internal const string SuffixString = " }";
		internal const string SeparatorString = "; ";
		#endregion

		#region Helpers
		/// <summary>Inserts the value into a sorted list.</summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="sortedList">The sorted list.</param>
		/// <param name="value">The value.</param>
		/// <returns>Index of the inserted item.</returns>
		internal static int InsertInSortedList<T>(
			[NotNull] List<T> sortedList,
			T value) =>
				InsertInSortedList(sortedList, value, Comparer<T>.Default, false);

		/// <summary>Inserts the value into a sorted list.</summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="sortedList">The sorted list.</param>
		/// <param name="value">The value.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>Index of the inserted item.</returns>
		internal static int InsertInSortedList<T>(
			[NotNull] List<T> sortedList,
			T value,
			IComparer<T> comparer) =>
				InsertInSortedList(sortedList, value, comparer, false);

		/// <summary>Inserts the value into a sorted list.</summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="sortedList">The sorted list.</param>
		/// <param name="value">The value.</param>
		/// <param name="skipDuplicates">If set to <c>true</c> duplicates are not inserted.</param>
		/// <returns>Index of the inserted item (or existing one if <paramref name="skipDuplicates"/> is set to <c>true</c>).</returns>
		internal static int InsertInSortedList<T>(
			[NotNull] List<T> sortedList,
			T value,
			bool skipDuplicates) =>
				InsertInSortedList(sortedList, value, null, skipDuplicates);

		/// <summary>Inserts the value into a sorted list.</summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="sortedList">The sorted list.</param>
		/// <param name="value">The value.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="skipDuplicates">If set to <c>true</c> duplicates are not inserted.</param>
		/// <returns>Index of the inserted item (or existing one if <paramref name="skipDuplicates"/> is set to <c>true</c>).</returns>
		internal static int InsertInSortedList<T>(
			[NotNull] List<T> sortedList,
			T value,
			[CanBeNull] IComparer<T> comparer,
			bool skipDuplicates)
		{
			var insertIndex = sortedList.BinarySearch(value, comparer);

			if (insertIndex < 0)
			{
				insertIndex = ~insertIndex;
				sortedList.Insert(insertIndex, value);
			}
			else if (!skipDuplicates)
			{
				sortedList.Insert(insertIndex, value);
			}
			return insertIndex;
		}
		#endregion
	}
}