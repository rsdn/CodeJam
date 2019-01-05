#if !LESSTHAN_NET35
using System;
using System.Collections.Generic;

using CodeJam.Ranges;

using JetBrains.Annotations;

namespace CodeJam.Dates
{
	/// <summary>
	/// Helper methods for date manipulations
	/// </summary>
	public static partial class DateTimeRangeExtensions
	{
		#region Range from date
		/// <summary>Returns date range for month.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Date range for month</returns>
		public static Range<DateTime> GetMonthRange(this DateTime date) =>
			Range.CreateExclusiveTo(date.FirstDayOfMonth(), date.FirstDayOfMonth().NextMonth());

		/// <summary>Returns date range for year.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Date range for year</returns>
		public static Range<DateTime> GetYearRange(this DateTime date) =>
			Range.CreateExclusiveTo(date.FirstDayOfYear(), date.FirstDayOfYear().NextYear());

		/// <summary>Gets date range from start of the year to the current date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Date range.</returns>
		public static Range<DateTime> GetFromStartOfYearRange(this DateTime date) =>
			Range.Create(date.FirstDayOfYear(), date);

		/// <summary>Gets date range from start of the year to the current date.</summary>
		/// <param name="date">The date.</param>
		/// <returns>Date range.</returns>
		public static Range<DateTime> GetToEndOfYearRange(this DateTime date) =>
			Range.CreateExclusiveTo(date, date.FirstDayOfYear().NextYear());
		#endregion

		#region Range manipulations
		/// <summary>Replaces exclusive boundaries with inclusive ones.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>A range with inclusive boundaries.</returns>
		public static Range<DateTime> MakeInclusive(this Range<DateTime> range) =>
			range.MakeInclusive(d => d.NextDay(), d => d.PrevDay());

		/// <summary>Replaces inclusive boundaries with exclusive ones.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>A range with inclusive boundaries.</returns>
		public static Range<DateTime> MakeExclusive(this Range<DateTime> range) =>
			range.MakeExclusive(d => d.PrevDay(), d => d.NextDay());
		#endregion

		#region Range Delta/Diff
		private static int DifferenceInDays(DateTime startDate, DateTime endDate) => (int)(endDate - startDate).TotalDays;

		/// <summary>Returns delta between two dates measured in days.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>Delta between two dates measured in days.</returns>
		public static int DifferenceInDays(this Range<DateTime> range)
		{
			range = range.MakeInclusive();
			return DifferenceInDays(range.FromValue, range.ToValue);
		}

		private static int DifferenceInMonths(DateTime startDate, DateTime endDate) =>
			// ReSharper disable once ArrangeRedundantParentheses
			(endDate.Month - startDate.Month) +
			(endDate.Year - startDate.Year) * 12;

		/// <summary>Returns count of days between two dates.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>Count of days between two dates.</returns>
		public static int CountOfDays(this Range<DateTime> range) => DifferenceInDays(range) + 1;

		/// <summary>Returns delta between two dates measured in months.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>Delta between two dates measured in months.</returns>
		public static int DifferenceInMonths(this Range<DateTime> range)
		{
			range = range.MakeInclusive();
			return DifferenceInMonths(range.FromValue, range.ToValue);
		}

		/// <summary>Returns count of month between two dates.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>Count of month between two dates.</returns>
		public static int CountOfMonths(this Range<DateTime> range) => DifferenceInMonths(range) + 1;
		#endregion

		#region Days between
		/// <summary>Returns first days in range.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>First days of months in range</returns>
		[NotNull]
		public static IEnumerable<DateTime> DaysBetween(this Range<DateTime> range)
		{
			range = range.MakeInclusive();
			if (range.IsEmpty)
				yield break;

			var startDate = range.FromValue;
			var endDate = range.ToValue;

			while (startDate <= endDate)
			{
				yield return startDate;
				startDate = startDate.NextDay();
			}
		}

		/// <summary>Returns first days of months in range.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>First days of months in range</returns>
		[NotNull]
		public static IEnumerable<DateTime> MonthsBetween(this Range<DateTime> range)
		{
			range = range.MakeInclusive();
			if (range.IsEmpty)
				yield break;

			var startDate = range.FromValue.FirstDayOfMonth();
			var endDate = range.ToValue;

			// if range.FromValue is not first date of month, the month is skipped.
			if (startDate < range.FromValue)
				startDate = startDate.NextMonth();
			while (startDate <= endDate)
			{
				yield return startDate;
				startDate = startDate.NextMonth();
			}
		}

		/// <summary>Returns first days of years in range.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>First days of years in range</returns>
		[NotNull]
		public static IEnumerable<DateTime> YearsBetween(this Range<DateTime> range)
		{
			range = range.MakeInclusive();
			if (range.IsEmpty)
				yield break;

			var startDate = range.FromValue.FirstDayOfYear();
			var endDate = range.ToValue;

			// if range.FromValue is not first date of year, the years is skipped.
			if (startDate < range.FromValue)
				startDate = startDate.NextYear();
			while (startDate <= endDate)
			{
				yield return startDate;
				startDate = startDate.NextYear();
			}
		}

		/// <summary>Splits the range by months.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>Ranges split by first day of months in range.</returns>
		[NotNull]
		public static IEnumerable<Range<DateTime>> SplitByMonths(this Range<DateTime> range)
		{
			if (range.IsEmpty)
				yield break;

			var startDate = range.From.Value;
			var lastMonthDate = range.To.Value.FirstDayOfMonth();

			while (startDate < lastMonthDate)
			{
				var next = startDate.FirstDayOfMonth().NextMonth();
				yield return Range.CreateExclusiveTo(startDate, next);
				startDate = next;
			}

			yield return Range.Create(startDate, range.To.Value);
		}

		/// <summary>Splits the range by years.</summary>
		/// <param name="range">The date range.</param>
		/// <returns>Ranges split by first day of years in range.</returns>
		[NotNull]
		public static IEnumerable<Range<DateTime>> SplitByYears(this Range<DateTime> range)
		{
			if (range.IsEmpty)
				yield break;

			var startDate = range.From.Value;
			var lastYearDate = range.To.Value.FirstDayOfYear();

			while (startDate < lastYearDate)
			{
				var next = startDate.FirstDayOfYear().NextYear();
				yield return Range.CreateExclusiveTo(startDate, next);
				startDate = next;
			}

			yield return Range.Create(startDate, range.To.Value);
		}
		#endregion
	}
}
#endif