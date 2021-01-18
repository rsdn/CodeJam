using System.Collections.Generic;

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
		// Temporary comment unused internal methods
		///// <summary>Inserts the value into a sorted list.</summary>
		///// <typeparam name="T">Type of the value.</typeparam>
		///// <param name="sortedList">The sorted list.</param>
		///// <param name="value">The value.</param>
		///// <returns>Index of the inserted item.</returns>
		//internal static int InsertInSortedList<T>(
		//	List<T> sortedList,
		//	T value) =>
		//		InsertInSortedList(sortedList, value, Comparer<T>.Default, false);

		///// <summary>Inserts the value into a sorted list.</summary>
		///// <typeparam name="T">Type of the value.</typeparam>
		///// <param name="sortedList">The sorted list.</param>
		///// <param name="value">The value.</param>
		///// <param name="comparer">The comparer.</param>
		///// <returns>Index of the inserted item.</returns>
		//internal static int InsertInSortedList<T>(
		//	List<T> sortedList,
		//	T value,
		//	IComparer<T> comparer) =>
		//		InsertInSortedList(sortedList, value, comparer, false);

		///// <summary>Inserts the value into a sorted list.</summary>
		///// <typeparam name="T">Type of the value.</typeparam>
		///// <param name="sortedList">The sorted list.</param>
		///// <param name="value">The value.</param>
		///// <param name="skipDuplicates">If set to <c>true</c> duplicates are not inserted.</param>
		///// <returns>Index of the inserted item (or existing one if <paramref name="skipDuplicates"/> is set to <c>true</c>).</returns>
		//internal static int InsertInSortedList<T>(
		//	List<T> sortedList,
		//	T value,
		//	bool skipDuplicates) =>
		//		InsertInSortedList(sortedList, value, null, skipDuplicates);

		/// <summary>Inserts the value into a sorted list.</summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="sortedList">The sorted list.</param>
		/// <param name="value">The value.</param>
		/// <param name="comparer">The comparer.</param>
		/// <param name="skipDuplicates">If set to <c>true</c> duplicates are not inserted.</param>
		/// <returns>
		/// Index of the inserted item (or existing one if <paramref name="skipDuplicates"/> is set to <c>true</c>).
		/// </returns>
		// ReSharper disable once UnusedMethodReturnValue.Global
		internal static int InsertInSortedList<T>(
			List<T> sortedList,
			T? value,
			IComparer<T>? comparer,
			bool skipDuplicates)
		{
#pragma warning disable CS8604
			var insertIndex = sortedList.BinarySearch(value, comparer);
			// Bug in MS code markup. value can be null for ref types.
#pragma warning restore CS8604

			if (insertIndex < 0)
			{
				insertIndex = ~insertIndex;
#pragma warning disable CS8604
				sortedList.Insert(insertIndex, value); // Bug in MS code markup. value can be nul for ref types
#pragma warning restore CS8604
			}
			else if (!skipDuplicates)
			{
#pragma warning disable CS8604
				sortedList.Insert(insertIndex, value); // Bug in MS code markup. value can be nul for ref types
#pragma warning restore CS8604
			}
			return insertIndex;
		}
		#endregion
	}
}