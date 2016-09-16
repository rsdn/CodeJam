using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>CompositeRange internal helpers</summary>
	internal static class CompositeRangeInternal
	{
		#region The stub param to mark unsafe overloads
		/// <summary>The message for unsafe (no arg validation) method.</summary>
		internal const string SkipsArgValidationObsolete =
			"Marked as obsolete to emit warnings on incorrect usage. " +
				"Read comments on CompositeRangeInternal.RangesXyz constants before suppressing the warning.";

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
			[Obsolete(SkipsArgValidationObsolete)]
			NoEmptyRanges,
			/// <summary>Do not sort ranges.</summary>
			[Obsolete(SkipsArgValidationObsolete)]
			RangesAlreadySorted,
			/// <summary>No validation at all.</summary>
			[Obsolete(SkipsArgValidationObsolete)]
			NoEmptyRangesAlreadySortedAndMerged
		}
		#endregion

		#region CompositeRanbe Constants
		internal const string PrefixString = ": { ";
		internal const string SuffixString = " }";
		internal const string SeparatorString = "; ";
		#endregion

		#region Helpers
		internal static int InsertInSortedList<T>([NotNull] List<T> sortedList, T value) =>
			InsertInSortedList(sortedList, value, Comparer<T>.Default, false);

		internal static int InsertInSortedList<T>([NotNull] List<T> sortedList, T value, IComparer<T> comparer) =>
			InsertInSortedList(sortedList, value, comparer, false);

		internal static int InsertInSortedList<T>(
			[NotNull] List<T> sortedList, T value,
			[CanBeNull] IComparer<T> comparer, bool skipDuplicates)
		{
			int insertIndex = sortedList.BinarySearch(value, comparer);

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
